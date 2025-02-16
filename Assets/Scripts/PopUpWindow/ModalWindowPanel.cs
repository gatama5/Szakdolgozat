using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalWindowPanel : MonoBehaviour
{

    [SerializeField] private Button apply_btn;
    [SerializeField] public GameObject modalWindow; // A bezárandó ablak GameObject-je
    [SerializeField] public TriggerZoneForWindow trg_zone;
    //public FPS_Controller playerController;



    private Action onApplyAction;
    ui_c ui_c;

    private void Awake()
    {
        apply_btn.onClick.AddListener(CloseModalButton);
        //trg_zone = GetComponent<TriggerZoneForWindow>();
    }

    //public void Confirm()
    //{
    //    onApplyAction?.Invoke();
    //    ui_c.Instance.Close();
    //}

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
        Debug.Log("megjelenítés");
        ui_c.Instance.Show();
    }

}
