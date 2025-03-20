using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class WindowBOxTrigger : MonoBehaviour
{
    public string title;
    public string message;
    public bool triggerOnEnable;

    public void OnEnable()
    {
        if (!triggerOnEnable || ui_c.Instance == null)
        {
            return;
        }
        ui_c.Instance.modalwindow.ShowAsHero();
    }

}

