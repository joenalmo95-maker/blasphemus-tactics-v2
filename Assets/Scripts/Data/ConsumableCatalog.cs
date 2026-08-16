using System.Collections.Generic;

public enum ConsumableType { PocionHP, PocionAP, ComidaDano, ComidaDefensa }

[System.Serializable]
public class ConsumableData
{
    public ConsumableType type;
    public int count;
}

public static class ConsumableCatalog
{
    public static string Name(ConsumableType t)
    {
        switch (t)
        {
            case ConsumableType.PocionHP: return "Poción de Vida";
            case ConsumableType.PocionAP: return "Poción de Energía";
            case ConsumableType.ComidaDano: return "Comida Picante";
            default: return "Comida de Hierro";
        }
    }

    public static int Price(ConsumableType t)
    {
        switch (t)
        {
            case ConsumableType.PocionHP: return 10;
            case ConsumableType.PocionAP: return 12;
            default: return 15;
        }
    }

    public static string Description(ConsumableType t)
    {
        switch (t)
        {
            case ConsumableType.PocionHP: return "Cura 8 HP (tecla 4)";
            case ConsumableType.PocionAP: return "Restaura 2 AP (tecla 5)";
            case ConsumableType.ComidaDano: return "+2 Daño 3 turnos (tecla 6)";
            default: return "+2 Defensa 3 turnos (tecla 7)";
        }
    }
}