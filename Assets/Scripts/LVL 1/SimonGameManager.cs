using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added TextMeshPro namespace

public class SimonGameManager : MonoBehaviour
{
    [SerializeField] SimonSaysButton[] buttons;
    [SerializeField] SimonGameStartButton startButton;
    [SerializeField] List<int> buttons_Order;
    [SerializeField] float initialPickDelay = 1f;
    [SerializeField] float minPickDelay = 0.3f;
    [SerializeField] float speedIncreaseRate = 0.05f;
    private float currentPickDelay;
    [SerializeField] int pickNumber = 0;
    [SerializeField] SimonScores score;
    [SerializeField] public bool isShowing = false;
    [SerializeField] public bool isEnded = false;

    [SerializeField] TextMeshProUGUI gameOverNotificationText;
    [SerializeField] float notificationDisplayTime = 3f;

    private Dictionary<int, int> buttonUsageCount = new Dictionary<int, int>();
    private SQLiteDBScript dbManager;



    void Start()
    {
        isEnded = false;
        SetButtonIndex();
        currentPickDelay = initialPickDelay;
        InitializeButtonUsage();

        // Hide notification text at start if it exists
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        // Megkeressьk az adatbбzis managert
        dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager == null)
        {
            Debug.LogWarning("SQLiteDBScript nem talбlhatу! Az eredmйnyek nem lesznek mentve.");
        }
    }


    void InitializeButtonUsage()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttonUsageCount[i] = 0;
        }
    }

    void SetButtonIndex()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].ButtonIndex = i;
        }
    }

    public IEnumerator PlayGame()
    {
        isShowing = true;
        pickNumber = 0;
        yield return new WaitForSeconds(currentPickDelay);

        foreach (var buttonIndex in buttons_Order)
        {
            buttons[buttonIndex].PressButton();
            yield return new WaitForSeconds(currentPickDelay);
        }

        PickRandomColor();

        currentPickDelay = Mathf.Max(minPickDelay,
            initialPickDelay - (speedIncreaseRate * buttons_Order.Count));

        isShowing = false;
    }

    void PickRandomColor()
    {
        int minUsage = int.MaxValue;
        foreach (var usage in buttonUsageCount.Values)
        {
            minUsage = Mathf.Min(minUsage, usage);
        }

        List<int> leastUsedButtons = new List<int>();
        foreach (var kvp in buttonUsageCount)
        {
            if (kvp.Value == minUsage)
            {
                leastUsedButtons.Add(kvp.Key);
            }
        }

        int selectedIndex = leastUsedButtons[UnityEngine.Random.Range(0, leastUsedButtons.Count)];

        buttonUsageCount[selectedIndex]++;

        buttons[selectedIndex].PressButton();
        buttons_Order.Add(selectedIndex);
    }


    public void PlayerPick(int btn_index)
    {

        if (!isShowing && buttons_Order.Count > 0 && !isEnded)
        {
            if (buttons_Order[pickNumber] == btn_index)
            {
                pickNumber++;
                if (pickNumber == buttons_Order.Count)
                {
                    score.Set(buttons_Order.Count);
                    StartCoroutine("PlayGame");
                }
            }
            else
            {
                isEnded = true;
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        int finalScore = score.GetCurrentScore();
        score.CheckForNewHighscore();
        Debug.Log($"Game Over! Final score: {finalScore}");

        SaveScoreToDatabase(finalScore);

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

    private void SaveScoreToDatabase(int finalScore)
    {
        if (dbManager != null && finalScore > 0) 
        {
            try
            {
                int playerID = dbManager.GetCurrentPlayerID();

                if (playerID <= 0 && PlayerPrefs.HasKey("CurrentPlayerID"))
                {
                    playerID = PlayerPrefs.GetInt("CurrentPlayerID");
                    dbManager.SetCurrentPlayerID(playerID);
                }

                if (dbManager.GetCurrentPlayerID() > 0)
                {
                    dbManager.UpdateSimonScore(finalScore);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Hiba tцrtйnt a pontszбm mentйse kцzben: {e.Message}");
            }
        }
    }


    public void ResetGame()
    {
        score.ResetScore();
        buttons_Order.Clear();
        currentPickDelay = initialPickDelay;
        InitializeButtonUsage();
        isEnded = false;

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }
}