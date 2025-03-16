using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenRedirected : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Kurzor l�that�v� t�tele
        Cursor.visible = true;

        // Kurzor felold�sa (ha esetleg z�rolva volt a j�t�kban)
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Scene loaded: Cursor is now visible");
    }

    // Update is called once per frame
    void Update()
    {
        // Ha b�rmilyen okb�l a kurzor �jra l�thatatlann� v�lna, biztos�tjuk, hogy l�that� maradjon
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}