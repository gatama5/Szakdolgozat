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

    // New field for game over notification
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

        // Gyorsнtбs minden sikeres kцr utбn
        currentPickDelay = Mathf.Max(minPickDelay,
            initialPickDelay - (speedIncreaseRate * buttons_Order.Count));

        isShowing = false;
    }

    void PickRandomColor()
    {
        // Megtalбljuk a legkevйsbй hasznбlt gombokat
        int minUsage = int.MaxValue;
        foreach (var usage in buttonUsageCount.Values)
        {
            minUsage = Mathf.Min(minUsage, usage);
        }

        // Цsszegyыjtjьk a legkevйsbй hasznбlt gombok indexeit
        List<int> leastUsedButtons = new List<int>();
        foreach (var kvp in buttonUsageCount)
        {
            if (kvp.Value == minUsage)
            {
                leastUsedButtons.Add(kvp.Key);
            }
        }

        // Vйletlenszerыen vбlasztunk a legkevйsbй hasznбlt gombok kцzьl
        int selectedIndex = leastUsedButtons[UnityEngine.Random.Range(0, leastUsedButtons.Count)];

        // Nцveljьk a hasznбlati szбmlбlуt
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
                    score.Set(buttons_Order.Count); // update the player score
                    StartCoroutine("PlayGame");
                }
            }
            else
            {
                isEnded = true;
                GameOver();
                // Game over logic here
            }
        }
    }

    private void GameOver()
    {
        int finalScore = score.GetCurrentScore();
        score.CheckForNewHighscore();
        Debug.Log($"Game Over! Final score: {finalScore}");

        // Adatbбzis mentйs
        SaveScoreToDatabase(finalScore);

        // Show game over notification
        ShowGameOverNotification();
    }

    private void ShowGameOverNotification()
    {
        if (gameOverNotificationText != null)
        {
            // Egyszerűen megjelenítjük a szöveget, nem módosítjuk a tartalmát
            gameOverNotificationText.gameObject.SetActive(true);

            // Hide the notification after delay
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
        if (dbManager != null && finalScore > 0) // NE MENTSЬK A NULLБS PONTSZБMOT
        {
            try
            {
                // Ellenхrizzьk, hogy van-e йrvйnyes jбtйkos azonosнtу
                int playerID = dbManager.GetCurrentPlayerID();

                // Ha nincs йrvйnyes azonosнtу, prуbбljuk meg visszaбllнtani a PlayerPrefs-bхl
                if (playerID <= 0 && PlayerPrefs.HasKey("CurrentPlayerID"))
                {
                    playerID = PlayerPrefs.GetInt("CurrentPlayerID");
                    dbManager.SetCurrentPlayerID(playerID);
                }

                // Ellenхrizzьk ъjra, hogy van-e йrvйnyes azonosнtу
                if (dbManager.GetCurrentPlayerID() > 0)
                {
                    // Frissнtjьk a Simon jбtйk pontszбmбt az adatbбzisban
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

        // Hide notification if it's visible
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }
}