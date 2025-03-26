using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalWindowPanel : MonoBehaviour
{

    [SerializeField] public Button apply_btn;
    [SerializeField] public GameObject modalWindow; // A bezárandó ablak GameObject-je
    [SerializeField] public TriggerZoneForWindow trg_zone;



    private Action onApplyAction;
    ui_c ui_c;

    private void Awake()
    {
        apply_btn.onClick.AddListener(CloseModalButton);
    }



    public void CloseModalButton()
    {
        if (modalWindow != null)
        {
            modalWindow.SetActive(false);
            if (trg_zone != null)
            {
                trg_zone.OnApplyPressed();
            }
        }
    }

    public void ShowAsHero()
    {
        ui_c.Instance.Show();
    }

}


