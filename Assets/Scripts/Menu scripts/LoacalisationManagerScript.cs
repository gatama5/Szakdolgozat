using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LoacalisationManagerScript : MonoBehaviour
{

    private bool active = false;

    void Start()
    {
        //int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        
        ChangeLocale(getLocal());
    }

    public void ChangeLocale(int localeID)
    {
        if(active == true)
            return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localeID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        active = false;
    }

    public int getLocal()
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        return LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
    }

    void Update()
    {
        
    }
}
