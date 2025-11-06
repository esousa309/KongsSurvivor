
using UnityEngine;

public static class CurrencyManager
{
    private const string KEY = "OMEGA_TOTAL";

    public static int TotalOmega { get; private set; }

    static CurrencyManager()
    {
        Load();
    }

    public static void Load()
    {
        TotalOmega = PlayerPrefs.GetInt(KEY, 0);
    }

    public static void Save()
    {
        PlayerPrefs.SetInt(KEY, TotalOmega);
        PlayerPrefs.Save();
    }

    public static void AddOmega(int amount)
    {
        TotalOmega = Mathf.Max(0, TotalOmega + amount);
        Save();
    }

    public static void ResetTotal()
    {
        TotalOmega = 0;
        Save();
    }
}
