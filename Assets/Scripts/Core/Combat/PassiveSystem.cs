using UnityEngine;

// 4.4: pasivas por clase que escalan con el stat que cada build apila.
// El jugador puede EQUIPAR/DESEQUIPAR la pasiva (persistente entre sesiones).
public static class PassiveSystem
{
    const string PREF_KEY = "PassiveEnabled";

    public static bool Enabled
    {
        get { return PlayerPrefs.GetInt(PREF_KEY, 1) == 1; }
    }

    public static void SetEnabled(bool on)
    {
        PlayerPrefs.SetInt(PREF_KEY, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void Toggle()
    {
        SetEnabled(!Enabled);
    }

    public static string Name(ClassRole role)
    {
        switch (role)
        {
            case ClassRole.Tank: return "Coloso";
            case ClassRole.Healer: return "Plegaria Ofensiva";
            default: return "Ojos del Ejecutor";
        }
    }

    public static string Describe(ClassRole role)
    {
        switch (role)
        {
            case ClassRole.Tank: return "Coloso: +1 de daño por cada 10 de HP máximo.";
            case ClassRole.Healer: return "Plegaria Ofensiva: +1 de daño por cada 10 de Curación y te cura al golpear.";
            default: return "Ojos del Ejecutor: +1 de daño por cada 5% de crítico.";
        }
    }

    public static int BonusDamage(ClassRole role, Unit player)
    {
        if (!Enabled || player == null) return 0;
        switch (role)
        {
            case ClassRole.Tank: return player.stats.maxHP / 10;
            case ClassRole.Healer: return player.stats.healingPower / 10;
            default: return player.stats.critChance / 5;
        }
    }

    public static void OnPlayerAttack(ClassRole role, Unit player, Unit target)
    {
        if (!Enabled) return;
        if (role == ClassRole.Healer && player != null)
        {
            int heal = BonusDamage(role, player) / 2;
            if (heal > 0) player.Heal(heal);
        }
    }
}