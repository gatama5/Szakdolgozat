
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
    public int numberToSpawn = 5;
    public GameObject trg; 
    public GameObject quad; 
    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    [SerializeField] TextMeshProUGUI gameOverNotificationText;
    [SerializeField] float notificationDisplayTime = 3f;

    private int destroyedTargets = 0;
    public PickUpGun pickUpGun;

    void Start()
    {
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
        destroyedTargets = 0;
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            if (!isSpawned)
            {
                yield return SpawnSingleTarget(obj);
            }
            while (isSpawned)
            {
                yield return null;
            }

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
            hit_times.Add(Math.Round(timer, 2));

            SaveHitToDatabase(Math.Round(timer, 2), 0, 0);

            timer = 0f;
            isSpawned = false;

            destroyedTargets++;

            if (destroyedTargets >= numberToSpawn)
            {
                Debug.Log("Minden target eltalálva! Fegyver ledobása...");
                if (pickUpGun != null)
                {
                    pickUpGun.DropWeapon();
                }
                else
                {
                    Debug.LogError("Nincs beállítva a pickUpGun referencia az ObjectSpawner_1place szkriptben!");
                }
                ShowGameOverNotification();
            }
        }
    }

    private void ShowGameOverNotification()
    {
        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(true);

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
            if (posX == 0 && posY == 0 && hitPlace_fromMiddle.Count > 0)
            {
                string lastPos = hitPlace_fromMiddle[hitPlace_fromMiddle.Count - 1];
                string[] coordinates = lastPos.Split('|');
                if (coordinates.Length == 2)
                {
                    double.TryParse(coordinates[0], out posX);
                    double.TryParse(coordinates[1], out posY);
                    Debug.Log($"Pozíció felülírva a hitPlace_fromMiddle-ből: ({posX},{posY})");
                }
            }

            if (dbManager.GetCurrentShootingSessionID() <= 0)
            {
                dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            }

            dbManager.UpdateShootingScore(hit_times.Count, hitTime, posX, posY);
            Debug.Log($"Shooting adat elmentve az adatbázisba: idő={hitTime}, pozíció=({posX},{posY}), sorszám={hit_times.Count}");
        }
        else
        {
            Debug.LogError("Nem található SQLiteDBScript a jelenetben! Adat nem menthető.");
        }
    }
}