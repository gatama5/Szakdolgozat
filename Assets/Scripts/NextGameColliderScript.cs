
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
    [SerializeField] private SimonGameManager simonGame; // Simon j�t�k referencia
    [SerializeField] private ObjectSpawner targetGame; // Target j�t�k referencia (ObjectSpawner)
    [SerializeField] private ObjectSpawner_1place shootingGame; // L�v�ld�z�s j�t�k referencia
    [SerializeField] private ButtonsForMaze mazeGame; // �j: Labirintus j�t�k referencia (ButtonsForMaze)
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI completionText; // Befejez�s sz�veg (GameOver)
    [SerializeField] private List<TextMeshProUGUI> inGameTexts = new List<TextMeshProUGUI>(); // J�t�k k�zbeni sz�vegek
    [SerializeField] private float textDisplayTime = 5f; // �j: Sz�veg megjelen�t�si ideje
    [SerializeField] private string menuSceneName = "menu_basic"; // �j: Menu scene neve

    private static Vector3[] levelPositions = new Vector3[] {
        new Vector3(-58.56f, 0.54f, 7.49f),  // 1. szint
        new Vector3(-68.96f, 0.54f, 7.49f),  // 2. szint
        new Vector3(-79.87f, 0.54f, 7.49f),  // 3. szint
        new Vector3(-92.23f, 0.54f, 5.08f)   // 4. szint (maze j�t�k)
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

        // Ellen�rizz�k a j�t�kok referenci�it
        if (thisLevelIndex == 0 && simonGame == null)
        {
            Debug.LogError("Simon Game Manager reference is missing from the first level collider!");
        }
        if (thisLevelIndex == 1 && targetGame == null) // 2. szint ellen�rz�se
        {
            Debug.LogError("Target Game (ObjectSpawner) reference is missing from the second level collider!");
        }
        if (thisLevelIndex == 2 && shootingGame == null) // 3. szint ellen�rz�se
        {
            Debug.LogError("Shooting Game reference is missing from the third level collider!");
        }
        if (thisLevelIndex == 3 && mazeGame == null) // 4. szint ellen�rz�se (�j: labirintus)
        {
            Debug.LogError("Maze Game reference is missing from the fourth level collider!");
        }

        // Ellen�rizz�k az UI elemeket
        CheckUIElements();
    }

    private void CheckUIElements()
    {
        // Ha befejez�s sz�veg meg van adva, akkor elrejtj�k am�g nincs r� sz�ks�g
        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
        else if (thisLevelIndex == levelPositions.Length - 1)
        {
            Debug.LogWarning("Completion text (Game Over) is not assigned but this is the last level!");
        }

        // Ellen�rizz�k a j�t�k k�zben l�that� sz�vegeket
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

        // Els� szint - Simon j�t�k ellen�rz�se
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

        // M�sodik szint - Target j�t�k ellen�rz�se (ObjectSpawner)
        else if (thisLevelIndex == 1)
        {
            if (targetGame == null)
            {
                Debug.LogError("Target Game (ObjectSpawner) reference is missing!");
                return;
            }

            // Ellen�rizz�k, hogy a j�t�kos megsemmis�tett-e legal�bb annyi targetet, amennyit spawnoltunk
            if (targetGame.destroyedTargets < targetGame.numberToSpawn)
            {
                canProceed = false;
                Debug.Log($"Complete the Target game first! Destroyed: {targetGame.destroyedTargets}/{targetGame.numberToSpawn}");
            }
        }

        // Harmadik szint - L�v�ld�z�s j�t�k ellen�rz�se
        else if (thisLevelIndex == 2)
        {
            if (shootingGame == null)
            {
                Debug.LogError("Shooting Game reference is missing!");
                return;
            }
            if (shootingGame.hit_times.Count == 0) // Ha m�g nincs tal�lat, nem j�tszott
            {
                canProceed = false;
                Debug.Log("Complete at least one round of the Shooting game first!");
            }
        }

        // Negyedik szint - Labirintus j�t�k ellen�rz�se
        else if (thisLevelIndex == 3)
        {
            if (mazeGame == null)
            {
                Debug.LogError("Maze Game reference is missing!");
                return;
            }

            // Ellen�rizz�k, hogy a j�t�kos megnyomta-e a j� gombot (teljes�tette a labirintust)
            // Most m�r haszn�lhatjuk az isCompleted v�ltoz�t a k�zvetlen ellen�rz�shez
            if (!mazeGame.isCompleted)
            {
                canProceed = false;
                Debug.Log("Complete the Maze game first! Find and press the correct button.");
            }
        }

        if (!canProceed)
        {
            // Visszal�kj�k a j�t�kost
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

        // Ha az utols� szint ut�n vagyunk, akkor a j�t�k v�get �r
        if (thisLevelIndex == levelPositions.Length - 1)  // A maze j�t�k az utols�, innen visszadobjuk a men�be
        {
            StartCoroutine(CompleteGame());
            return;
        }

        // Egy�bk�nt tov�bbl�p�nk a k�vetkez� szintre
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
        // Elrejtj�k az �sszes j�t�k k�zben l�that� sz�veget
        foreach (TextMeshProUGUI inGameText in inGameTexts)
        {
            if (inGameText != null)
            {
                inGameText.gameObject.SetActive(false);
            }
        }

        // Megjelen�ti a befejez�s sz�veget, ha meg van adva
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);

            // Ha van score_time, hozz�adjuk az id�t a befejez�s sz�veghez
            if (thisLevelIndex == 3 && mazeGame != null && mazeGame.score_time != System.TimeSpan.Zero)
            {
                string formattedTime = string.Format("{0:mm\\:ss\\.ff}", mazeGame.score_time);
                completionText.text += $"\nYour time: {formattedTime}";
            }
        }

        // V�rakoz�s a meghat�rozott ideig
        yield return new WaitForSeconds(textDisplayTime);

        // J�t�k v�ge, visszadob�s a men�be
        ResetLevels();

        // Alaphelyzetbe �ll�tjuk a labirintus j�t�kot
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