
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using UnityEngine.Video;
//using UnityEngine.SceneManagement;

//public class PlayBefore_screen_script : MonoBehaviour
//{
//    [SerializeField] private Button startButton;
//    [SerializeField] private Button backButton;
//    [SerializeField] private TMP_InputField nameInput;
//    [SerializeField] private TMP_InputField emailInput;
//    [SerializeField] private TMP_InputField ageInput;
//    [SerializeField] private TextMeshProUGUI genDisplayBox;

//    private SQLiteDBScript dbManager;
//    private LoacalisationManagerScript locManager;

//    private bool areAllFieldsFilled = false;

//    void Start()
//    {
//        // Find the localization manager first
//        locManager = FindObjectOfType<LoacalisationManagerScript>();
//        if (locManager == null)
//        {
//            Debug.LogError("LoacalisationManagerScript not found!");
//        }

//        // Set up button listeners
//        startButton.onClick.AddListener(Load_PlayVideoSceen);
//        backButton.onClick.AddListener(GoToMainMenu);

//        // Find components if not assigned in inspector
//        if (ageInput == null)
//            ageInput = transform.Find("InputFields/Age_input")?.GetComponent<TMP_InputField>();

//        if (genDisplayBox == null)
//            genDisplayBox = transform.Find("Texts/Gen_display_box")?.GetComponent<TextMeshProUGUI>();

//        if (ageInput == null || genDisplayBox == null)
//        {
//            Debug.LogError("Hiányzó komponensek a hierarchiában!");
//            return;
//        }

//        // Find database manager
//        dbManager = FindObjectOfType<SQLiteDBScript>();
//        if (dbManager == null)
//        {
//            Debug.LogError("SQLiteDBScript nem található!");
//            return;
//        }

//        // Add listeners for input validation and generation calculation
//        ageInput.onValueChanged.AddListener(ValidateInput);


//        // Add listeners for all fields to update button state
//        if (nameInput != null)
//            nameInput.onValueChanged.AddListener((_) => UpdateStartButtonState());

//        if (emailInput != null)
//            emailInput.onValueChanged.AddListener((_) => UpdateStartButtonState());

//        // Initial UI update
//        UpdateStartButtonState();
//    }

//    private void Load_PlayVideoSceen()
//    {
//        if (areAllFieldsFilled)
//        {
//            // Save player data and check if it was successful
//            if (SavePlayerData())
//            {
//                // Load appropriate scene based on localization
//                if (locManager != null)
//                {
//                    if (locManager.getLocal() == 1)
//                    {
//                        SceneManager.LoadScene(4);
//                    }
//                    else if (locManager.getLocal() == 0)
//                    {
//                        SceneManager.LoadScene(5);
//                    }
//                    else
//                    {
//                        Debug.LogWarning("Unknown localization value: " + locManager.getLocal());
//                        SceneManager.LoadScene(4); // Default to scene 4 if localization is unknown
//                    }
//                }
//                else
//                {
//                    Debug.LogWarning("Localization manager not found, defaulting to scene 4");
//                    SceneManager.LoadScene(4);
//                }
//            }
//            else
//            {
//                Debug.LogError("Failed to save player data, not proceeding to game scene");
//                // Consider showing a UI message to the user here
//            }
//        }
//    }

//    private bool SavePlayerData()
//    {
//        if (dbManager == null)
//        {
//            Debug.LogError("Cannot save player data: Database manager not found");
//            return false;
//        }

//        try
//        {
//            // Validate all required fields
//            if (string.IsNullOrEmpty(nameInput.text) ||
//                string.IsNullOrEmpty(emailInput.text) ||
//                string.IsNullOrEmpty(ageInput.text) ||
//                string.IsNullOrEmpty(genDisplayBox.text))
//            {
//                Debug.LogError("Cannot save player data: One or more required fields are empty");
//                return false;
//            }

//            string playerName = nameInput.text;
//            string playerEmail = emailInput.text;

//            // Parse age with validation
//            if (!int.TryParse(ageInput.text, out int playerAge))
//            {
//                Debug.LogError("Cannot save player data: Invalid age format");
//                return false;
//            }

//            string generation = genDisplayBox.text;

//            // Kezdeti értékek (később frissítésre kerülnek)
//            int simonScore = 0;
//            double mazeTime = 0;
//            int shootingScore = 0;

//            // Adatok mentése - Now using the updated method without playerID
//            int playerId = dbManager.InsertPlayerData(
//                playerName,
//                playerAge,
//                playerEmail,
//                generation,
//                simonScore,
//                mazeTime,
//                shootingScore
//            );

//            if (playerId <= 0)
//            {
//                Debug.LogError("Failed to insert player data into database");
//                return false;
//            }

//            // Store player ID in PlayerPrefs so other scenes can access it
//            PlayerPrefs.SetInt("CurrentPlayerID", playerId);
//            PlayerPrefs.Save();

//            return true;
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"Hiba az adatok mentése során: {e.Message}");
//            return false;
//        }
//    }


//    private void ValidateInput(string text)
//    {
//        if (string.IsNullOrEmpty(text))
//        {
//            genDisplayBox.text = "";
//            UpdateStartButtonState();
//            return;
//        }

//        // Ellenőrizzük, hogy a teljes szöveg érvényes-e
//        if (int.TryParse(text, out int number))
//        {
//            if (number >= 0 && number <= 99)
//            {
//                // Az érték rendben van, hagyjuk és frissítsük a generációt
//                int birthYear = 2024 - number;
//                string generation = DetermineGeneration(birthYear);
//                genDisplayBox.text = generation;
//                UpdateStartButtonState();
//                return;
//            }
//        }

//        // Ha ide jutunk, az bemenet érvénytelen
//        if (text.Length > 0)
//        {
//            // Mivel az aktuális bemenet érvénytelen, állítsuk vissza az előző érvényes értékre
//            ageInput.text = text.Substring(0, text.Length - 1);
//            // Helyezzük a kurzort a szöveg végére
//            ageInput.caretPosition = ageInput.text.Length;

//            // Ha még maradt érvényes szám, akkor frissítsük a generációt azzal
//            if (int.TryParse(ageInput.text, out int validNumber) && validNumber >= 0 && validNumber <= 99)
//            {
//                int birthYear = 2024 - validNumber;
//                string generation = DetermineGeneration(birthYear);
//                genDisplayBox.text = generation;
//            }
//        }
//    }

//    private string DetermineGeneration(int birthYear)
//    {
//        if (birthYear >= 2012)
//            return "Alpha generation";
//        else if (birthYear >= 1997)
//            return "Z generation";
//        else if (birthYear >= 1981)
//            return "Y generation (Millennials)";
//        else if (birthYear >= 1965)
//            return "X generation";
//        else if (birthYear >= 1946)
//            return "Baby Boomer";
//        else
//            return "Veteran generation";
//    }

//    private void UpdateStartButtonState()
//    {
//        areAllFieldsFilled = !string.IsNullOrEmpty(nameInput?.text) &&
//                            !string.IsNullOrEmpty(emailInput?.text) &&
//                            !string.IsNullOrEmpty(ageInput?.text);

//        if (startButton != null)
//            startButton.interactable = areAllFieldsFilled;
//    }

//    private void GoToMainMenu()
//    {
//        SceneManager.LoadScene(0);
//    }
//}

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
    [SerializeField] private TextMeshProUGUI playerIDText; // Játékos ID kijelzésére

    private SQLiteDBScript dbManager;
    private LoacalisationManagerScript locManager;

    private bool isAgeFieldFilled = false;
    private int tempGeneratedID = 0;

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

        // Azonnal generáljunk egy ID-t betöltéskor és jelenítsük meg
        GenerateAndDisplayID();

        // Add listeners for input validation and generation calculation
        ageInput.onValueChanged.AddListener(ValidateInput);

        // Add listeners for field to update button state
        if (ageInput != null)
            ageInput.onValueChanged.AddListener((_) => UpdateStartButtonState());

        // Initial UI update
        UpdateStartButtonState();
    }

    // Új metódus az ID generálásához és megjelenítéséhez
    private void GenerateAndDisplayID()
    {
        if (playerIDText != null)
        {
            // Dátum alapú ID generálása
            string dateStr = DateTime.Now.ToString("yyMMdd");
            string timeStr = DateTime.Now.ToString("HHmm");
            string idStr = dateStr + timeStr.Substring(0, Math.Min(2, timeStr.Length));

            if (!int.TryParse(idStr, out tempGeneratedID))
            {
                tempGeneratedID = (int)(DateTime.Now.Ticks % 1000000000);
            }

            // Csak az ID kiírása
            playerIDText.text = tempGeneratedID.ToString();
        }
    }

    private void UpdateStartButtonState()
    {
        // Csak az életkor mezőt ellenőrizzük
        isAgeFieldFilled = !string.IsNullOrEmpty(ageInput?.text);

        if (startButton != null)
            startButton.interactable = isAgeFieldFilled;
    }

    private void Load_PlayVideoSceen()
    {
        if (isAgeFieldFilled)
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
            // Validate age field
            if (string.IsNullOrEmpty(ageInput.text) ||
                string.IsNullOrEmpty(genDisplayBox.text))
            {
                Debug.LogError("Cannot save player data: One or more required fields are empty");
                return false;
            }

            // Parse age with validation
            if (!int.TryParse(ageInput.text, out int playerAge))
            {
                Debug.LogError("Cannot save player data: Invalid age format");
                return false;
            }

            string generation = genDisplayBox.text;

            // Használjuk az eredeti InsertPlayerData függvényt, mert a SavePlayerDetailsWithID még nem létezik
            int playerId = dbManager.InsertPlayerData(
                playerAge,
                generation,
                0, // simonScore
                0, // mazeTime
                0  // shootingScore
            );

            if (playerId <= 0)
            {
                Debug.LogError("Failed to insert player data into database");
                return false;
            }

            // Store player ID in PlayerPrefs for other scenes
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

        // Ellenőrizzük, hogy a teljes szöveg érvényes-e
        if (int.TryParse(text, out int number))
        {
            if (number >= 0 && number <= 99)
            {
                // Az érték rendben van, hagyjuk és frissítsük a generációt
                int birthYear = 2024 - number;
                string generation = DetermineGeneration(birthYear);
                genDisplayBox.text = generation;
                UpdateStartButtonState();
                return;
            }
        }

        // Ha ide jutunk, az bemenet érvénytelen
        if (text.Length > 0)
        {
            // Mivel az aktuális bemenet érvénytelen, állítsuk vissza az előző érvényes értékre
            ageInput.text = text.Substring(0, text.Length - 1);
            // Helyezzük a kurzort a szöveg végére
            ageInput.caretPosition = ageInput.text.Length;

            // Ha még maradt érvényes szám, akkor frissítsük a generációt azzal
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