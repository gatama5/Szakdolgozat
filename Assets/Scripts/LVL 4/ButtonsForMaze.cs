
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

    [Header("Game State")]
    public bool isCompleted = false; // Új változó a játék befejezésének követésére

    [Header("Trigger")]
    public MazeTrigger mazeTrigger; // Adjunk hozzá referenciát a triggerre

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

        // Find MazeTrigger if not assigned
        if (mazeTrigger == null)
        {
            mazeTrigger = FindObjectOfType<MazeTrigger>();
            if (mazeTrigger == null)
            {
                UnityEngine.Debug.LogWarning("MazeTrigger not found or assigned. The trigger collider won't be disabled.");
            }
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

        // Alaphelyzetbe állítjuk a befejezés állapotot
        isCompleted = false;
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
        // Ha a játék már be van fejezve, nem indulhat újra
        if (isCompleted)
        {
            UnityEngine.Debug.Log("The maze game is already completed and cannot be restarted.");
            return;
        }

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
        if (playerInMaze && !isCompleted) // Csak akkor reagálunk a gombra, ha a játék nincs befejezve
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
        // Játék vége állapot beállítása
        isCompleted = true;

        sw.Stop();
        timerActive = false;
        score_time = sw.Elapsed;

        // Format time for logging
        string formattedTime = string.Format("{0:mm\\:ss\\.ff}", score_time);
        UnityEngine.Debug.Log($"Gratulálok! Teljesítetted a feladatot! Idõd: {formattedTime}");

        // Final timer update
        UpdateTimerDisplay();

        // Letiltjuk a triggert, hogy ne lehessen újra aktiválni
        if (mazeTrigger != null)
        {
            mazeTrigger.gameObject.GetComponent<Collider>().enabled = false;
            UnityEngine.Debug.Log("Maze trigger collider disabled to prevent reactivation.");
        }

        // Eltüntetjük a rossz gombokat
        StartCoroutine(HideRemainingButtons());

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

    // Coroutine a gombok eltüntetésére
    private IEnumerator HideRemainingButtons()
    {
        yield return new WaitForSeconds(1.0f); // Várunk egy másodpercet a vizuális visszajelzés érdekében

        // Elrejtjük az összes rossz gombot
        foreach (GameObject button in badButtons)
        {
            if (button.activeSelf)
            {
                button.SetActive(false);
            }
        }

        UnityEngine.Debug.Log("All remaining bad buttons hidden.");
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

    public bool IsMazeCompleted()
    {
        return isCompleted;
    }

    // Reset függvény módosítása
    public void ResetMaze()
    {
        // Ha a játék már be van fejezve, nem állítjuk vissza
        if (isCompleted)
        {
            UnityEngine.Debug.Log("Cannot reset maze - game is already completed.");
            return;
        }

        playerInMaze = false;
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

    // Teljesen új játék indításához (pl. újraindítás esetén)
    public void FullReset()
    {
        isCompleted = false;
        playerInMaze = false;
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

        // Re-enable all buttons
        foreach (GameObject button in badButtons)
        {
            button.SetActive(true);
        }

        // Trigger újra aktiválása, ha létezik
        if (mazeTrigger != null && mazeTrigger.gameObject.GetComponent<Collider>() != null)
        {
            mazeTrigger.gameObject.GetComponent<Collider>().enabled = true;
            mazeTrigger.mazeTriggerTimes = 0;
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
