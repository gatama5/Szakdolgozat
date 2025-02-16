using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ui_c : MonoBehaviour
{
    public static ui_c Instance;

    [SerializeField] private ModalWindowPanel _modalwindow;
    [SerializeField] private GameObject panel;


    public ModalWindowPanel modalwindow => _modalwindow;

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}


    public void Close()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }
}
