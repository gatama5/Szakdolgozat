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
        if (!triggerOnEnable)
        {
            return;
        }
        UIController.Instance.modalwindow.ShowAsHero(title, message, null);
    }
}
