﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("Hбny target objektumot spawnoljon")]
    public int numberToSpawn = 5;

    [Tooltip("A spawnolandу target objektum")]
    public GameObject target;

    [Tooltip("A quad, aminek a terьletйn belьl spawnolunk")]
    public GameObject quad;

    [Tooltip("Vбrakozбsi idх a kцvetkezх target spawnolбsбig")]
    public float spawnDelay = 2.0f;

    [Tooltip("Minimбlis tбvolsбg kйt target kцzцtt")]
    public float minDistanceBetweenTargets = 1.0f;

    [SerializeField] TextMeshProUGUI gameOverNotificationText;
    [SerializeField] float notificationDisplayTime = 3f;

    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    public PickUpGun pickUpGun;

    private List<GameObject> activeTargets = new List<GameObject>();

    public int destroyedTargets = 0;

    private List<float> spawnTimes = new List<float>();

    void Start()
    {
        hit_times = new List<double>();
        hitPlace_fromMiddle = new List<string>();

        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        CheckDestroyedTargets();
    }


    private void CheckDestroyedTargets()
    {
        bool anyDestroyed = false;

        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] == null)
            {
                if (i < spawnTimes.Count && spawnTimes[i] > 0)
                {
                    double hitTime = Math.Round(Time.time - spawnTimes[i], 2);

                    bool alreadyExists = false;
                    foreach (double time in hit_times)
                    {
                        if (Math.Abs(time - hitTime) < 0.01)
                        {
                            alreadyExists = true;
                            Debug.Log($"Duplikált találat kihagyva: {hitTime}");
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        hit_times.Add(hitTime);
                        Debug.Log("Találat feldolgozva! Idő spawn és találat között: " + hitTime + " mp");

                    }
                }

                activeTargets.RemoveAt(i);
                if (i < spawnTimes.Count)
                    spawnTimes.RemoveAt(i);
                destroyedTargets++;
                anyDestroyed = true;
            }
        }

        if (anyDestroyed && activeTargets.Count == 0 && destroyedTargets >= numberToSpawn)
        {
            if (pickUpGun != null)
            {
                pickUpGun.DropWeapon();
            }

            SaveHitResults();

            ShowGameOverNotification();
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

    private void SaveHitResults()
    {
        for (int i = 0; i < hit_times.Count; i++)
        {
            double hitTime = hit_times[i];
            double posX = 0;
            double posY = 0;

            if (i < hitPlace_fromMiddle.Count)
            {
                string posInfo = hitPlace_fromMiddle[i];
                string[] coordinates = posInfo.Split('|');
                if (coordinates.Length == 2)
                {
                    double.TryParse(coordinates[0], out posX);
                    double.TryParse(coordinates[1], out posY);
                }
            }

            SaveHitToDatabase(hitTime, posX, posY);
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

            int shotNumber = hit_times.Count;

            dbManager.UpdateTargetScore(shotNumber, hitTime, posX, posY);
            Debug.Log($"Target adat elmentve az adatbázisba: idő={hitTime}, pozíció=({posX},{posY}), sorszám={shotNumber}");
        }
        else
        {
            Debug.LogError("Nem található SQLiteDBScript a jelenetben! Adat nem menthető.");
        }
    }
    public IEnumerator spawnObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            yield break;
        }

        if (quad == null)
        {
            yield break;
        }

        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            yield break;
        }

        destroyedTargets = 0;
        activeTargets.Clear();
        spawnTimes.Clear();
        hit_times.Clear(); 


        if (gameOverNotificationText != null)
        {
            gameOverNotificationText.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector3 spawnPosition = FindValidSpawnPosition(meshCollider);

            GameObject spawnedTarget = Instantiate(targetObject, spawnPosition, quad.transform.rotation);

            Target targetComponent = spawnedTarget.GetComponentInChildren<Target>();
            if (targetComponent == null)
            {
                targetComponent = spawnedTarget.AddComponent<Target>();
            }

            activeTargets.Add(spawnedTarget);
            spawnTimes.Add(Time.time);

            float randomDelay = UnityEngine.Random.Range(spawnDelay * 0.75f, spawnDelay * 1.25f);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    private Vector3 FindValidSpawnPosition(MeshCollider collider)
    {
        float quadX, quadY;
        Vector3 position;
        bool positionValid = false;
        int attempts = 0;
        const int maxAttempts = 30;

        position = new Vector3(quad.transform.position.x, quad.transform.position.y, quad.transform.position.z);

        while (!positionValid && attempts < maxAttempts)
        {
            quadX = UnityEngine.Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            quadY = UnityEngine.Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            position = new Vector3(quadX, quadY, quad.transform.position.z);

            positionValid = IsPositionValid(position);
            attempts++;
        }

        return position;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject obj in activeTargets)
        {
            if (obj != null && Vector3.Distance(position, obj.transform.position) < minDistanceBetweenTargets)
            {
                return false;
            }
        }
        return true;
    }
}