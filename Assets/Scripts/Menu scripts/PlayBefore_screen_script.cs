
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PlayBefore_screen_script : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField ageInput;
    [SerializeField] private TextMeshProUGUI genDisplayBox;

    private SQLiteDBScript dbManager;
    private LoacalisationManagerScript locManager;

    private bool areAllFieldsFilled = false;

    void Start()
    {
        // Find the localization manager first
        locManager = FindObjectOfType<LoacalisationManagerScript>();
        if (locManager == null)
        {
            Debug.LogError("LoacalisationManagerScript not found!");
        }

        // Set up button listeners
        startButton.onClick.AddListener(Load_PlayVideoSceen);
        backButton.onClick.AddListener(GoToMainMenu);

        // Find components if not assigned in inspector
        if (ageInput == null)
            ageInput = transform.Find("InputFields/Age_input")?.GetComponent<TMP_InputField>();

        if (genDisplayBox == null)
            genDisplayBox = transform.Find("Texts/Gen_display_box")?.GetComponent<TextMeshProUGUI>();

        if (ageInput == null || genDisplayBox == null)
        {
            Debug.LogError("Hiányzó komponensek a hierarchiában!");
            return;
        }

        // Find database manager
        dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager == null)
        {
            Debug.LogError("SQLiteDBScript nem található!");
            return;
        }

        // Add listeners for input validation and generation calculation
        ageInput.onValueChanged.AddListener(ValidateInput);
        ageInput.onValueChanged.AddListener(UpdateGeneration);

        // Add listeners for all fields to update button state
        if (nameInput != null)
            nameInput.onValueChanged.AddListener((_) => UpdateStartButtonState());

        if (emailInput != null)
            emailInput.onValueChanged.AddListener((_) => UpdateStartButtonState());

        // Initial UI update
        UpdateStartButtonState();
    }

    private void Load_PlayVideoSceen()
    {
        if (areAllFieldsFilled)
        {
            // Save player data and check if it was successful
            if (SavePlayerData())
            {
                // Load appropriate scene based on localization
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
                        SceneManager.LoadScene(4); // Default to scene 4 if localization is unknown
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
                // Consider showing a UI message to the user here
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
            // Validate all required fields
            if (string.IsNullOrEmpty(nameInput.text) ||
                string.IsNullOrEmpty(emailInput.text) ||
                string.IsNullOrEmpty(ageInput.text) ||
                string.IsNullOrEmpty(genDisplayBox.text))
            {
                Debug.LogError("Cannot save player data: One or more required fields are empty");
                return false;
            }

            string playerName = nameInput.text;
            string playerEmail = emailInput.text;

            // Parse age with validation
            if (!int.TryParse(ageInput.text, out int playerAge))
            {
                Debug.LogError("Cannot save player data: Invalid age format");
                return false;
            }

            string generation = genDisplayBox.text;

            // Kezdeti értékek (késõbb frissítésre kerülnek)
            int simonScore = 0;
            double mazeTime = 0;
            int shootingScore = 0;

            // Adatok mentése - Now using the updated method without playerID
            int playerId = dbManager.InsertPlayerData(
                playerName,
                playerAge,
                playerEmail,
                generation,
                simonScore,
                mazeTime,
                shootingScore
            );

            if (playerId <= 0)
            {
                Debug.LogError("Failed to insert player data into database");
                return false;
            }

            // Store player ID in PlayerPrefs so other scenes can access it
            PlayerPrefs.SetInt("CurrentPlayerID", playerId);
            PlayerPrefs.Save();

            Debug.Log("Játékos adatok sikeresen elmentve! Player ID: " + playerId);
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
                UpdateStartButtonState();
                return;
            }
        }

        // Remove the last character if the input is invalid
        if (text.Length > 0)
        {
            ageInput.text = text.Substring(0, text.Length - 1);
        }
    }

    private void UpdateGeneration(string ageText)
    {
        if (string.IsNullOrEmpty(ageText))
        {
            genDisplayBox.text = "";
            return;
        }

        if (int.TryParse(ageText, out int age))
        {
            int birthYear = 2024 - age;
            string generation = DetermineGeneration(birthYear);
            genDisplayBox.text = generation;
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

    private void UpdateStartButtonState()
    {
        areAllFieldsFilled = !string.IsNullOrEmpty(nameInput?.text) &&
                            !string.IsNullOrEmpty(emailInput?.text) &&
                            !string.IsNullOrEmpty(ageInput?.text);

        if (startButton != null)
            startButton.interactable = areAllFieldsFilled;
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}