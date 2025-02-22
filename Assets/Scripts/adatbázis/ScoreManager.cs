
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

    private List<double> shootingTimes = new List<double>();
    private List<string> hitPositions = new List<string>();
    private double bestTime = double.MaxValue;
    private int bestTimeIndex = -1;

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();

    void Start()
    {
        // Kezdeti értékek beállítása
        if (shootingScore != null)
            shootingScore.text = "No hits recorded yet";
        if (simonScore != null)
            simonScore.text = "Simon Score: 0";
        if (mazeScore != null)
            mazeScore.text = "Maze Score: 0";

        UpdateAllScores();
    }

    void Update()
    {
        // Csak akkor frissítünk, ha van új adat és érvényes referenciák
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
            simonScore.SetText($"Simon High Score: {sm_scr.GetHighScore()}");
        }
    }

    private void UpdateMazeScore()
    {
        if (mazeScore != null && maze_scr != null)
        {
            mazeScore.SetText($"Maze Score: {maze_scr.score_time}");
        }
    }

    private void UpdateShootingScores()
    {
        if (objectSpawner == null || shootingScore == null) return;

        try
        {
            // Adatok másolása, ha vannak
            if (objectSpawner.hit_times != null)
            {
                shootingTimes = new List<double>(objectSpawner.hit_times);
            }

            if (objectSpawner.hitPlace_fromMiddle != null)
            {
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
            }

            // Ellenõrizzük, hogy van-e egyáltalán adat
            if (shootingTimes.Count == 0)
            {
                shootingScore.SetText("No hits recorded yet");
                return;
            }

            // Legjobb idõ keresése
            double currentBestTime = shootingTimes.Min();
            if (currentBestTime < bestTime)
            {
                bestTime = currentBestTime;
                bestTimeIndex = shootingTimes.IndexOf(bestTime);
            }

            // UI szöveg összeállítása
            string bestPosition = "N/A";
            if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
            {
                bestPosition = hitPositions[bestTimeIndex];
            }

            string lastTime = shootingTimes.Count > 0 ?
                shootingTimes[shootingTimes.Count - 1].ToString("F2") : "N/A";

            string lastPosition = "N/A";
            if (hitPositions.Count > 0)
            {
                lastPosition = hitPositions[hitPositions.Count - 1];
            }

            string displayText = string.Format(
                "Best Shot:\nTime: {0:F2} sec\nPosition: {1}\n\nLast Shot:\nTime: {2}\nPosition: {3}\n\nTotal Hits: {4}",
                bestTime,
                bestPosition,
                lastTime,
                lastPosition,
                shootingTimes.Count
            );

            shootingScore.SetText(displayText);
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
}