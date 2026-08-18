using UnityEngine;

// 5.2: límite diario de mazmorras de zona (5 por día real)
public static class DungeonDaily
{
    public const int MaxPerDay = 5;
    const string K_DATE = "DungeonDate";
    const string K_COUNT = "DungeonCount";

    static string Today()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd");
    }

    public static int Count
    {
        get
        {
            if (PlayerPrefs.GetString(K_DATE, "") != Today()) return 0;
            return PlayerPrefs.GetInt(K_COUNT, 0);
        }
    }

    public static int Remaining()
    {
        return MaxPerDay - Count;
    }

    public static bool CanEnter()
    {
        return Remaining() > 0;
    }

    // 2.1-fix: leer el contador ANTES de sobrescribir la fecha
    // (evita sumar sobre el valor crudo del día anterior tras el reset)
    public static void Consume()
    {
        int prev = Count; // 0 si es día nuevo (reset correcto)
        PlayerPrefs.SetString(K_DATE, Today());
        PlayerPrefs.SetInt(K_COUNT, Mathf.Min(prev + 1, MaxPerDay));
        PlayerPrefs.Save();
    }

    // DEBUG: reinicia el contador de hoy (F9 en el Tablón de Misiones)
    public static void ResetToday()
    {
        PlayerPrefs.SetString(K_DATE, Today());
        PlayerPrefs.SetInt(K_COUNT, 0);
        PlayerPrefs.Save();
        Debug.Log("[DungeonDaily] DEBUG: contador de hoy reiniciado.");
    }
}