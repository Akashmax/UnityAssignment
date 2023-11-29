using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class BonusGameController : MonoBehaviour
{
    public TextMeshProUGUI outcomeText;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI DisplayRounds;
    public TextMeshProUGUI OutComePlayerRed;
    public TextMeshProUGUI OutComePlayerBlue;

    public GameObject gridPicesPtrefab;
    public GameObject gridRedParent;
    public GameObject gridBlueParent;
    public GameObject gamepage;
    public GameObject FinalResult;
    private int totalRounds = 10;
    private int gridSizeX = 5;
    private int gridSizeY = 4;

    private GameObject[,] player1Grid;
    private GameObject[,] player2Grid;

    void Start()
    {
        player1Grid = InitializeGrid("Player1", Color.red, gridRedParent);
        player2Grid = InitializeGrid("Player2", Color.blue, gridBlueParent);
    }


    public void startpage()
    {
        gamepage.SetActive(true);
       StartCoroutine(PlayBonusRounds());
    }

    GameObject[,] InitializeGrid(string playerName, Color playerColor, GameObject gridParent)
    {
        GameObject[,] grid = new GameObject[gridSizeX, gridSizeY];

        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                // Instantiate the grid cell prefab
                GameObject gridCell = Instantiate(gridPicesPtrefab, Vector3.zero, Quaternion.identity);

                // Set the parent of the grid cell to the gridParent
                gridCell.transform.SetParent(gridParent.transform, false); // Make sure to set worldPositionStays to false

                // Customize the appearance based on player
                Image imageComponent = gridCell.GetComponent<Image>();
                if (imageComponent != null)
                {
                    imageComponent.color = playerColor;
                }
               
                gridCell.transform.GetChild(0).name = "2";
                /*  // Add a TextMeshProUGUI component to display the score
                  TextMeshProUGUI scoreText = gridCell.AddComponent<TextMeshProUGUI>();
                  scoreText.text = "2"; // Default score*/
                float randomValue = Random.value;

                if (randomValue <= 0.35f)
                {
                    // BLANK with 35% chance
                    gridCell.name = "BLANK";
                    gridCell.transform.GetChild(0).name = "0"; // Assuming 0 represents BLANK
                }
                else if (randomValue <= 0.60f)
                {
                    // STEAL with 25% chance
                    gridCell.name = "STEAL";
                    gridCell.transform.GetChild(0).name = "1"; // Assuming 1 represents STEAL
                }
                else if (randomValue <= 0.90f)
                {
                    // BOOST with 30% chance
                    gridCell.name = "BOOST";
                    gridCell.transform.GetChild(0).name = "2"; // Assuming 2 represents BOOST
                }
                else
                {
                    // SHIELD with 10% chance
                    gridCell.name = "SHIELD";
                    gridCell.transform.GetChild(0).name = "3"; // Assuming 3 represents SHIELD
                }
                grid[i, j] = gridCell;
            }
        }

        return grid;
    }


 
    IEnumerator<WaitForSeconds> PlayBonusRounds()
    {
        for (int round = 1; round <= totalRounds; round++)
        {
            DisplayOutcome($"----- Round {round} -----");

            yield return new WaitForSeconds(0.5f);
            SimulatePlayerOutcome("Player 1", player1Grid, OutComePlayerRed);
            yield return new WaitForSeconds(0.8f);
            SimulatePlayerOutcome("Player 2", player2Grid, OutComePlayerBlue);

         
            yield return new WaitForSeconds(1.5f);
            DisplayPlayerGrid("Player 2", player2Grid);
            DisplayPlayerGrid("Player 1", player1Grid);

            yield return new WaitForSeconds(3f);
            DisplayRounds.text = "Rounds Left " + (totalRounds - (round));

        }
      
        DisplayWinner();
        DisplayBanner();
    }

    private void DisplayBanner()
    {
        gamepage.SetActive(false);
        FinalResult.SetActive(true);
        FinalResult.transform.localScale = Vector3.zero;
        FinalResult.transform.DOScale(Vector3.one * 0.8f, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                FinalResult.transform.DOScale(Vector3.one, 0.5f)
                    .SetEase(Ease.OutBack);
            });
    }

    public void ReloadCurrentScene()
    {
   
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    void SimulatePlayerOutcome(string playerName, GameObject[,] playerGrid,TextMeshProUGUI OutComeresult)
    {
        int numZones = 2;
        string outcome = SimulateOutcome();
        OutComeresult.text = outcome;

        switch (outcome)
        {
            case "STEAL":
                Steal(playerName, playerGrid, numZones);
                break;
            case "BOOST":
                Boost(playerName, playerGrid, numZones);
                break;
            case "SHIELD":
                Shield(playerName, playerGrid, numZones);
                break;
            case "BLANK":
                // No Action is performed.
                break;
        }
     
        DisplayOutcome($"{playerName}: {outcome}");
    }

    string SimulateOutcome()
    {
        float randomValue = Random.value;

        if (randomValue <= 0.35f)
        {
            return "BLANK";
        }
        else if (randomValue <= 0.60f)
        {
            return "STEAL";
        }
        else if (randomValue <= 0.90f)
        {
            return "BOOST";
        }
        else
        {
            return "SHIELD";
        }
    }

    void DisplayOutcome(string text)
    {
        outcomeText.text += $"{text}\n";
    }



    void DisplayPlayerGrid(string playerName, GameObject[,] playerGrid)
    {

        for (int i = 0; i < playerGrid.GetLength(0); i++)
        {
            for (int j = 0; j < playerGrid.GetLength(1); j++)
            {
                GameObject gridCell = playerGrid[i, j];
                gridCell.transform.localScale = Vector3.zero;
                gridCell.transform.DOScale(Vector3.one, 0.5f)
                    .SetDelay(0.05f * (i * playerGrid.GetLength(1) + j))
                    .SetEase(Ease.OutBack);
            }
        }
    }


    void DisplayWinner()
    {
        int player1ControlledZones = CountControlledZones(player1Grid);
        int player2ControlledZones = CountControlledZones(player2Grid);

        int player1Score = CalculatePlayerScore(player1Grid);
        int player2Score = CalculatePlayerScore(player2Grid);


        Debug.Log(player1Score+""+player2Score);

        Debug.Log(player1ControlledZones + "" + player2ControlledZones);

        if (player1ControlledZones > player2ControlledZones)
        {
            DisplayBanner("Player 1 Wins!");
        }
        else if (player2ControlledZones > player1ControlledZones)
        {
            DisplayBanner("Player 2 Wins!");
        }
        else
        {
            if (player1Score > player2Score)
            {
                DisplayBanner("Player 1 Wins!");
            }
            else if (player2Score > player1Score)
            {
                DisplayBanner("Player 2 Wins!");
            }
            else
            {
                DisplayBanner("It's a Tie!");
            }
        }

        DisplayOutcome($"Player 1 Score: {player1Score}\nPlayer 2 Score: {player2Score}");
    }

    void DisplayBanner(string text)
    {
        winnerText.text = text;
        Debug.Log(text);
    }



    private  int CountControlledZones(GameObject[,] playerGrid)
    {
        int count = 0;

        for (int i = 0; i < playerGrid.GetLength(0); i++)
        {
            for (int j = 0; j < playerGrid.GetLength(1); j++)
            {
                if (playerGrid[i, j].transform.GetChild(0).name == "2")
                {
                    count++;
                }
            }
        }

        return count;
    }

    private  int CalculatePlayerScore(GameObject[,] playerGrid)
    {
        int score = 0;

        for (int i = 0; i < playerGrid.GetLength(0); i++)
        {
            for (int j = 0; j < playerGrid.GetLength(1); j++)
            {
                score += int.Parse((playerGrid[i, j].transform.GetChild(0).name));
            }
        }

        return score;
    }




    private void Steal(string playerName, GameObject[,] playerGrid, int numZones)
    {

        List<Vector2Int> targetZones = GetTargetZones(playerName, playerGrid, numZones);
        if (targetZones.Count > 0 && HasNonShieldedZones(playerName, playerGrid))
        {
            List<Vector2Int> stolenZones = GetRandomZones(targetZones, numZones);
            foreach (var zone in stolenZones)
            {
                Transform zoneTransform = playerGrid[zone.x, zone.y].transform;
                TextMeshProUGUI textEffect = zoneTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (textEffect != null)
                {
                    textEffect.text = "Stolen!";
                    textEffect.DOFade(0, 0.5f).From().SetEase(Ease.OutQuad);
                }
                zoneTransform.GetChild(0).name = "1";
            }
        }
    }

    private void Boost(string playerName, GameObject[,] playerGrid, int numZones)
    {
        List<Vector2Int> targetZones = GetTargetZones(playerName, playerGrid, numZones);
        if (targetZones.Count > 0)
        {
            List<Vector2Int> boostedZones = GetRandomZones(targetZones, numZones);
            foreach (var zone in boostedZones)
            {
                Transform zoneTransform = playerGrid[zone.x, zone.y].transform;
                TextMeshProUGUI textEffect = zoneTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (textEffect != null)
                {
                    textEffect.text = $"Boosted! x{GetRandomBoostAmount()}";
                    float originalScale = zoneTransform.localScale.x;
                    float targetScale = originalScale * 1.5f;
                    zoneTransform.DOScale(targetScale, 0.5f)
                        .OnComplete(() =>
                        {
                            zoneTransform.DOScale(originalScale, 0.5f);
                        });
                }
                int currentScore = int.Parse(zoneTransform.GetChild(0).name);
                int boostAmount = GetRandomBoostAmount();
                zoneTransform.GetChild(0).name = (currentScore * boostAmount).ToString();
            }
        }
    }

    private void Shield(string playerName, GameObject[,] playerGrid, int numZones)
    {
    
        List<Vector2Int> targetZones = GetTargetZones(playerName, playerGrid, numZones);

    
        if (targetZones.Count > 0)
        {
     
            List<Vector2Int> shieldedZones = GetRandomZones(targetZones, numZones);
            foreach (var zone in shieldedZones)
            {
                Transform zoneTransform = playerGrid[zone.x, zone.y].transform;
                TextMeshProUGUI textEffect = zoneTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (textEffect != null)
                {
                    textEffect.text = "Shielded!";
                }
                Color originalColor = zoneTransform.GetComponent<Image>().color;
                Color shieldColor = Color.gray; 
                zoneTransform.GetComponent<Image>().DOColor(shieldColor, 0.5f)
                    .OnComplete(() =>
                    {
                        zoneTransform.GetComponent<Image>().DOColor(originalColor, 0.2f);
                    });
                zoneTransform.GetChild(0).name = "3";
            }
        }
    }


    private List<Vector2Int> GetTargetZones(string playerName, GameObject[,] playerGrid, int numZones)
    {
        List<Vector2Int> targetZones = new List<Vector2Int>();

        for (int i = 0; i < playerGrid.GetLength(0); i++)
        {
            for (int j = 0; j < playerGrid.GetLength(1); j++)
            {
                if (playerGrid[i, j].transform.GetChild(0).name == "1" && playerGrid[i, j].transform.GetChild(0).name != "2") // Assuming 1 represents the current player's control, and 2 represents a shielded zone
                {
                    targetZones.Add(new Vector2Int(i, j));
                }
            }
        }

        return targetZones;
    }

    private bool HasNonShieldedZones(string playerName, GameObject[,] playerGrid)
    {
        for (int i = 0; i < playerGrid.GetLength(0); i++)
        {
            for (int j = 0; j < playerGrid.GetLength(1); j++)
            {
                if (playerGrid[i, j].transform.GetChild(0).name == "1" && playerGrid[i, j].transform.GetChild(0).name != "2") // Assuming 1 represents the current player's control, and 2 represents a shielded zone
                {
                    return true;
                }
            }
        }
        return false;
    }

    private List<Vector2Int> GetRandomZones(List<Vector2Int> zones, int numZones)
    {
        List<Vector2Int> randomZones = new List<Vector2Int>();
        zones = ShuffleList(zones);
        for (int i = 0; i < Mathf.Min(numZones, zones.Count); i++)
        {
            randomZones.Add(zones[i]);
        }
        return randomZones;
    }

    private int GetRandomBoostAmount()
    {
        float randomValue = Random.value;

        if (randomValue <= 0.35f)
        {
            return 2;
        }
        else if (randomValue <= 0.65f)
        {
            return 3;
        }
        else if (randomValue <= 0.90f)
        {
            return 5;
        }
        else
        {
            return 10;
        }
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
