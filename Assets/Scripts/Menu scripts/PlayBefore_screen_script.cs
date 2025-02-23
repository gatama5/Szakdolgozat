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
    private static int currentPlayerId = 1;

    private LoacalisationManagerScript locManager;
    //public Animator transition;

    private bool areAllFieldsFilled = false;

    void Start()
    {
        locManager = FindObjectOfType<LoacalisationManagerScript>();
        startButton.onClick.AddListener(Load_PlayVideoSceen);
        backButton.onClick.AddListener(GoToMainMenu);

        ageInput = transform.Find("InputFields/Age_input").GetComponent<TMP_InputField>();
        genDisplayBox = transform.Find("Texts/Gen_display_box").GetComponent<TextMeshProUGUI>();


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

        ageInput.onValueChanged.AddListener(ValidateInput);
        ageInput.onValueChanged.AddListener(UpdateGeneration);

        UpdateStartButtonState();
    }

    private void Load_PlayVideoSceen()
    {
        if (areAllFieldsFilled)
        {
            // Adatok mentése az adatbázisba
            SavePlayerData();

            // Scene betöltése
            if (locManager.getLocal() == 1)
            {
                SceneManager.LoadScene(9);
            }
            if (locManager.getLocal() == 0)
            {
                SceneManager.LoadScene(10);
            }
        }
    }

    private void SavePlayerData()
    {
        try
        {
            string playerName = nameInput.text;
            string playerEmail = emailInput.text;
            int playerAge = int.Parse(ageInput.text);
            string generation = genDisplayBox.text;

            // Kezdeti értékek (késõbb frissítésre kerülnek)
            int simonScore = 0;
            double mazeTime = 0;
            int shootingScore = 0;

            // Adatok mentése
            dbManager.InsertPlayerData(
                currentPlayerId,
                playerName,
                playerAge,
                playerEmail,
                generation,
                simonScore,
                mazeTime,
                shootingScore
            );

            // ID növelése a következõ játékoshoz
            currentPlayerId++;

            // Játékos ID mentése a késõbbi frissítésekhez
            PlayerPrefs.SetInt("CurrentPlayerId", currentPlayerId - 1);
            PlayerPrefs.Save();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Hiba az adatok mentése során: {e.Message}");
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
        areAllFieldsFilled = !string.IsNullOrEmpty(nameInput.text) &&
                            !string.IsNullOrEmpty(emailInput.text) &&
                            !string.IsNullOrEmpty(ageInput.text);

        startButton.interactable = areAllFieldsFilled;
    }

    //private void Load_PlayVideoSceen()
    //{

    //    if (locManager.getLocal() == 1) 
    //    {
    //        SceneManager.LoadScene(9);
    //    }
    //    if (locManager.getLocal() == 0) 
    //    {
    //        SceneManager.LoadScene(10);
    //    }
    //}

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

}