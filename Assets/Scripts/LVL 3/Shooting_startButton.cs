
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//public class Shooting_startButton : MonoBehaviour
//{
//    public ObjectSpawner_1place obj_s_1p;
//    public GameObject trg;
//    public PickUpGun pug;
//    public AudioSource src;
//    [SerializeField] public float startDelay = 3f; // �tnevezve �s publikusra �ll�tva
//    [SerializeField] private bool showCountdown = true; // Opcion�lis visszasz�ml�l�s mutat�sa

//    private void OnMouseDown()
//    {
//        src.Play();
//        if (pug.isPickedUp)
//        {
//            StartCoroutine(StartPlay());
//        }
//        else
//        {
//            Debug.Log("K�rem vegye fel a fegyvert a kezd�shez!");
//        }
//    }

//    public IEnumerator StartPlay()
//    {
//        // Visszasz�ml�l�s megjelen�t�se
//        float remainingTime = startDelay;
//        while (remainingTime > 0)
//        {
//            if (showCountdown)
//            {
//                Debug.Log("J�t�k indul: " + Mathf.Ceil(remainingTime) + " m�sodperc m�lva");
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
using TMPro; // TextMeshPro n�vt�r hozz�ad�sa

public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f;
    [SerializeField] private bool showCountdown = true;

    // TextMeshPro sz�vegdoboz referencia
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
            // Hiba�zenet megjelen�t�se a sz�vegdobozban
            if (countdownText != null)
            {
                countdownText.text = "K�rem vegye fel a fegyvert a kezd�shez!";
            }
            else
            {
                Debug.Log("K�rem vegye fel a fegyvert a kezd�shez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        // Visszasz�ml�l�s megjelen�t�se
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

    // Sz�veg t�rl�se k�sleltet�ssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }
}