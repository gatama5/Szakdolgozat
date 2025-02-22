
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float startDelay = 3f; // �tnevezve �s publikusra �ll�tva
    [SerializeField] private bool showCountdown = true; // Opcion�lis visszasz�ml�l�s mutat�sa

    private void OnMouseDown()
    {
        src.Play();
        if (pug.isPickedUp)
        {
            StartCoroutine(StartPlay());
        }
        else
        {
            Debug.Log("K�rem vegye fel a fegyvert a kezd�shez!");
        }
    }

    public IEnumerator StartPlay()
    {
        // Visszasz�ml�l�s megjelen�t�se
        float remainingTime = startDelay;
        while (remainingTime > 0)
        {
            if (showCountdown)
            {
                Debug.Log("J�t�k indul: " + Mathf.Ceil(remainingTime) + " m�sodperc m�lva");
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


