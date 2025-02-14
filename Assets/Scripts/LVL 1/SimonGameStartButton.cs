using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonGameStartButton : MonoBehaviour
{
     public SimonGameManager gm;
     public SimonSaysButton[] buttons;
     public AudioSource src;
     [SerializeField] public float start_delay = 3f;

    void OnMouseDown()
    {
        //for (int i = 0; i < buttons.Length; i++)
        //{
        //    buttons[i].GetComponent<MeshRenderer>().material.color = buttons[i].defaultColor;
        //}
        src.Play();
        gm.isEnded = false;
        gm.ResetGame();
        gm.StartCoroutine(gm.PlayGame());
    }
}
