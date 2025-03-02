
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class ButtonsForMaze : MonoBehaviour
{
    public GameObject[] badButtons;
    public GameObject goodButton;
    public Color deactivate = Color.yellow;
    public Color activated = Color.green;
    public AudioSource buttonSound;
    public GameObject buttonsParent;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Make sure to assign this in the Inspector

    private bool doorstate = false;
    //private float timeInLabyrinth = 0f;
    private bool timerActive = false;
    public bool playerInMaze = false;
    Stopwatch sw = new Stopwatch();
    public TimeSpan score_time;

    private ScoreManager scoreManager;
    private JSONDataSaver jsonDataSaver; // Added reference to the JSONDataSaver

    void Start()
    {
        // Find ScoreManager
        scoreManager = FindObjectOfType<ScoreManager>();

        // Find JSONDataSaver
        jsonDataSaver = FindObjectOfType<JSONDataSaver>();
        if (jsonDataSaver == null)
        {
            UnityEngine.Debug.LogWarning("JSONDataSaver not found in scene. Player data will not be backed up after maze completion.");
        }

        // Initialize timer text
        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }
        else
        {
            UnityEngine.Debug.LogError("Timer Text is not assigned in ButtonsForMaze!");
        }

        // Kezdetben a gombok és szöveg rejtve
        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        SetInitialButtonColors();
    }

    void Update()
    {
        // Update timer display if active
        if (timerActive && sw.IsRunning)
        {
            score_time = sw.Elapsed;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = string.Format("Time: {0:mm\\:ss\\.ff}", score_time);
        }
    }

    private void SetInitialButtonColors()
    {
        foreach (GameObject button in badButtons)
        {
            Renderer renderer = button.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = deactivate;
        }

        Renderer goodButtonRenderer = goodButton.GetComponent<Renderer>();
        if (goodButtonRenderer != null)
            goodButtonRenderer.material.color = deactivate;
    }

    public void StartMazeChallenge()
    {
        // Reset timer
        sw.Reset();

        // Gombok megjelenítése
        if (buttonsParent != null)
            buttonsParent.SetActive(true);

        // Idõzítõ indítása
        sw.Start();
        timerActive = true;
        UnityEngine.Debug.Log("Maze challenge started! Timer is running.");

        // Make sure the timer display is initialized
        UpdateTimerDisplay();
    }

    public void OnButtonPress(GameObject pressedButton)
    {
        if (playerInMaze)
        {
            UnityEngine.Debug.Log(pressedButton.name);
            pressedButton.GetComponent<MeshRenderer>().material.color = activated;
            buttonSound.Play();

            if (pressedButton == goodButton)
            {
                GoodButtonPress();
            }
            else
            {
                BadButtonPress(pressedButton);
            }
        }
    }

    public void GoodButtonPress()
    {
        sw.Stop();
        timerActive = false;
        score_time = sw.Elapsed;

        // Format time for logging
        string formattedTime = string.Format("{0:mm\\:ss\\.ff}", score_time);
        UnityEngine.Debug.Log($"Gratulálok! Teljesítetted a feladatot! Idõd: {formattedTime}");

        // Final timer update
        UpdateTimerDisplay();

        // Update score in ScoreManager
        if (scoreManager != null)
        {
            scoreManager.RefreshScores();
        }

        // Backup player data after maze completion
        if (jsonDataSaver != null)
        {
            UnityEngine.Debug.Log("Maze completed, backing up player data to JSON...");
            jsonDataSaver.OnMazeGameCompleted();
        }
    }

    public void BadButtonPress(GameObject pressedButton)
    {
        UnityEngine.Debug.Log($"Rossz gomb keress tovább!");
        pressedButton.SetActive(false);
    }

    public bool GetDoorState()
    {
        return doorstate;
    }

    public float GetCompletionTime()
    {
        return (float)score_time.TotalSeconds;
    }

    // Reset függvény hozzáadása, ha szükséges újrakezdeni a játékot
    public void ResetMaze()
    {
        playerInMaze = false;
        //timeInLabyrinth = 0f;
        timerActive = false;
        doorstate = false;

        // Reset timer
        sw.Reset();
        score_time = TimeSpan.Zero;
        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }

        // Reset buttons
        SetInitialButtonColors();
        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        // Re-enable all buttons that might have been disabled
        foreach (GameObject button in badButtons)
        {
            button.SetActive(true);
        }
    }

    public void ResetScore()
    {
        score_time = TimeSpan.Zero;
        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }
    }
}