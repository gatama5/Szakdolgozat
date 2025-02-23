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
            if (objectSpawner.hit_times.Count != shootingTimes.Count)
            {
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

    //private void UpdateShootingScores()
    //{
    //    if (objectSpawner == null || shootingScore == null) return;

    //    try
    //    {
    //        if (objectSpawner.hit_times != null)
    //            shootingTimes = new List<double>(objectSpawner.hit_times);

    //        if (objectSpawner.hitPlace_fromMiddle != null)
    //            hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);

    //        if (shootingTimes.Count == 0)
    //        {
    //            shootingScore.SetText("No hits recorded yet");
    //            return;
    //        }

    //        double currentBestTime = shootingTimes.Min();
    //        if (currentBestTime < bestTime)
    //        {
    //            bestTime = currentBestTime;
    //            bestTimeIndex = shootingTimes.IndexOf(bestTime);

    //            // Adatbázis frissítése a legjobb eredménnyel
    //            if (dbManager != null && shootingTimes.Count > 0 && bestTimeIndex >= 0)
    //            {
    //                string[] coordinates = hitPositions[bestTimeIndex].Split(',');
    //                if (coordinates.Length == 2 &&
    //                    double.TryParse(coordinates[0], out double hitX) &&
    //                    double.TryParse(coordinates[1], out double hitY))
    //                {
    //                    dbManager.UpdateShootingScore(shootingTimes.Count, bestTime, hitX, hitY);
    //                }
    //            }
    //        }

    //        string bestPosition = bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count ?
    //            hitPositions[bestTimeIndex] : "N/A";

    //        string lastTime = shootingTimes.Count > 0 ?
    //            shootingTimes[shootingTimes.Count - 1].ToString("F2") : "N/A";

    //        string lastPosition = hitPositions.Count > 0 ?
    //            hitPositions[hitPositions.Count - 1] : "N/A";

    //        string displayText = string.Format(
    //            "Best Shot:\nTime: {0:F2} sec\nPosition: {1}\n\nLast Shot:\nTime: {2}\nPosition: {3}\n\nTotal Hits: {4}",
    //            bestTime,
    //            bestPosition,
    //            lastTime,
    //            lastPosition,
    //            shootingTimes.Count
    //        );

    //        shootingScore.SetText(displayText);
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"Error in UpdateShootingScores: {e.Message}");
    //        shootingScore.SetText("Score updating...");
    //    }
    //}

    private void UpdateShootingScores()
    {
        if (objectSpawner == null || shootingScore == null) return;

        try
        {
            if (objectSpawner.hit_times != null)
                shootingTimes = new List<double>(objectSpawner.hit_times);

            if (objectSpawner.hitPlace_fromMiddle != null)
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);

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
            }

            // Format the display text with proper position values
            StringBuilder displayText = new StringBuilder();
            displayText.AppendLine($"Best Shot:");
            displayText.AppendLine($"Time: {bestTime:F2} sec");

            if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
            {
                displayText.AppendLine($"Position: {hitPositions[bestTimeIndex]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
            }

            displayText.AppendLine("\nLast Shot:");
            displayText.AppendLine($"Time: {shootingTimes[shootingTimes.Count - 1]:F2} sec");

            if (hitPositions.Count > 0)
            {
                displayText.AppendLine($"Position: {hitPositions[hitPositions.Count - 1]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
            }

            displayText.AppendLine($"\nTotal Hits: {shootingTimes.Count}");

            shootingScore.SetText(displayText.ToString());

            // Update database with each shot
            if (dbManager != null)
            {
                for (int i = 0; i < shootingTimes.Count; i++)
                {
                    if (i < hitPositions.Count)
                    {
                        string[] coordinates = hitPositions[i].Split(',');
                        if (coordinates.Length == 2)
                        {
                            if (double.TryParse(coordinates[0], out double hitX) &&
                                double.TryParse(coordinates[1], out double hitY))
                            {
                                dbManager.UpdateShootingScore(i + 1, shootingTimes[i], hitX, hitY);
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateShootingScores: {e.Message}");
            shootingScore.SetText("Score updating...");
        }
    }

    public void RefreshScores()
    {
        UpdateAllScores();
    }

    public void RestartCurrentGame()
    {
        ResetScoresBasedOnLevel();
        InitializeUITexts();
        UpdateAllScores();
    }
}
