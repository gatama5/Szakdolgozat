
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
    public bool isCompleted = false; 

    [Header("Trigger")]
    public MazeTrigger mazeTrigger;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;

    [Header("Game Start Notification")]
    [SerializeField] TextMeshProUGUI gameStartNotificationText;
    [SerializeField] float startNotificationDisplayTime = 3f;

    [Header("Game Over Notification")]
    [SerializeField] TextMeshProUGUI gameOverNotificationText;
    [SerializeField] float notificationDisplayTime = 3f;

    private bool doorstate = false;
    private bool timerActive = false;
    public bool playerInMaze = false;
    Stopwatch sw = new Stopwatch();
    public TimeSpan score_time;

    private ScoreManager scoreManager;
    private JSONDataSaver jsonDataSaver;

    void Start()
    {

        scoreManager = FindObjectOfType<ScoreManager>();

        jsonDataSaver = FindObjectOfType<JSONDataSaver>();
        if (jsonDataSaver == null)
        {
            UnityEngine.Debug.LogWarning("JSONDataSaver not found in scene. Player data will not be backed up after maze completion.");
        }


        if (mazeTrigger == null)
        {
            mazeTrigger = FindObjectOfType<MazeTrigger>();
            if (mazeTrigger == null)
            {
                UnityEngine.Debug.LogWarning("MazeTrigger not found or assigned. The trigger collider won't be disabled.");
            }
        }

        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }
        else
        {
            UnityEngine.Debug.LogError("Timer Text is not assigned in ButtonsForMaze!");
        }

        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(false);
        }

        SetInitialButtonColors();

        isCompleted = false;
    }

    void Update()
    {
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
        if (isCompleted)
        {
            UnityEngine.Debug.Log("The maze game is already completed and cannot be restarted.");
            return;
        }

        if (buttonsParent != null)
        {
            buttonsParent.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.LogError("ButtonsParent nincs beállítva a ButtonsForMaze szkriptben!");
            return;
        }

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        ShowGameStartNotification();

        sw.Reset();

        sw.Start();
        timerActive = true;
        UnityEngine.Debug.Log("Maze challenge started! Timer is running.");

        UpdateTimerDisplay();
    }

    private void ShowGameStartNotification()
    {
        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(true);

            if (gameStartNotificationText.gameObject.activeInHierarchy)
            {
                StartCoroutine(HideStartNotificationAfterDelay());
            }
            else
            {
                UnityEngine.Debug.LogWarning("A gameStartNotificationText objektum nem aktív a hierarchiában. Közvetlen időzítőt használunk.");
                Invoke("HideStartNotification", startNotificationDisplayTime);
            }
        }
    }

    private void HideStartNotification()
    {
        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideStartNotificationAfterDelay()
    {
        yield return new WaitForSeconds(startNotificationDisplayTime);
        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(false);
        }
    }

    public void OnButtonPress(GameObject pressedButton)
    {
        if (playerInMaze && !isCompleted)
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
        isCompleted = true;

        sw.Stop();
        timerActive = false;
        score_time = sw.Elapsed;

        string formattedTime = string.Format("{0:mm\\:ss\\.ff}", score_time);
        UnityEngine.Debug.Log($"Gratulálok! Teljesítetted a feladatot! Idõd: {formattedTime}");

        UpdateTimerDisplay();

        if (mazeTrigger != null)
        {
            mazeTrigger.gameObject.GetComponent<Collider>().enabled = false;
            UnityEngine.Debug.Log("Maze trigger collider disabled to prevent reactivation.");
        }

        StartCoroutine(HideRemainingButtons());
        if (scoreManager != null)
        {
            scoreManager.RefreshScores();
        }

        if (jsonDataSaver != null)
        {
            UnityEngine.Debug.Log("Maze completed, backing up player data to JSON...");
            jsonDataSaver.OnMazeGameCompleted();
        }

        ShowGameOverNotification();
    }

    private void ShowGameOverNotification()
    {
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(true);

            StartCoroutine(HideNotificationAfterDelay());
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDisplayTime);
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideRemainingButtons()
    {
        yield return new WaitForSeconds(1.0f);

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

    public void ResetMaze()
    {
        if (isCompleted)
        {
            UnityEngine.Debug.Log("Cannot reset maze - game is already completed.");
            return;
        }

        playerInMaze = false;
        timerActive = false;
        doorstate = false;

        sw.Reset();
        score_time = TimeSpan.Zero;
        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(false);
        }

        SetInitialButtonColors();
        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        foreach (GameObject button in badButtons)
        {
            button.SetActive(true);
        }
    }

    public void FullReset()
    {
        isCompleted = false;
        playerInMaze = false;
        timerActive = false;
        doorstate = false;

        sw.Reset();
        score_time = TimeSpan.Zero;
        if (timerText != null)
        {
            timerText.text = "Time: 00:00.00";
        }

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        if (gameStartNotificationText != null)
        {
            gameStartNotificationText.gameObject.SetActive(false);
        }

        SetInitialButtonColors();
        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        foreach (GameObject button in badButtons)
        {
            button.SetActive(true);
        }

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