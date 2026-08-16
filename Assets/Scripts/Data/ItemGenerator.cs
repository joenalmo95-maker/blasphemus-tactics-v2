using UnityEngine;
using System.Collections.Generic;

public static class ItemGenerator
{
    private static string[] quality = { "del Penitente", "del Inquisidor", "del Templario", "del Halo Roto" };

    public static ItemData Generate(ClassData classData)
    {
        return GenerateWithRarity(classData, RollRarityBasic());
    }

    public static ItemData GenerateWithRarity(ClassData classData, Rarity rarity)
    {
        ItemSlot slot = (ItemSlot)Random.Range(0, 5);

        ItemData item = new ItemData();
        item.slot = slot;
        item.rarity = rarity;
        item.requiredClass = classData != null ? classData.className : "";
        item.stats = StatBlock.Zero();

        string baseName = SlotName(slot, classData);
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
        if (cd == null) return "Arma";
        switch (cd.role)
        {
            case ClassRole.Tank: return "Espada y escudo";
            case ClassRole.Healer: return "Cetro";
            case ClassRole.DPS: return "Látigo";
            default: return "Arma";
        }
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
        List<System.Action> pool = new List<System.Action>();
        pool.Add(() => item.stats.maxHP += Random.Range(2, 6));
        pool.Add(() => item.stats.defense += Random.Range(1, 4));
        pool.Add(() => item.stats.damage += Random.Range(1, 4));
        pool.Add(() => item.stats.attack += Random.Range(2, 6));
        pool.Add(() => item.stats.critChance += Random.Range(1, 6));
        pool.Add(() => item.stats.apMove += 1);

        if (cd != null)
        {
            if (cd.role == ClassRole.Healer)
            {
                pool.Add(() => item.stats.healingPower += Random.Range(5, 16));
            }
            else
            {
                pool.Add(() => item.stats.lifesteal += Random.Range(2, 8));
            }

            if (cd.role == ClassRole.DPS) pool.Add(() => item.stats.critChance += Random.Range(2, 8));
            if (cd.role == ClassRole.Tank) pool.Add(() => item.stats.maxHP += Random.Range(3, 8));
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
            default: return 120;
        }
    }
}