
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using System.Text;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI simonScore;
    [SerializeField] private TextMeshProUGUI shootingScore;
    [SerializeField] private TextMeshProUGUI mazeScore;

    [Header("Game References")]
    [SerializeField] private SimonScores sm_scr;
    [SerializeField] private ObjectSpawner_1place objectSpawner;
    [SerializeField] private ButtonsForMaze maze_scr;
    [SerializeField] private Gun gunscript;

    private SQLiteDBScript dbManager;
    private List<double> shootingTimes = new List<double>();
    private List<string> hitPositions = new List<string>();
    private double bestTime = double.MaxValue;
    private int bestTimeIndex = -1;

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();

    void Awake()
    {
        dbManager = FindObjectOfType<SQLiteDBScript>();
        ResetScoresBasedOnLevel();
    }

    void Start()
    {
        InitializeUITexts();
        UpdateAllScores();

        // Debug log - ellenõrizzük, hogy a ScoreManager megtalálta-e az összes szükséges komponenst
        Debug.Log($"ScoreManager initialized with: objectSpawner={objectSpawner != null}, gunscript={gunscript != null}, dbManager={dbManager != null}");
    }

    private void ResetScoresBasedOnLevel()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        // A jelenlegi szint elõtti játékok eredményeit megtartjuk,
        // a mostani és következõ játékok eredményeit nullázzuk

        if (currentLevel <= 0) // Simon játék
        {
            if (sm_scr != null)
                sm_scr.ResetScore();
        }

        if (currentLevel <= 1) // Labirintus
        {
            if (maze_scr != null)
                maze_scr.ResetScore();
        }

        if (currentLevel <= 2) // Lövöldözõs játék
        {
            if (objectSpawner != null)
            {
                objectSpawner.hit_times?.Clear();
                objectSpawner.hitPlace_fromMiddle?.Clear();
            }
            shootingTimes.Clear();
            hitPositions.Clear();
            bestTime = double.MaxValue;
            bestTimeIndex = -1;
            hitpoints.Clear();
        }
    }

    private void InitializeUITexts()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        if (shootingScore != null)
            shootingScore.text = currentLevel > 2 ? shootingScore.text : "No hits recorded yet";

        if (simonScore != null)
            simonScore.text = currentLevel > 0 ? simonScore.text : "Simon Score: 0";

        if (mazeScore != null)
            mazeScore.text = currentLevel > 1 ? mazeScore.text : "Maze Score: 0";
    }

    void Update()
    {
        if (objectSpawner != null && objectSpawner.hit_times != null && shootingScore != null)
        {
            // Ellenõrizzük, hogy változott-e a találatok száma
            if (objectSpawner.hit_times.Count != shootingTimes.Count)
            {
                Debug.Log($"Hit times count changed: {objectSpawner.hit_times.Count} vs {shootingTimes.Count}");
                UpdateShootingScores();
            }

            // Ellenõrizzük a hitPlace_fromMiddle listát is
            if (objectSpawner.hitPlace_fromMiddle != null &&
                objectSpawner.hitPlace_fromMiddle.Count != hitPositions.Count)
            {
                Debug.Log($"Hit positions count changed: {objectSpawner.hitPlace_fromMiddle.Count} vs {hitPositions.Count}");
                UpdateShootingScores();
            }
        }
    }

    private void UpdateAllScores()
    {
        UpdateSimonScore();
        UpdateMazeScore();
        UpdateShootingScores();
    }

    private void UpdateSimonScore()
    {
        if (simonScore != null && sm_scr != null)
        {
            int currentScore = sm_scr.GetHighScore();
            simonScore.SetText($"Simon High Score: {currentScore}");

            // Adatbázis frissítése csak ha teljesítette a játékot
            if (dbManager != null && FindObjectOfType<SimonGameManager>().isEnded)
            {
                dbManager.UpdateSimonScore(currentScore);
            }
        }
    }

    private void UpdateMazeScore()
    {
        if (mazeScore != null && maze_scr != null)
        {
            double currentTime = maze_scr.score_time.TotalMinutes + ((maze_scr.score_time.Seconds % 60) / 100.0);
            mazeScore.SetText($"Maze Score: {currentTime}");

            // Adatbázis frissítése csak ha van érvényes idõ
            if (dbManager != null && currentTime > 0)
            {
                dbManager.UpdateMazeTime(currentTime);
            }
        }
    }

    private void UpdateShootingScores()
    {
        if (objectSpawner == null || shootingScore == null)
        {
            Debug.LogWarning("UpdateShootingScores: objectSpawner or shootingScore is null");
            return;
        }

        try
        {
            // Adatok másolása és ellenõrzése
            if (objectSpawner.hit_times != null)
            {
                shootingTimes = new List<double>(objectSpawner.hit_times);
                Debug.Log($"Copied {shootingTimes.Count} hit times from objectSpawner");
            }
            else
            {
                Debug.LogError("objectSpawner.hit_times is null!");
            }

            if (objectSpawner.hitPlace_fromMiddle != null)
            {
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
                Debug.Log($"Copied {hitPositions.Count} hit positions from objectSpawner");

                // Debug: Minden pozíció kiírása
                for (int i = 0; i < hitPositions.Count; i++)
                {
                    Debug.Log($"Hit position {i}: {hitPositions[i]}");
                }
            }
            else
            {
                Debug.LogError("objectSpawner.hitPlace_fromMiddle is null!");
            }

            if (shootingTimes.Count == 0)
            {
                shootingScore.SetText("No hits recorded yet");
                return;
            }

            double currentBestTime = shootingTimes.Min();
            if (currentBestTime < bestTime)
            {
                bestTime = currentBestTime;
                bestTimeIndex = shootingTimes.IndexOf(bestTime);
                Debug.Log($"New best time: {bestTime} at index {bestTimeIndex}");
            }

            // Format the display text with proper position values
            StringBuilder displayText = new StringBuilder();
            displayText.AppendLine($"Best Shot:");
            displayText.AppendLine($"Time: {bestTime:F2} sec");

            if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
            {
                displayText.AppendLine($"Position: {hitPositions[bestTimeIndex]}");
                Debug.Log($"Best shot position: {hitPositions[bestTimeIndex]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
                Debug.LogWarning($"Best shot position unavailable. bestTimeIndex={bestTimeIndex}, hitPositions.Count={hitPositions.Count}");
            }

            displayText.AppendLine("\nLast Shot:");
            displayText.AppendLine($"Time: {shootingTimes[shootingTimes.Count - 1]:F2} sec");

            if (hitPositions.Count > 0)
            {
                displayText.AppendLine($"Position: {hitPositions[hitPositions.Count - 1]}");
                Debug.Log($"Last shot position: {hitPositions[hitPositions.Count - 1]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
                Debug.LogWarning("Last shot position unavailable. hitPositions is empty.");
            }

            displayText.AppendLine($"\nTotal Hits: {shootingTimes.Count}");

            shootingScore.SetText(displayText.ToString());
            Debug.Log($"Updated shooting score text: {shootingScore.text}");

            // Update database with each shot
            if (dbManager != null)
            {
                for (int i = 0; i < shootingTimes.Count; i++)
                {
                    if (i < hitPositions.Count)
                    {
                        string[] coordinates = hitPositions[i].Split('|');
                        if (coordinates.Length == 2)
                        {
                            if (double.TryParse(coordinates[0], out double hitX) &&
                                double.TryParse(coordinates[1], out double hitY))
                            {
                                Debug.Log($"Sending to database: shot {i + 1}, time={shootingTimes[i]}, X={hitX}, Y={hitY}");
                                dbManager.UpdateShootingScore(i + 1, shootingTimes[i], hitX, hitY);
                            }
                            else
                            {
                                Debug.LogError($"Failed to parse coordinates: {hitPositions[i]}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Invalid coordinate format: {hitPositions[i]}, expected format: X,Y");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Missing position data for shot {i + 1}");
                    }
                }
            }
            else
            {
                Debug.LogError("dbManager is null, cannot update database!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateShootingScores: {e.Message}\n{e.StackTrace}");
            shootingScore.SetText("Score updating error...");
        }
    }

    public void RefreshScores()
    {
        Debug.Log("RefreshScores called");
        UpdateAllScores();
    }

    public void RestartCurrentGame()
    {
        Debug.Log("RestartCurrentGame called");
        ResetScoresBasedOnLevel();
        InitializeUITexts();
        UpdateAllScores();
    }
}