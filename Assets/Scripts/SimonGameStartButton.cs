using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonGameStartButton : MonoBehaviour
{
    [SerializeField] SimonGameManager gm;
    [SerializeField] SimonSaysButton[] buttons;

    void OnMouseDown()
    {
        //for (int i = 0; i < buttons.Length; i++)
        //{
        //    buttons[i].GetComponent<MeshRenderer>().material.color = buttons[i].defaultColor;
        //}
        gm.ResetGame();
        gm.StartCoroutine("PlayGame");
    }
}
