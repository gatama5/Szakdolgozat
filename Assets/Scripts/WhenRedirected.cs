using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenRedirected : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Kurzor láthatóvá tétele
        Cursor.visible = true;

        // Kurzor feloldása (ha esetleg zárolva volt a játékban)
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Scene loaded: Cursor is now visible");
    }

    // Update is called once per frame
    void Update()
    {
        // Ha bármilyen okból a kurzor újra láthatatlanná válna, biztosítjuk, hogy látható maradjon
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}