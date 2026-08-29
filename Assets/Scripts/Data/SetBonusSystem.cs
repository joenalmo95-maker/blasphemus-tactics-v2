using UnityEngine;

// 0.7-E + 1.7: sistema de sets con bonos por 2 y 4 piezas
// Rojo = DPS sostenido, Amarillo = ejecutor critico, Verde = tanque
public static class SetBonusSystem
{
    public static string SetName(SetType s)
    {
        switch (s)
        {
            case SetType.Rojo: return "Set del Juicio";
            case SetType.Amarillo: return "Set del Halo";
            case SetType.Verde: return "Set de la Plegaria";
            default: return "";
        }
    }

    public static string SetSuffix(SetType s)
    {
        switch (s)
        {
            case SetType.Rojo: return "del Juicio";
            case SetType.Amarillo: return "del Halo";
            case SetType.Verde: return "de la Plegaria";
            default: return "";
        }
    }

    public static Color SetColor(SetType s)
    {
        switch (s)
        {
            case SetType.Rojo: return Color.red;
            case SetType.Amarillo: return Color.yellow;
            case SetType.Verde: return Color.green;
            default: return Color.gray;
        }
    }

    public static string PieceName(SetPieceType p)
    {
        switch (p)
        {
            case SetPieceType.Casco: return "Casco";
            case SetPieceType.Peto: return "Peto";
            case SetPieceType.Pantalon: return "Pantalón";
            case SetPieceType.Guantes: return "Guantes";
            default: return "";
        }
    }

    public static string BonusDescription(SetType s)
    {
        switch (s)
        {
            case SetType.Rojo: return "(2/4) +5% daño, +5% crítico | (4/4) +10% daño, +5% lifesteal";
            case SetType.Amarillo: return "(2/4) +5% crítico, Umbral 35% HP (15% prob) | (4/4) +10% crítico, Umbral 10% HP (20% prob)";
            case SetType.Verde: return "(2/4) +50 HP, +2 DEF | (4/4) +100 HP, +5 HP/golpe, pociones +25%, +3% evasión";
            default: return "";
        }
    }

    public static int CountPieces(SetType set)
    {
        if (InventorySystem.Instance == null || set == SetType.Ninguno) return 0;
        int count = 0;
        foreach (ItemData item in InventorySystem.Instance.GetAllEquipped())
        {
            if (item != null && item.setId == set && item.setPiece != SetPieceType.Ninguna) count++;
        }
        return count;
    }

    public static bool Has2Pieces(SetType set) => CountPieces(set) >= 2;
    public static bool HasFullSet(SetType set) => CountPieces(set) >= 4;

    // === ROJO: DPS sostenido (2 piezas = base, 4 piezas = lifesteal) ===
    public static float RojoDamageMult()
    {
        if (HasFullSet(SetType.Rojo)) return 1.10f;
        if (Has2Pieces(SetType.Rojo)) return 1.05f;
        return 1f;
    }
    public static int RojoCritBonus() => Has2Pieces(SetType.Rojo) ? 5 : 0;
    public static int RojoLifestealBonus() => HasFullSet(SetType.Rojo) ? 5 : 0;

    // === AMARILLO: ejecutor critico (critico + umbrales de ejecucion) ===
    public static int AmarilloCritBonus()
    {
        if (HasFullSet(SetType.Amarillo)) return 10;
        if (Has2Pieces(SetType.Amarillo)) return 5;
        return 0;
    }
    public static float AmarilloExecuteThreshold()
    {
        if (HasFullSet(SetType.Amarillo)) return 10f;
        if (Has2Pieces(SetType.Amarillo)) return 35f;
        return 20f;
    }
    public static int AmarilloExecuteChance()
    {
        if (HasFullSet(SetType.Amarillo)) return 20;
        if (Has2Pieces(SetType.Amarillo)) return 15;
        return 25;
    }

    // === VERDE: tanque + supervivencia ===
    public static int VerdeMaxHPBonus()
    {
        if (HasFullSet(SetType.Verde)) return 100;
        if (Has2Pieces(SetType.Verde)) return 50;
        return 0;
    }
    public static int VerdeDefenseBonus() => Has2Pieces(SetType.Verde) ? 2 : 0;
    public static int VerdeHealOnHitBonus() => HasFullSet(SetType.Verde) ? 5 : 0;
    public static int VerdeEvasionBonus() => HasFullSet(SetType.Verde) ? 3 : 0;
    public static float VerdePotionBonus() => HasFullSet(SetType.Verde) ? 0.25f : 0f;
}