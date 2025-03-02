using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelectorButton : MonoBehaviour
{
    [Tooltip("A nyelv azonos�t�ja (0 = angol, 1 = magyar)")]
    [SerializeField] private int languageID = 0;

    private Button button;

    void Start()
    {
        // Gomb komponens megkeres�se
        button = GetComponent<Button>();

        if (button != null)
        {
            // Click esem�ny hozz�ad�sa
            button.onClick.AddListener(ChangeLanguage);
        }
        else
        {
            Debug.LogError("A LanguageSelectorButton script-hez Button komponens sz�ks�ges!", this);
        }
    }

    // Nyelv v�ltoztat�sa a LoacalisationManagerScript seg�ts�g�vel
    private void ChangeLanguage()
    {
        // Singleton instance el�r�se
        if (LoacalisationManagerScript.Instance != null)
        {
            LoacalisationManagerScript.Instance.ChangeLocale(languageID);
            //Debug.Log($"Nyelv v�ltoztatva: {languageID} (0=angol, 1=magyar)");
        }
        else
        {
            Debug.LogError("LoacalisationManagerScript nem tal�lhat�! Ellen�rizd, hogy l�tezik-e a LoacalisationManager GameObject a sz�npadon!", this);
        }
    }
}