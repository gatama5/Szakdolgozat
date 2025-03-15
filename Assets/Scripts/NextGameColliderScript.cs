
using UnityEngine;
public class NextGameColliderScript : MonoBehaviour
{
    private static int currentLevel = 0;
    [SerializeField] private GameObject player;
    [SerializeField] private int thisLevelIndex;
    [SerializeField] private SimonGameManager simonGame; // Simon játék referencia
    [SerializeField] private ObjectSpawner targetGame; // Target játék referencia (ObjectSpawner)
    [SerializeField] private ObjectSpawner_1place shootingGame; // Lövöldözõs játék referencia


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

        // Ha idáig eljutottunk, akkor továbbléphetünk
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

    public static int GetCurrentLevel()
    {
        return currentLevel;
    }

    public static void ResetLevels()
    {
        currentLevel = 0;
    }
}