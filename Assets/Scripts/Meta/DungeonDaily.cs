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

    public static void Consume()
    {
        PlayerPrefs.SetString(K_DATE, Today());
        PlayerPrefs.SetInt(K_COUNT, Count + 1);
        PlayerPrefs.Save();
    }
}