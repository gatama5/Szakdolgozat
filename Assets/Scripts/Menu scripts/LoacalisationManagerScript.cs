using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LoacalisationManagerScript : MonoBehaviour
{

    private bool active = false;

    public static LoacalisationManagerScript Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
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
