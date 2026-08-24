using UnityEngine;
using System.Collections.Generic;

public static class ItemGenerator
{
    private static string[] quality = { "del Penitente", "del Inquisidor", "del Templario", "del Halo Roto" };

    // 0.7-C: Jerarquía de espadones por tier (nombre + daño base + rareza)
    public struct EspadonTier
    {
        public string nombre;
        public int dañoMin;
        public int dañoMax;
        public Rarity rareza;
    }

    // 1.3: Curva de daño de espadones más progresiva (early game más suave, endgame más fuerte)
    public static readonly EspadonTier[] Espadones = new EspadonTier[]
    {
        new EspadonTier { nombre = "Espadón del Penitente",  dañoMin = 2,  dañoMax = 4,  rareza = Rarity.Common },
        new EspadonTier { nombre = "Espadón del Inquisidor", dañoMin = 5,  dañoMax = 8,  rareza = Rarity.Rare },
        new EspadonTier { nombre = "Espadón del Halo Roto",  dañoMin = 10, dañoMax = 15, rareza = Rarity.Epic },
        new EspadonTier { nombre = "Espadón de la Vigilia",  dañoMin = 18, dañoMax = 25, rareza = Rarity.Legendary },
        new EspadonTier { nombre = "Espadón del Milagro",    dañoMin = 30, dañoMax = 45, rareza = Rarity.Reliquia }
    };

    public static ItemData Generate(ClassData classData)
    {
        return GenerateWithRarity(classData, RollRarityBasic());
    }

    public static ItemData GenerateWithRarity(ClassData classData, Rarity rarity)
    {
        // 1.1-fix: UNIFICACIÓN - GenerateWithRarity SOLO genera armaduras (Chest/Legs/Helm/Gloves)
        // Los espadones se generan explícitamente con GenerateEspadon()
        ItemSlot[] armorSlots = { ItemSlot.Chest, ItemSlot.Legs, ItemSlot.Helm, ItemSlot.Gloves };
        ItemSlot slot = armorSlots[Random.Range(0, armorSlots.Length)];

        ItemData item = new ItemData();
        item.slot = slot;
        item.rarity = rarity;
        item.requiredClass = "";
        item.armorType = RollArmorType(classData);
        item.stats = StatBlock.Zero();

        string baseName = ArmorName(slot, item.armorType, classData);
        item.itemName = baseName + " " + quality[Random.Range(0, quality.Length)];

        int affixes = 1 + (int)rarity;
        List<System.Action> pool = BuildPool(classData, item);
        for (int i = 0; i < affixes && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            pool[idx]();
            pool.RemoveAt(idx);
        }

        return item;
    }

    static string SlotName(ItemSlot slot, ClassData cd)
    {
        switch (slot)
        {
            case ItemSlot.Weapon: return WeaponName(cd);
            case ItemSlot.Chest: return "Peto";
            case ItemSlot.Legs: return "Pantalón";
            case ItemSlot.Helm: return "Casco";
            case ItemSlot.Gloves: return "Guantes";
            default: return "Objeto";
        }
    }

    static string WeaponName(ClassData cd)
    {
        // 1.1: Valerius usa solo espadón (eliminado Cetro/Espada y escudo/Látigo)
        return "Espadón";
    }

    static Rarity RollRarityBasic()
    {
        int roll = Random.Range(0, 100);
        if (roll < 55) return Rarity.Common;
        if (roll < 80) return Rarity.Rare;
        if (roll < 95) return Rarity.Epic;
        return Rarity.Legendary;
    }

    static List<System.Action> BuildPool(ClassData cd, ItemData item)
    {
        // 1.2: rangos reducidos para que el loot genérico sea inferior al equipo de set
        List<System.Action> pool = new List<System.Action>();
        pool.Add(() => item.stats.maxHP += Random.Range(1, 4));        // antes: 2-6
        pool.Add(() => item.stats.defense += Random.Range(1, 2));      // antes: 1-4
        pool.Add(() => item.stats.damage += Random.Range(1, 2));       // antes: 1-4
        pool.Add(() => item.stats.accuracy += Random.Range(1, 3));     // antes: 2-6
        pool.Add(() => item.stats.critChance += Random.Range(1, 3));   // antes: 1-6
        // AP eliminado del pool genérico (solo espadones reliquia dan AP)

        if (cd != null)
        {
            if (cd.role == ClassRole.Healer)
            {
                pool.Add(() => item.stats.healingPower += Random.Range(2, 6)); // antes: 5-16
            }
            else
            {
                pool.Add(() => item.stats.lifesteal += Random.Range(1, 2));    // antes: 2-8
            }

            if (cd.role == ClassRole.DPS) pool.Add(() => item.stats.critChance += Random.Range(1, 2)); // antes: 2-8
            if (cd.role == ClassRole.Tank) pool.Add(() => item.stats.maxHP += Random.Range(2, 4));     // antes: 3-8
        }

        return pool;
    }

    public static Color RarityColor(Rarity r)
    {
        switch (r)
        {
            case Rarity.Rare: return Color.cyan;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return Color.yellow;
            case Rarity.Reliquia: return new Color(1f, 0.4f, 0.1f); // naranja ardiente
            default: return Color.white;
        }
    }
    public static int SellPrice(ItemData item)
    {
        switch (item.rarity)
        {
            case Rarity.Rare: return 10;
            case Rarity.Epic: return 20;
            case Rarity.Legendary: return 40;
            case Rarity.Reliquia: return 100;
            default: return 5;
        }
    }

     public static int BuyPrice(Rarity r)
    {
        switch (r)
        {
            case Rarity.Common: return 15;
            case Rarity.Rare: return 30;
            case Rarity.Epic: return 60;
            case Rarity.Legendary: return 120;
            case Rarity.Reliquia: return 500;
            default: return 15;
        }
    }

    // --- 1.1: unificación de armaduras (solo Placas para Valerius) ---
    public static ArmorType ArmorFor(ClassData cd)
    {
        return ArmorType.Placas;
    }

    static ArmorType RollArmorType(ClassData cd)
    {
        // 1.1: 100% Placas, eliminado Cuero/Ropa del loot pool
        return ArmorType.Placas;
    }

    static string ArmorName(ItemSlot slot, ArmorType t, ClassData cd)
    {
        if (slot == ItemSlot.Weapon) return WeaponName(cd);
        // 1.1: nombres lore-friendly (siempre Placas, pero con sufijo épico)
        string baseN = slot == ItemSlot.Chest ? "Coraza"
                     : slot == ItemSlot.Legs ? "Grebas"
                     : slot == ItemSlot.Helm ? "Yelmo" : "Guanteletes";
        return baseN + " de Placas";
    }

    // 0.7-E.2: sin clases — toda armadura es equipable; ArmorType queda como sabor visual
    public static bool CanEquipClass(ItemData item, ClassData cd)
    {
        if (item == null) return false;
        return true;
    }

    // 0.7-C: Generador de espadón específico por tier
    public static ItemData GenerateEspadon(Rarity tierDeseado)
    {
        EspadonTier tier = Espadones[0]; // fallback Common
        foreach (EspadonTier t in Espadones)
        {
            if (t.rareza == tierDeseado) { tier = t; break; }
        }

        ItemData item = new ItemData();
        item.slot = ItemSlot.Weapon;
        item.rarity = tier.rareza;
        item.requiredClass = "";
        item.armorType = ArmorType.Ninguna;
        item.itemName = tier.nombre;
        item.stats = StatBlock.Zero();

        // Daño base escalado por tier
        int dmgBase = Random.Range(tier.dañoMin, tier.dañoMax + 1);
        item.stats.damage = dmgBase;

        // 1.3: Precisión de arma reducida (10 base + 5 por rareza). 
        // Sumado a los 80 base de Valerius, da un rango perfecto de 90-110 (cap 95% hit chance).
        item.stats.accuracy = 10 + (int)tier.rareza * 5;

        // Affixes aleatorios (1 por nivel de rareza, mínimo 1)
        int affixes = 1 + (int)tier.rareza;
        List<System.Action> pool = BuildPool(null, item);
        for (int i = 0; i < affixes && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            pool[idx]();
            pool.RemoveAt(idx);
        }

        Debug.Log("[ItemGenerator] Espadón generado: " + item.itemName + " | Daño base: " + dmgBase + " | Rareza: " + tier.rareza);
        return item;
    }

    // 0.7-E: generador de pieza de set específica
    public static ItemData GenerateSetPiece(SetType set, SetPieceType piece, EnemyTier tier)
    {
        ItemData item = new ItemData();
        item.slot = PieceToSlot(piece);
        item.setId = set;
        item.setPiece = piece;
        item.armorType = ArmorType.Ninguna;
        item.requiredClass = "";
        // 0.7-E.4b2-fix: las piezas de set son el mejor equipo del juego → siempre Reliquia
        item.rarity = Rarity.Reliquia;
        item.itemName = SetBonusSystem.PieceName(piece) + " " + SetBonusSystem.SetSuffix(set);
        item.stats = StatBlock.Zero();

        // 0.7-E.4b2: stats mejoradas (piezas de set superiores al loot genérico)
        int t = (int)tier;
        switch (piece)
        {
            case SetPieceType.Casco:
                item.stats.defense += 3 + t / 2;
                item.stats.critChance += 3;
                item.stats.accuracy += 2;
                break;
            case SetPieceType.Peto:
                item.stats.maxHP += 25 + t * 5;
                item.stats.defense += 3 + t / 2;
                break;
            case SetPieceType.Pantalon:
                item.stats.evasion += 4 + t / 2;
                item.stats.maxHP += 15 + t * 3;
                break;
            case SetPieceType.Guantes:
                item.stats.damage += 5 + t;
                item.stats.critChance += 4 + t / 2;
                break;
        }

        Debug.Log("[ItemGenerator] Pieza de set: " + item.itemName + " [" + item.rarity + "]");
        return item;
    }

    static ItemSlot PieceToSlot(SetPieceType p)
    {
        switch (p)
        {
            case SetPieceType.Casco: return ItemSlot.Helm;
            case SetPieceType.Peto: return ItemSlot.Chest;
            case SetPieceType.Pantalon: return ItemSlot.Legs;
            default: return ItemSlot.Gloves;
        }
    }

    // 0.7-E.3: Botas (slot libre, NO cuentan para el set)
    public static ItemData GenerateBoots(EnemyTier tier)
    {
        ItemData item = new ItemData();
        item.slot = ItemSlot.Boots;
        item.setId = SetType.Ninguno;
        item.setPiece = SetPieceType.Ninguna;
        item.armorType = ArmorType.Ninguna;
        item.requiredClass = "";
        // 0.7-E.4b2-fix: las piezas de set son el mejor equipo del juego → siempre Reliquia
        item.rarity = Rarity.Reliquia;
        item.itemName = "Botas " + quality[Random.Range(0, quality.Length)];
        item.stats = StatBlock.Zero();

        int t = (int)tier;
        item.stats.evasion += 2 + t / 2;
        item.stats.maxHP += 4 + t * 2;

        // 0.7-E.3: 8% de probabilidad de 2% robo de vida (loot raro)
        if (Random.Range(0, 100) < 8)
        {
            item.stats.lifesteal += 2;
            if (item.rarity < Rarity.Epic) item.rarity = Rarity.Epic;
        }

        Debug.Log("[ItemGenerator] Botas: " + item.itemName + (item.stats.lifesteal > 0 ? " [ROBO DE VIDA]" : ""));
        return item;
    }

    // 0.7-F.1b: Botas del Capitán (Reliquia exclusiva del Boss Mundial)
    public static ItemData GenerateBossBoots()
    {
        ItemData item = new ItemData();
        item.slot = ItemSlot.Boots;
        item.setId = SetType.Ninguno;
        item.setPiece = SetPieceType.Ninguna;
        item.armorType = ArmorType.Ninguna;
        item.requiredClass = "";
        item.rarity = Rarity.Reliquia;
        item.itemName = "Botas del Capitán Caído";
        item.stats = StatBlock.Zero();

        // Stats Reliquia endgame
        item.stats.defense += 8;
        item.stats.maxHP += 40;
        item.stats.evasion += 6;
        item.stats.damage += 8;
        item.stats.critChance += 8;
        item.stats.lifesteal += 2;
        item.stats.worldSpeed += 30; // +30% velocidad en el mundo

        Debug.Log("[ItemGenerator] ★ BOTAS DEL BOSS: " + item.itemName + " [Reliquia +30% worldSpeed]");
        return item;
    }
}