
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
using TMPro;

public class ObjectSpawner_1place : MonoBehaviour
{
    public int numberToSpawn = 5; // Hány objektumot spawnoljunk
    public GameObject trg; // A spawnolandó target objektum
    public GameObject quad; // A Quad, amelynek a közepére spawnoljuk az objektumot
    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Játék befejezésekor megjelenő értesítés
    [SerializeField] TextMeshProUGUI gameOverNotificationText;
    [SerializeField] float notificationDisplayTime = 3f;

    // Számláló a megsemmisített targetek számolására
    private int destroyedTargets = 0;
    // Referencia a PickUpGun komponensre
    public PickUpGun pickUpGun;

    void Start()
    {
        // Hide notification text at start if it exists
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }

    void Update()
    {

        if (isSpawned)
        {
            timer += Time.deltaTime;
            destroyedObj(spawned);
        }
    }


    public IEnumerator spawnObject(GameObject obj)
    {
        // Reset the counter at the beginning of a new spawn session
        destroyedTargets = 0;

        // Elrejtjük az értesítést, ha látható lenne
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        // Egyesével spawnolja a célpontokat, mindig megvárva az előző megsemmisítését
        for (int i = 0; i < numberToSpawn; i++)
        {
            // Csak akkor spawnolja a következőt, ha nincs aktív célpont
            if (!isSpawned)
            {
                yield return SpawnSingleTarget(obj);
            }

            // Várunk amíg a célpont megsemmisül
            while (isSpawned)
            {
                yield return null;
            }

            // Várunk egy véletlenszerű időt a következő célpont előtt
            float randomDelay = UnityEngine.Random.Range(2f, 5f);
            yield return new WaitForSeconds(randomDelay);
        }
    }


    private IEnumerator SpawnSingleTarget(GameObject obj)
    {
        Vector3 fit_quad = quad.transform.position;

        spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
        isSpawned = true;
        timer = 0f; 

        yield return null;
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Találat! Idő spawn és találat között: " + Math.Round(timer, 2) + " mp");
            hit_times.Add(Math.Round(timer, 2)); // idő elmentése

            // Itt mentjük rögtön az adatbázisba, közvetlenül
            SaveHitToDatabase(Math.Round(timer, 2), 0, 0);

            timer = 0f;
            isSpawned = false;  // Jelezzük, hogy nincs aktív célpont

            // Növeljük a megsemmisített targetek számát
            destroyedTargets++;

            // Ellenőrizzük, hogy minden target el lett-e találva
            if (destroyedTargets >= numberToSpawn)
            {
                Debug.Log("Minden target eltalálva! Fegyver ledobása...");
                // Ha a pickUpGun referencia létezik, hívjuk meg a DropWeapon() metódust
                if (pickUpGun != null)
                {
                    pickUpGun.DropWeapon();
                }
                else
                {
                    Debug.LogError("Nincs beállítva a pickUpGun referencia az ObjectSpawner_1place szkriptben!");
                }

                // Jelenítsük meg a játék vége értesítést
                ShowGameOverNotification();
            }
        }
    }

    private void ShowGameOverNotification()
    {
        if (gameOverNotificationText != null)
        {
            // Egyszerűen megjelenítjük a szöveget, nem módosítjuk a tartalmát
            gameOverNotificationText.gameObject.SetActive(true);

            // Hide the notification after delay
            StartCoroutine(HideNotificationAfterDelay());
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDisplayTime);
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }

    private void SaveHitToDatabase(double hitTime, double posX, double posY)
    {
        SQLiteDBScript dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager != null)
        {
            // MINDIG ellenőrizzük, hogy a pozíció nem nulla-e
            if (posX == 0 && posY == 0 && hitPlace_fromMiddle.Count > 0)
            {
                // Vegyük a legutolsó pozíciót a hitPlace_fromMiddle listából
                string lastPos = hitPlace_fromMiddle[hitPlace_fromMiddle.Count - 1];
                string[] coordinates = lastPos.Split('|');
                if (coordinates.Length == 2)
                {
                    double.TryParse(coordinates[0], out posX);
                    double.TryParse(coordinates[1], out posY);
                    Debug.Log($"Pozíció felülírva a hitPlace_fromMiddle-ből: ({posX},{posY})");
                }
            }

            // Ellenőrizzük, hogy létezik-e már session
            if (dbManager.GetCurrentShootingSessionID() <= 0)
            {
                dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            }

            // Mentjük az adatot Shooting típusként
            dbManager.UpdateShootingScore(hit_times.Count, hitTime, posX, posY);
            Debug.Log($"Shooting adat elmentve az adatbázisba: idő={hitTime}, pozíció=({posX},{posY}), sorszám={hit_times.Count}");
        }
        else
        {
            Debug.LogError("Nem található SQLiteDBScript a jelenetben! Adat nem menthető.");
        }
    }
}