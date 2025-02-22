
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f; // Átnevezve és publikusra állítva
    [SerializeField] private bool showCountdown = true; // Opcionális visszaszámlálás mutatása

    private void OnMouseDown()
    {
        src.Play();
        if (pug.isPickedUp)
        {
            StartCoroutine(StartPlay());
        }
        else
        {
            Debug.Log("Kérem vegye fel a fegyvert a kezdéshez!");
        }
    }

    public IEnumerator StartPlay()
    {
        // Visszaszámlálás megjelenítése
        float remainingTime = startDelay;
        while (remainingTime > 0)
        {
            if (showCountdown)
            {
                Debug.Log("Játék indul: " + Mathf.Ceil(remainingTime) + " másodperc múlva");
            }
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (showCountdown)
        {
            Debug.Log("Start!");
        }

        obj_s_1p.StartCoroutine(obj_s_1p.spawnObject(trg));
    }
}


