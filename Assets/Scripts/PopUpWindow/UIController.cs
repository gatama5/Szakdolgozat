using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ui_c : MonoBehaviour
{
    public static ui_c Instance;

    [SerializeField] private ModalWindowPanel _modalwindow;
    [SerializeField] public GameObject panel;


    public ModalWindowPanel modalwindow => _modalwindow;

    public void Close()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }
}
