
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//public class Shooting_startButton : MonoBehaviour
//{
//    public ObjectSpawner_1place obj_s_1p;
//    public GameObject trg;
//    public PickUpGun pug;
//    public AudioSource src;
//    [SerializeField] public float startDelay = 3f; // Átnevezve és publikusra állítva
//    [SerializeField] private bool showCountdown = true; // Opcionális visszaszámlálás mutatása

//    private void OnMouseDown()
//    {
//        src.Play();
//        if (pug.isPickedUp)
//        {
//            StartCoroutine(StartPlay());
//        }
//        else
//        {
//            Debug.Log("Kérem vegye fel a fegyvert a kezdéshez!");
//        }
//    }

//    public IEnumerator StartPlay()
//    {
//        // Visszaszámlálás megjelenítése
//        float remainingTime = startDelay;
//        while (remainingTime > 0)
//        {
//            if (showCountdown)
//            {
//                Debug.Log("Játék indul: " + Mathf.Ceil(remainingTime) + " másodperc múlva");
//            }
//            yield return new WaitForSeconds(1f);
//            remainingTime -= 1f;
//        }

//        if (showCountdown)
//        {
//            Debug.Log("Start!");
//        }

//        obj_s_1p.StartCoroutine(obj_s_1p.spawnObject(trg));
//    }
//}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro névtér hozzáadása

public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f;
    [SerializeField] private bool showCountdown = true;

    // TextMeshPro szövegdoboz referencia
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
            // Hibaüzenet megjelenítése a szövegdobozban
            if (countdownText != null)
            {
                countdownText.text = "Kérem vegye fel a fegyvert a kezdéshez!";
            }
            else
            {
                Debug.Log("Kérem vegye fel a fegyvert a kezdéshez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        // Visszaszámlálás megjelenítése
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

    // Szöveg törlése késleltetéssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }
}