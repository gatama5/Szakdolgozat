using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    private Dictionary<int, int> buttonUsageCount = new Dictionary<int, int>();
    private SQLiteDBScript dbManager;
    //[SerializeField] public GameObject door;
    //[SerializeField] public GameObject doorknob;


    //public Tuple<Vector3, Quaternion> door_start_pos =
    // new Tuple<Vector3, Quaternion>(
    //     new Vector3(13.21f, 2.32f, 6.26f),
    //     new Quaternion(0, -0.707103193f, 0, 0.707110405f)
    // );

    //public Tuple<Vector3, Quaternion> door_open_pos =
    //    new Tuple<Vector3, Quaternion>(
    //         new Vector3(16.44f, 2.32f, 3.75f),
    //         new Quaternion(0, -1, 0, 5.30f)
    //   );


    //public Tuple<Vector3, Quaternion> door_knob_start = new Tuple<Vector3, Quaternion>(new Vector3(10.72f, 6.48f, 6.26f), new Quaternion(0, -0.70f, 0, 0.70f));
    //public Tuple<Vector3, Quaternion> door_knob_open = new Tuple<Vector3, Quaternion>(new Vector3(16.5f, 6.4000001f, 0.109999999f), new Quaternion(0, -0.00617052522f, 0, 0.999981046f));


    //void Start()
    //{
    //    //door.transform.position = door_start_pos.Item1;
    //    //door.transform.rotation = door_start_pos.Item2;
    //    //doorknob.transform.position = door_knob_start.Item1;
    //    //doorknob.transform.rotation = door_knob_start.Item2;
    //    isEnded = false;
    //    SetButtonIndex();
    //    currentPickDelay = initialPickDelay;
    //    InitializeButtonUsage();

    //}

    void Start()
    {
        isEnded = false;
        SetButtonIndex();
        currentPickDelay = initialPickDelay;
        InitializeButtonUsage();

        // Megkeressük az adatbázis managert
        dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager == null)
        {
            Debug.LogWarning("SQLiteDBScript nem található! Az eredmények nem lesznek mentve.");
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

        // Gyorsítás minden sikeres kör után
        currentPickDelay = Mathf.Max(minPickDelay,
            initialPickDelay - (speedIncreaseRate * buttons_Order.Count));

        isShowing = false;
    }

    void PickRandomColor()
    {
        // Megtaláljuk a legkevésbé használt gombokat
        int minUsage = int.MaxValue;
        foreach (var usage in buttonUsageCount.Values)
        {
            minUsage = Mathf.Min(minUsage, usage);
        }

        // Összegyûjtjük a legkevésbé használt gombok indexeit
        List<int> leastUsedButtons = new List<int>();
        foreach (var kvp in buttonUsageCount)
        {
            if (kvp.Value == minUsage)
            {
                leastUsedButtons.Add(kvp.Key);
            }
        }

        // Véletlenszerûen választunk a legkevésbé használt gombok közül
        int selectedIndex = leastUsedButtons[UnityEngine.Random.Range(0, leastUsedButtons.Count)];

        // Növeljük a használati számlálót
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

    //private void GameOver()
    //{
    //    score.CheckForNewHighscore(); // Ellenõrizzük, hogy új highscore született-e
    //    Debug.Log($"Game Over! Final score: {score.GetCurrentScore()}");
    //}


    private void GameOver()
    {
        int finalScore = score.GetCurrentScore();
        score.CheckForNewHighscore();
        Debug.Log($"Game Over! Final score: {finalScore}");

        // Adatbázis mentés
        SaveScoreToDatabase(finalScore);
    }

    private void SaveScoreToDatabase(int finalScore)
    {
        if (dbManager != null)
        {
            try
            {
                // Frissítjük a Simon játék pontszámát az adatbázisban
                dbManager.UpdateSimonScore(finalScore);
                Debug.Log($"Simon játék pontszám sikeresen mentve: {finalScore}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Hiba történt a pontszám mentése közben: {e.Message}");
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
    }

}
