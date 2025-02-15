using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private ModalWindowPanel _modalwindow;

    public ModalWindowPanel modalwindow => _modalwindow;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        
    }
}
