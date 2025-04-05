
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

    // Renderer komponens referencia a gombhoz
    private Renderer buttonRenderer;
    // Zöld szín a gombhoz
    [SerializeField] private Color greenColor = new Color(0.0f, 1.0f, 0.0f);

    void Start()
    {
        canva.enabled = false;
        isPlaying = false;

        // Megkeressük a Renderer komponenst és beállítjuk a zöld színt
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = greenColor;
        }
        else
        {
            // Ha nincs renderer, keressünk gyermek objektumokban
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
            // Hibaüzenet megjelenítése a szövegdobozban
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
        // Első lépés: játéktípus adatainak resetelése a Shooting játékhoz (szint=2)
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            // Meghívjuk a Reset függvényt a Shooting játékhoz (2-es szint)
            scoreManager.ResetGameTypeData(2);
        }
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
        // Célpontok spawnolásának elindítása
        if (obj_s_1p != null && trg != null)
        {
            // Biztosítjuk, hogy a korábban futó coroutine-ok le legyenek állítva
            obj_s_1p.StopAllCoroutines();
            // Ellenőrizzük, hogy a pickUpGun referencia be van-e állítva
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

    // Szöveg törlése késleltetéssel
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Ha nincs szöveg, elrejthetjük a canvas-t is
        if (canva != null && string.IsNullOrEmpty(countdownText.text))
        {
            canva.enabled = false;
        }
    }
}