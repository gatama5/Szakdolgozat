using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenRedirected : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;

        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Scene loaded: Cursor is now visible");
    }

    void Update()
    {
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}