
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
    [SerializeField] private SimonGameManager simonGame;
    [SerializeField] private ObjectSpawner targetGame; 
    [SerializeField] private ObjectSpawner_1place shootingGame;
    [SerializeField] private ButtonsForMaze mazeGame; 
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private List<TextMeshProUGUI> inGameTexts = new List<TextMeshProUGUI>();
    [SerializeField] private float textDisplayTime = 5f; 
    [SerializeField] private string menuSceneName = "menu_basic"; 
    [SerializeField] private float teleportDelay = 0.5f;

    private static Vector3[] levelPositions = new Vector3[] {
        new Vector3(-58.56f, 0.54f, 7.49f),  // 1. szint
        new Vector3(-68.96f, 0.54f, 7.49f),  // 2. szint
        new Vector3(-79.87f, 0.54f, 7.49f),  // 3. szint
        new Vector3(-92.23f, 0.54f, 5.08f)   // 4. szint
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

        if (thisLevelIndex == 0 && simonGame == null)
        {
            Debug.LogError("Simon Game Manager reference is missing from the first level collider!");
        }
        if (thisLevelIndex == 1 && targetGame == null) 
        {
            Debug.LogError("Target Game (ObjectSpawner) reference is missing from the second level collider!");
        }
        if (thisLevelIndex == 2 && shootingGame == null) 
        {
            Debug.LogError("Shooting Game reference is missing from the third level collider!");
        }
        if (thisLevelIndex == 3 && mazeGame == null) 
        {
            Debug.LogError("Maze Game reference is missing from the fourth level collider!");
        }

        CheckUIElements();
    }

    private void CheckUIElements()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
        else if (thisLevelIndex == levelPositions.Length - 1)
        {
            Debug.LogWarning("Completion text (Game Over) is not assigned but this is the last level!");
        }

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

        else if (thisLevelIndex == 1)
        {
            if (targetGame == null)
            {
                Debug.LogError("Target Game (ObjectSpawner) reference is missing!");
                return;
            }

            if (targetGame.destroyedTargets < targetGame.numberToSpawn)
            {
                canProceed = false;
                Debug.Log($"Complete the Target game first! Destroyed: {targetGame.destroyedTargets}/{targetGame.numberToSpawn}");
            }
        }

        else if (thisLevelIndex == 2)
        {
            if (shootingGame == null)
            {
                Debug.LogError("Shooting Game reference is missing!");
                return;
            }
            if (shootingGame.hit_times.Count == 0) 
            {
                canProceed = false;
                Debug.Log("Complete at least one round of the Shooting game first!");
            }
        }

        else if (thisLevelIndex == 3)
        {
            if (mazeGame == null)
            {
                Debug.LogError("Maze Game reference is missing!");
                return;
            }

            if (!mazeGame.isCompleted)
            {
                canProceed = false;
                Debug.Log("Complete the Maze game first! Find and press the correct button.");
            }
        }

        if (!canProceed)
        {
            FPS_Controller fpsController = player.GetComponent<FPS_Controller>();
            if (fpsController != null)
            {
                bool originalCanMove = fpsController.canMove;
                fpsController.canMove = false;

                CharacterController charController = player.GetComponent<CharacterController>();
                if (charController != null)
                {
                    Vector3 pushDirection = -player.transform.forward * 2f;
                    charController.Move(pushDirection);
                }

                StartCoroutine(ReenableMovementAfterDelay(fpsController, originalCanMove, 0.2f));
            }
            else
            {
                player.transform.position += -player.transform.forward * 2f;
            }
            return;
        }

        if (thisLevelIndex == levelPositions.Length - 1) 
        {
            StartCoroutine(CompleteGame());
            return;
        }

        StartCoroutine(MoveToNextLevelSafely());
    }

    private IEnumerator MoveToNextLevelSafely()
    {
        if (currentLevel < levelPositions.Length - 1)
        {
            FPS_Controller fpsController = player.GetComponent<FPS_Controller>();
            bool originalCanMove = true;

            if (fpsController != null)
            {
                originalCanMove = fpsController.canMove;
                fpsController.canMove = false;
            }

            yield return new WaitForFixedUpdate();

            currentLevel++;
            Vector3 newPos = levelPositions[currentLevel];

            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                player.transform.position = newPos;
                charController.enabled = true;
            }
            else
            {
                player.transform.position = newPos;
            }

            yield return new WaitForSeconds(teleportDelay);

            if (fpsController != null)
            {
                fpsController.canMove = originalCanMove;
            }
        }
    }

    private IEnumerator ReenableMovementAfterDelay(FPS_Controller controller, bool originalState, float delay)
    {
        yield return new WaitForSeconds(delay);
        controller.canMove = originalState;
    }

    private IEnumerator CompleteGame()
    {
        foreach (TextMeshProUGUI inGameText in inGameTexts)
        {
            if (inGameText != null)
            {
                inGameText.gameObject.SetActive(false);
            }
        }

        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);

            if (thisLevelIndex == 3 && mazeGame != null && mazeGame.score_time != System.TimeSpan.Zero)
            {
                string formattedTime = string.Format("{0:mm\\:ss\\.ff}", mazeGame.score_time);
                completionText.text += $"\nYour time: {formattedTime}";
            }
        }

        yield return new WaitForSeconds(textDisplayTime);

        ResetLevels();

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