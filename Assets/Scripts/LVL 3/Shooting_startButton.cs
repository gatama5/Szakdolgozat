using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro nйvtйr hozzбadбsa

public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f;
    [SerializeField] private bool showCountdown = true;

    // TextMeshPro szцvegdoboz referencia
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas canva;


    public int countOfObjectTillEnd = 5;

    public bool isPlaying = false;

    void Start()
    {
        canva.enabled = false;
        isPlaying = false;
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
            // Hibaьzenet megjelenнtйse a szцvegdobozban
            if (countdownText != null)
            {
                countdownText.text = "Kйrem vegye fel a fegyvert a kezdйshez!";
            }
            else
            {
                Debug.Log("Kйrem vegye fel a fegyvert a kezdйshez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        // Visszaszбmlбlбs megjelenнtйse
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
        obj_s_1p.StartCoroutine(obj_s_1p.spawnObject(trg));

    }

    // Szцveg tцrlйse kйsleltetйssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }
}