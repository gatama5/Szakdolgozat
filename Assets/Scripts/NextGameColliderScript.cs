
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NextGameColliderScript : MonoBehaviour
{
    private static int currentLevel = 0;
    [SerializeField] private GameObject player;
    [SerializeField] private int thisLevelIndex;
    [SerializeField] private SimonGameManager simonGame; // Simon játék referencia
    [SerializeField] private ObjectSpawner targetGame; // Target játék referencia (ObjectSpawner)
    [SerializeField] private ObjectSpawner_1place shootingGame; // Lövöldözõs játék referencia
    [SerializeField] private ButtonsForMaze mazeGame; // Új: Labirintus játék referencia (ButtonsForMaze)
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI completionText; // Befejezés szöveg (GameOver)
    [SerializeField] private List<TextMeshProUGUI> inGameTexts = new List<TextMeshProUGUI>(); // Játék közbeni szövegek
    [SerializeField] private float textDisplayTime = 5f; // Új: Szöveg megjelenítési ideje
    [SerializeField] private string menuSceneName = "menu_basic"; // Új: Menu scene neve

    private static Vector3[] levelPositions = new Vector3[] {
        new Vector3(-58.56f, 0.54f, 7.49f),  // 1. szint
        new Vector3(-68.96f, 0.54f, 7.49f),  // 2. szint
        new Vector3(-79.87f, 0.54f, 7.49f),  // 3. szint
        new Vector3(-92.23f, 0.54f, 5.08f)   // 4. szint (maze játék)
    };

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is not set in NextGameColliderScript!");
            return;
        }

        if (currentLevel == 0)
        {
            player.transform.position = levelPositions[0];
        }

        // Ellenõrizzük a játékok referenciáit
        if (thisLevelIndex == 0 && simonGame == null)
        {
            Debug.LogError("Simon Game Manager reference is missing from the first level collider!");
        }
        if (thisLevelIndex == 1 && targetGame == null) // 2. szint ellenõrzése
        {
            Debug.LogError("Target Game (ObjectSpawner) reference is missing from the second level collider!");
        }
        if (thisLevelIndex == 2 && shootingGame == null) // 3. szint ellenõrzése
        {
            Debug.LogError("Shooting Game reference is missing from the third level collider!");
        }
        if (thisLevelIndex == 3 && mazeGame == null) // 4. szint ellenõrzése (új: labirintus)
        {
            Debug.LogError("Maze Game reference is missing from the fourth level collider!");
        }

        // Ellenõrizzük az UI elemeket
        CheckUIElements();
    }

    private void CheckUIElements()
    {
        // Ha befejezés szöveg meg van adva, akkor elrejtjük amíg nincs rá szükség
        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
        else if (thisLevelIndex == levelPositions.Length - 1)
        {
            Debug.LogWarning("Completion text (Game Over) is not assigned but this is the last level!");
        }

        // Ellenõrizzük a játék közben látható szövegeket
        for (int i = 0; i < inGameTexts.Count; i++)
        {
            if (inGameTexts[i] == null)
            {
                Debug.LogWarning($"In-game text at index {i} is null!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || thisLevelIndex != currentLevel)
            return;

        bool canProceed = true;

        // Elsõ szint - Simon játék ellenõrzése
        if (thisLevelIndex == 0)
        {
            if (simonGame == null)
            {
                Debug.LogError("Simon Game Manager reference is missing!");
                return;
            }
            if (!simonGame.isEnded)
            {
                canProceed = false;
                Debug.Log("Complete the Simon Says game first!");
            }
        }

        // Második szint - Target játék ellenõrzése (ObjectSpawner)
        else if (thisLevelIndex == 1)
        {
            if (targetGame == null)
            {
                Debug.LogError("Target Game (ObjectSpawner) reference is missing!");
                return;
            }

            // Ellenõrizzük, hogy a játékos megsemmisített-e legalább annyi targetet, amennyit spawnoltunk
            if (targetGame.destroyedTargets < targetGame.numberToSpawn)
            {
                canProceed = false;
                Debug.Log($"Complete the Target game first! Destroyed: {targetGame.destroyedTargets}/{targetGame.numberToSpawn}");
            }
        }

        // Harmadik szint - Lövöldözõs játék ellenõrzése
        else if (thisLevelIndex == 2)
        {
            if (shootingGame == null)
            {
                Debug.LogError("Shooting Game reference is missing!");
                return;
            }
            if (shootingGame.hit_times.Count == 0) // Ha még nincs találat, nem játszott
            {
                canProceed = false;
                Debug.Log("Complete at least one round of the Shooting game first!");
            }
        }

        // Negyedik szint - Labirintus játék ellenõrzése
        else if (thisLevelIndex == 3)
        {
            if (mazeGame == null)
            {
                Debug.LogError("Maze Game reference is missing!");
                return;
            }

            // Ellenõrizzük, hogy a játékos megnyomta-e a jó gombot (teljesítette a labirintust)
            // Most már használhatjuk az isCompleted változót a közvetlen ellenõrzéshez
            if (!mazeGame.isCompleted)
            {
                canProceed = false;
                Debug.Log("Complete the Maze game first! Find and press the correct button.");
            }
        }

        if (!canProceed)
        {
            // Visszalökjük a játékost
            Vector3 pushBackPosition = player.transform.position + (player.transform.forward * -2f);
            if (player.GetComponent<CharacterController>() != null)
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = pushBackPosition;
                player.GetComponent<CharacterController>().enabled = true;
            }
            else
            {
                player.transform.position = pushBackPosition;
            }
            return;
        }

        // Ha az utolsó szint után vagyunk, akkor a játék véget ér
        if (thisLevelIndex == levelPositions.Length - 1)  // A maze játék az utolsó, innen visszadobjuk a menübe
        {
            StartCoroutine(CompleteGame());
            return;
        }

        // Egyébként továbblépünk a következõ szintre
        MoveToNextLevel();
    }

    private void MoveToNextLevel()
    {
        if (currentLevel < levelPositions.Length - 1)
        {
            currentLevel++;
            Vector3 newPos = levelPositions[currentLevel];
            if (player.GetComponent<CharacterController>() != null)
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = newPos;
                player.GetComponent<CharacterController>().enabled = true;
            }
            else
            {
                player.transform.position = newPos;
            }
        }
    }

    private IEnumerator CompleteGame()
    {
        // Elrejtjük az összes játék közben látható szöveget
        foreach (TextMeshProUGUI inGameText in inGameTexts)
        {
            if (inGameText != null)
            {
                inGameText.gameObject.SetActive(false);
            }
        }

        // Megjeleníti a befejezés szöveget, ha meg van adva
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);

            // Ha van score_time, hozzáadjuk az idõt a befejezés szöveghez
            if (thisLevelIndex == 3 && mazeGame != null && mazeGame.score_time != System.TimeSpan.Zero)
            {
                string formattedTime = string.Format("{0:mm\\:ss\\.ff}", mazeGame.score_time);
                completionText.text += $"\nYour time: {formattedTime}";
            }
        }

        // Várakozás a meghatározott ideig
        yield return new WaitForSeconds(textDisplayTime);

        // Játék vége, visszadobás a menübe
        ResetLevels();

        // Alaphelyzetbe állítjuk a labirintus játékot
        if (thisLevelIndex == 3 && mazeGame != null)
        {
            mazeGame.ResetMaze();
        }

        SceneManager.LoadScene(menuSceneName);
    }

    public static int GetCurrentLevel()
    {
        return currentLevel;
    }

    public static void ResetLevels()
    {
        currentLevel = 0;
    }
}