using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro nйvtйr hozzбadбsa

public class RandomSpawner_StartButton : MonoBehaviour
{
    public ObjectSpawner objSpawner;       // Referencia a random ObjectSpawner-re
    public GameObject targetPrefab;         // A cйlpont objektum, amit spawnolni fogunk
    public PickUpGun pickUpGun;            // Referencia a fegyver felvйtelhez
    public AudioSource audioSource;         // Hang forrбs a gomb megnyomбsбhoz
    public Gun gunScript;                   // Referencia a Gun script-hez

    [SerializeField] public float startDelay = 3f;    // Kйsleltetйs a jбtйk indнtбsa elхtt
    [SerializeField] private bool showCountdown = true;    // Visszaszбmlбlбs mutatбsa

    // TextMeshPro szцvegdoboz referencia
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    public bool isPlaying = false;

    void Start()
    {
        // Kikapcsoljuk a visszaszбmlбlу canvas-t
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
        isPlaying = false;

        // Ha nem adtunk meg referenciбt, keressьk meg az objektumokat
        if (objSpawner == null)
        {
            objSpawner = FindObjectOfType<ObjectSpawner>();
            if (objSpawner == null)
            {
                Debug.LogError("Nem talбlhatу ObjectSpawner objektum a jбtйkban!");
            }
        }

        if (pickUpGun == null)
        {
            pickUpGun = FindObjectOfType<PickUpGun>();
            if (pickUpGun == null)
            {
                Debug.LogWarning("Nem talбlhatу PickUpGun objektum a jбtйkban!");
            }
        }

        if (gunScript == null)
        {
            gunScript = FindObjectOfType<Gun>();
        }
    }

    private void OnMouseDown()
    {
        // Lejбtszuk a hang effektet
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Ellenхrizzьk, hogy fel van-e vйve a fegyver
        if (pickUpGun != null && pickUpGun.isPickedUp)
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
                // Canvas bekapcsolбsa, hogy lбtszуdjon a szцveg
                if (countdownCanvas != null)
                {
                    countdownCanvas.enabled = true;
                }
                // Kйsleltetve eltьntetjьk a hibaьzenetet
                StartCoroutine(ClearTextAfterDelay(2f));
            }
            else
            {
                Debug.Log("Kйrem vegye fel a fegyvert a kezdйshez!");
            }
        }
    }

    public IEnumerator StartPlay()
    {
        // Beбllнtjuk a megfelelх йrtйkeket az ObjectSpawner-ben
        if (objSpawner != null && targetPrefab != null)
        {
            objSpawner.target = targetPrefab;

            // Ha van beбllнtva referencia a pickUpGun komponensre, бtadjuk az ObjectSpawner-nek is
            if (pickUpGun != null)
            {
                objSpawner.pickUpGun = pickUpGun;
            }

            // Цsszekapcsoljuk a Gun script-et az ObjectSpawner-rel
            if (gunScript != null)
            {
                // Adjunk hozzб egy referenciбt a gunScript-ben, hogy tudja, melyik ObjectSpawner-t hasznбlja
                //gunScript.osp_1place = null; // Nullбzzuk, mivel nem 1place-t hasznбlunk
                gunScript.pck_gun = pickUpGun;
            }
        }

        // Visszaszбmlбlбs megjelenнtйse
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

        // Tцrцljьk a visszaszбmlбlбs szцvegйt
        if (showCountdown && countdownText != null)
        {
            countdownText.text = "";
            if (countdownCanvas != null)
            {
                countdownCanvas.enabled = false;
            }
        }

        // Elindнtjuk a vйletlenszerы cйlpont generбlбst
        if (objSpawner != null)
        {
            // Leбllнtjuk az esetleg mбr futу koroutinokat, hogy ne legyen duplikбciу
            objSpawner.StopAllCoroutines();

            // Ellenхrizzьk, hogy a target prefab be lett-e бllнtva
            if (targetPrefab == null && objSpawner.target != null)
            {
                targetPrefab = objSpawner.target;
                Debug.Log("Target prefab beбllнtva az ObjectSpawner-bхl: " + targetPrefab.name);
            }

            if (targetPrefab != null)
            {
                // Elindнtjuk a cйlpontok spawnolбsбt
                objSpawner.StartCoroutine(objSpawner.spawnObject(targetPrefab));
            }
            else
            {
                Debug.LogError("Nincs beбllнtva target prefab a spawnolandу objektumhoz!");
            }
        }
        else
        {
            Debug.LogError("Nincs beбllнtva az ObjectSpawner referencia!");
        }
    }

    // Szцveg tцrlйse kйsleltetйssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Ha nincs szцveg, elrejthetjьk a canvas-t is
        if (countdownCanvas != null && string.IsNullOrEmpty(countdownText.text))
        {
            countdownCanvas.enabled = false;
        }
    }
}