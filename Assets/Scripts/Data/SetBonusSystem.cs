using UnityEngine;

// 0.7-E: sistema de sets de armadura (3 sets, 4 piezas cada uno)
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
            case SetType.Rojo: return "(4/4) +15% daño total";
            case SetType.Amarillo: return "(4/4) +25% crítico";
            case SetType.Verde: return "(4/4) +50 HP máx y +2 HP por golpe";
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

    public static bool HasFullSet(SetType set)
    {
        return CountPieces(set) >= 4;
    }
}