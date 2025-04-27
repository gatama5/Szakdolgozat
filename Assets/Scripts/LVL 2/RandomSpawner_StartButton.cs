
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomSpawner_StartButton : MonoBehaviour
{
    public ObjectSpawner objSpawner; 
    public GameObject targetPrefab;
    public PickUpGun pickUpGun; 
    public AudioSource audioSource;
    public Gun gunScript; 

    [SerializeField] public float startDelay = 3f; 
    [SerializeField] private bool showCountdown = true;

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    private Renderer buttonRenderer;
    [SerializeField] private Color greenColor = new Color(0.0f, 1.0f, 0.0f);

    public bool isPlaying = false;

    void Start()
    {
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
        isPlaying = false;

        if (objSpawner == null)
        {
            objSpawner = FindObjectOfType<ObjectSpawner>();
            if (objSpawner == null)
            {
                Debug.LogError("Nem található ObjectSpawner objektum a játékban!");
            }
        }

        if (pickUpGun == null)
        {
            pickUpGun = FindObjectOfType<PickUpGun>();
            if (pickUpGun == null)
            {
                Debug.LogWarning("Nem található PickUpGun objektum a játékban!");
            }
        }

        if (gunScript == null)
        {
            gunScript = FindObjectOfType<Gun>();
        }

        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = greenColor;
        }
        else
        {
            buttonRenderer = GetComponentInChildren<Renderer>();
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = greenColor;
            }
            else
            {
                Debug.LogWarning("Nem található Renderer komponens a gombhoz!");
            }
        }
    }

    private void OnMouseDown()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (pickUpGun != null && pickUpGun.isPickedUp)
        {
            isPlaying = true;
            StartCoroutine(StartPlay());
        }
        else
        {
            if (countdownText != null)
            {
                countdownText.text = "Kérem vegye fel a fegyvert a kezdéshez!";
                if (countdownCanvas != null)
                {
                    countdownCanvas.enabled = true;
                }
                StartCoroutine(ClearTextAfterDelay(2f));
            }
            else
            {
                Debug.Log("Kérem vegye fel a fegyvert a kezdéshez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetGameTypeData(1);
        }

        if (objSpawner != null && targetPrefab != null)
        {
            objSpawner.target = targetPrefab;

            if (pickUpGun != null)
            {
                objSpawner.pickUpGun = pickUpGun;
            }

            if (gunScript != null)
            {
                gunScript.pck_gun = pickUpGun;
            }
        }

        float remainingTime = startDelay;
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = true;
        }

        while (remainingTime > 0)
        {
            if (showCountdown && countdownText != null)
            {
                countdownText.text = Mathf.Ceil(remainingTime).ToString();
            }
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (showCountdown && countdownText != null)
        {
            countdownText.text = "";
            if (countdownCanvas != null)
            {
                countdownCanvas.enabled = false;
            }
        }

        if (objSpawner != null)
        {
            objSpawner.StopAllCoroutines();

            if (targetPrefab == null && objSpawner.target != null)
            {
                targetPrefab = objSpawner.target;
                Debug.Log("Target prefab beállítva az ObjectSpawner-ből: " + targetPrefab.name);
            }

            if (targetPrefab != null)
            {
                objSpawner.StartCoroutine(objSpawner.spawnObject(targetPrefab));
            }
            else
            {
                Debug.LogError("Nincs beállítva target prefab a spawnolandó objektumhoz!");
            }
        }
        else
        {
            Debug.LogError("Nincs beállítva az ObjectSpawner referencia!");
        }
    }

    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (countdownCanvas != null && string.IsNullOrEmpty(countdownText.text))
        {
            countdownCanvas.enabled = false;
        }
    }
}