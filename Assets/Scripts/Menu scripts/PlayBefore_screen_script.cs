
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class PlayBefore_screen_script : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField ageInput;
    [SerializeField] private TextMeshProUGUI genDisplayBox;
    [SerializeField] private TextMeshProUGUI playerIDText;

    private SQLiteDBScript dbManager;
    private LoacalisationManagerScript locManager;

    private bool isAgeFieldFilled = false;
    private int tempGeneratedID = 0;

    void Start()
    {
        locManager = FindObjectOfType<LoacalisationManagerScript>();
        if (locManager == null)
        {
            Debug.LogError("LoacalisationManagerScript not found!");
        }

        startButton.onClick.AddListener(Load_PlayVideoSceen);
        backButton.onClick.AddListener(GoToMainMenu);

        if (ageInput == null)
            ageInput = transform.Find("InputFields/Age_input")?.GetComponent<TMP_InputField>();

        if (genDisplayBox == null)
            genDisplayBox = transform.Find("Texts/Gen_display_box")?.GetComponent<TextMeshProUGUI>();

        if (ageInput == null || genDisplayBox == null)
        {
            Debug.LogError("Hiányzó komponensek a hierarchiában!");
            return;
        }

        dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager == null)
        {
            Debug.LogError("SQLiteDBScript nem található!");
            return;
        }

        GenerateAndDisplayID();

        ageInput.onValueChanged.AddListener(ValidateInput);

        if (ageInput != null)
            ageInput.onValueChanged.AddListener((_) => UpdateStartButtonState());
        UpdateStartButtonState();
    }

    private void GenerateAndDisplayID()
    {
        if (playerIDText != null)
        {
            string dateStr = DateTime.Now.ToString("MMdd");
            string timeStr = DateTime.Now.ToString("HHmm");
            string idStr = dateStr + timeStr;

            if (!int.TryParse(idStr, out tempGeneratedID))
            {
                tempGeneratedID = (int)(DateTime.Now.Ticks % 1000000000);
            }

            playerIDText.text = tempGeneratedID.ToString();
        }
    }

    private void UpdateStartButtonState()
    {
        isAgeFieldFilled = !string.IsNullOrEmpty(ageInput?.text);

        if (startButton != null)
            startButton.interactable = isAgeFieldFilled;
    }

    private void Load_PlayVideoSceen()
    {
        if (isAgeFieldFilled)
        {
            if (SavePlayerData())
            {
                if (locManager != null)
                {
                    if (locManager.getLocal() == 1)
                    {
                        SceneManager.LoadScene(4);
                    }
                    else if (locManager.getLocal() == 0)
                    {
                        SceneManager.LoadScene(5);
                    }
                    else
                    {
                        Debug.LogWarning("Unknown localization value: " + locManager.getLocal());
                        SceneManager.LoadScene(4);
                    }
                }
                else
                {
                    Debug.LogWarning("Localization manager not found, defaulting to scene 4");
                    SceneManager.LoadScene(4);
                }
            }
            else
            {
                Debug.LogError("Failed to save player data, not proceeding to game scene");
            }
        }
    }

    private bool SavePlayerData()
    {
        if (dbManager == null)
        {
            Debug.LogError("Cannot save player data: Database manager not found");
            return false;
        }

        try
        {
            if (string.IsNullOrEmpty(ageInput.text) ||
                string.IsNullOrEmpty(genDisplayBox.text))
            {
                Debug.LogError("Cannot save player data: One or more required fields are empty");
                return false;
            }

            if (!int.TryParse(ageInput.text, out int playerAge))
            {
                Debug.LogError("Cannot save player data: Invalid age format");
                return false;
            }

            string generation = genDisplayBox.text;

            int playerId = dbManager.InsertPlayerData(
                playerAge,
                generation,
                0,
                0, 
                0  
            );

            if (playerId <= 0)
            {
                Debug.LogError("Failed to insert player data into database");
                return false;
            }


            PlayerPrefs.SetInt("CurrentPlayerID", playerId);
            PlayerPrefs.Save();

            Debug.Log($"Játékos adatok mentve. ID: {playerId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Hiba az adatok mentése során: {e.Message}");
            return false;
        }
    }

    private void ValidateInput(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            genDisplayBox.text = "";
            UpdateStartButtonState();
            return;
        }

        if (int.TryParse(text, out int number))
        {
            if (number >= 0 && number <= 99)
            {

                int birthYear = 2024 - number;
                string generation = DetermineGeneration(birthYear);
                genDisplayBox.text = generation;
                UpdateStartButtonState();
                return;
            }
        }

        if (text.Length > 0)
        {
            ageInput.text = text.Substring(0, text.Length - 1);
            ageInput.caretPosition = ageInput.text.Length;

            if (int.TryParse(ageInput.text, out int validNumber) && validNumber >= 0 && validNumber <= 99)
            {
                int birthYear = 2024 - validNumber;
                string generation = DetermineGeneration(birthYear);
                genDisplayBox.text = generation;
            }
        }
    }

    private string DetermineGeneration(int birthYear)
    {
        if (birthYear >= 2012)
            return "Alpha generation";
        else if (birthYear >= 1997)
            return "Z generation";
        else if (birthYear >= 1981)
            return "Y generation (Millennials)";
        else if (birthYear >= 1965)
            return "X generation";
        else if (birthYear >= 1946)
            return "Baby Boomer";
        else
            return "Veteran generation";
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}