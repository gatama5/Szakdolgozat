
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f;
    [SerializeField] private bool showCountdown = true;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas canva;
    public int countOfObjectTillEnd = 5;
    public bool isPlaying = false;

    private Renderer buttonRenderer;
    [SerializeField] private Color greenColor = new Color(0.0f, 1.0f, 0.0f);

    void Start()
    {
        canva.enabled = false;
        isPlaying = false;

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
        src.Play();
        if (pug.isPickedUp)
        {
            isPlaying = true;
            StartCoroutine(StartPlay());
        }
        else
        {
            if (countdownText != null)
            {
                countdownText.text = "Kérem vegye fel a fegyvert a kezdéshez";
                canva.enabled = true;
                StartCoroutine(ClearTextAfterDelay(2f));
            }
            else
            {
                Debug.Log("Kérem vegye fel a fegyvert a kezdéshez");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetGameTypeData(2);
        }
        float remainingTime = startDelay;
        canva.enabled = true;
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
            StartCoroutine(ClearTextAfterDelay(0));
        }
        if (obj_s_1p != null && trg != null)
        {

            obj_s_1p.StopAllCoroutines();
            if (pug != null)
            {
                obj_s_1p.pickUpGun = pug;
            }
            obj_s_1p.StartCoroutine(obj_s_1p.spawnObject(trg));
        }
        else
        {
            Debug.LogError("Hiányzó objektum referenciák a Shooting játékhoz!");
        }
    }

    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (canva != null && string.IsNullOrEmpty(countdownText.text))
        {
            canva.enabled = false;
        }
    }
}