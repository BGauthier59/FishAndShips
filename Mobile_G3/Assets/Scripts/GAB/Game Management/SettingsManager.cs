using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoSingleton<SettingsManager>
{
    #region Language

    private static Language _gameLanguage;

    public static Language GetGameLanguage()
    {
        return _gameLanguage;
    }

    public static void SetGameLanguage(Language language)
    {
        _gameLanguage = language;
    }

    #endregion

    #region Volume

    

    #endregion

    #region Level of Quality

    

    #endregion

    #region Rumble

    

    #endregion
}