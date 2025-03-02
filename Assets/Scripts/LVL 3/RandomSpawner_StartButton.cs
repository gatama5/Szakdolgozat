using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro n�vt�r hozz�ad�sa

public class RandomSpawner_StartButton : MonoBehaviour
{
    public ObjectSpawner objSpawner;       // Referencia a random ObjectSpawner-re
    public GameObject targetPrefab;         // A c�lpont objektum, amit spawnolni fogunk
    public PickUpGun pickUpGun;            // Referencia a fegyver felv�telhez
    public AudioSource audioSource;         // Hang forr�s a gomb megnyom�s�hoz
    public Gun gunScript;                   // Referencia a Gun script-hez

    [SerializeField] public float startDelay = 3f;    // K�sleltet�s a j�t�k ind�t�sa el�tt
    [SerializeField] private bool showCountdown = true;    // Visszasz�ml�l�s mutat�sa

    // TextMeshPro sz�vegdoboz referencia
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    public bool isPlaying = false;

    void Start()
    {
        // Kikapcsoljuk a visszasz�ml�l� canvas-t
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
        isPlaying = false;

        // Ha nem adtunk meg referenci�t, keress�k meg az objektumokat
        if (objSpawner == null)
        {
            objSpawner = FindObjectOfType<ObjectSpawner>();
            if (objSpawner == null)
            {
                Debug.LogError("Nem tal�lhat� ObjectSpawner objektum a j�t�kban!");
            }
        }

        if (pickUpGun == null)
        {
            pickUpGun = FindObjectOfType<PickUpGun>();
            if (pickUpGun == null)
            {
                Debug.LogWarning("Nem tal�lhat� PickUpGun objektum a j�t�kban!");
            }
        }

        if (gunScript == null)
        {
            gunScript = FindObjectOfType<Gun>();
        }
    }

    private void OnMouseDown()
    {
        // Lej�tszuk a hang effektet
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Ellen�rizz�k, hogy fel van-e v�ve a fegyver
        if (pickUpGun != null && pickUpGun.isPickedUp)
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
                // Canvas bekapcsol�sa, hogy l�tsz�djon a sz�veg
                if (countdownCanvas != null)
                {
                    countdownCanvas.enabled = true;
                }
                // K�sleltetve elt�ntetj�k a hiba�zenetet
                StartCoroutine(ClearTextAfterDelay(2f));
            }
            else
            {
                Debug.Log("K�rem vegye fel a fegyvert a kezd�shez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        // Be�ll�tjuk a megfelel� �rt�keket az ObjectSpawner-ben
        if (objSpawner != null && targetPrefab != null)
        {
            objSpawner.target = targetPrefab;

            // Ha van be�ll�tva referencia a pickUpGun komponensre, �tadjuk az ObjectSpawner-nek is
            if (pickUpGun != null)
            {
                objSpawner.pickUpGun = pickUpGun;
            }

            // �sszekapcsoljuk a Gun script-et az ObjectSpawner-rel
            if (gunScript != null)
            {
                // Adjunk hozz� egy referenci�t a gunScript-ben, hogy tudja, melyik ObjectSpawner-t haszn�lja
                gunScript.osp_1place = null; // Null�zzuk, mivel nem 1place-t haszn�lunk
                gunScript.pck_gun = pickUpGun;
            }
        }

        // Visszasz�ml�l�s megjelen�t�se
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

        // T�r�lj�k a visszasz�ml�l�s sz�veg�t
        if (showCountdown && countdownText != null)
        {
            countdownText.text = "";
            if (countdownCanvas != null)
            {
                countdownCanvas.enabled = false;
            }
        }

        // Elind�tjuk a v�letlenszer� c�lpont gener�l�st
        if (objSpawner != null)
        {
            // Le�ll�tjuk az esetleg m�r fut� koroutinokat, hogy ne legyen duplik�ci�
            objSpawner.StopAllCoroutines();

            Debug.Log("A c�lpontok spawnol�s�nak ind�t�sa...");

            // Ellen�rizz�k, hogy a target prefab be lett-e �ll�tva
            if (targetPrefab == null && objSpawner.target != null)
            {
                targetPrefab = objSpawner.target;
                Debug.Log("Target prefab be�ll�tva az ObjectSpawner-b�l: " + targetPrefab.name);
            }

            if (targetPrefab != null)
            {
                // Elind�tjuk a c�lpontok spawnol�s�t
                objSpawner.StartCoroutine(objSpawner.spawnObject(targetPrefab));
            }
            else
            {
                Debug.LogError("Nincs be�ll�tva target prefab a spawnoland� objektumhoz!");
            }
        }
        else
        {
            Debug.LogError("Nincs be�ll�tva az ObjectSpawner referencia!");
        }
    }

    // Sz�veg t�rl�se k�sleltet�ssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Ha nincs sz�veg, elrejthetj�k a canvas-t is
        if (countdownCanvas != null && string.IsNullOrEmpty(countdownText.text))
        {
            countdownCanvas.enabled = false;
        }
    }
}