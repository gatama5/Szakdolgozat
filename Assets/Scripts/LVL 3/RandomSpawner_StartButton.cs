using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro névtér hozzáadása

public class RandomSpawner_StartButton : MonoBehaviour
{
    public ObjectSpawner objSpawner;       // Referencia a random ObjectSpawner-re
    public GameObject targetPrefab;         // A célpont objektum, amit spawnolni fogunk
    public PickUpGun pickUpGun;            // Referencia a fegyver felvételhez
    public AudioSource audioSource;         // Hang forrás a gomb megnyomásához
    public Gun gunScript;                   // Referencia a Gun script-hez

    [SerializeField] public float startDelay = 3f;    // Késleltetés a játék indítása elõtt
    [SerializeField] private bool showCountdown = true;    // Visszaszámlálás mutatása

    // TextMeshPro szövegdoboz referencia
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    public bool isPlaying = false;

    void Start()
    {
        // Kikapcsoljuk a visszaszámláló canvas-t
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
        isPlaying = false;

        // Ha nem adtunk meg referenciát, keressük meg az objektumokat
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
    }

    private void OnMouseDown()
    {
        // Lejátszuk a hang effektet
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Ellenõrizzük, hogy fel van-e véve a fegyver
        if (pickUpGun != null && pickUpGun.isPickedUp)
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
                // Canvas bekapcsolása, hogy látszódjon a szöveg
                if (countdownCanvas != null)
                {
                    countdownCanvas.enabled = true;
                }
                // Késleltetve eltüntetjük a hibaüzenetet
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
        // Beállítjuk a megfelelõ értékeket az ObjectSpawner-ben
        if (objSpawner != null && targetPrefab != null)
        {
            objSpawner.target = targetPrefab;

            // Ha van beállítva referencia a pickUpGun komponensre, átadjuk az ObjectSpawner-nek is
            if (pickUpGun != null)
            {
                objSpawner.pickUpGun = pickUpGun;
            }

            // Összekapcsoljuk a Gun script-et az ObjectSpawner-rel
            if (gunScript != null)
            {
                // Adjunk hozzá egy referenciát a gunScript-ben, hogy tudja, melyik ObjectSpawner-t használja
                gunScript.osp_1place = null; // Nullázzuk, mivel nem 1place-t használunk
                gunScript.pck_gun = pickUpGun;
            }
        }

        // Visszaszámlálás megjelenítése
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

        // Töröljük a visszaszámlálás szövegét
        if (showCountdown && countdownText != null)
        {
            countdownText.text = "";
            if (countdownCanvas != null)
            {
                countdownCanvas.enabled = false;
            }
        }

        // Elindítjuk a véletlenszerû célpont generálást
        if (objSpawner != null)
        {
            // Leállítjuk az esetleg már futó koroutinokat, hogy ne legyen duplikáció
            objSpawner.StopAllCoroutines();

            Debug.Log("A célpontok spawnolásának indítása...");

            // Ellenõrizzük, hogy a target prefab be lett-e állítva
            if (targetPrefab == null && objSpawner.target != null)
            {
                targetPrefab = objSpawner.target;
                Debug.Log("Target prefab beállítva az ObjectSpawner-bõl: " + targetPrefab.name);
            }

            if (targetPrefab != null)
            {
                // Elindítjuk a célpontok spawnolását
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

    // Szöveg törlése késleltetéssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Ha nincs szöveg, elrejthetjük a canvas-t is
        if (countdownCanvas != null && string.IsNullOrEmpty(countdownText.text))
        {
            countdownCanvas.enabled = false;
        }
    }
}