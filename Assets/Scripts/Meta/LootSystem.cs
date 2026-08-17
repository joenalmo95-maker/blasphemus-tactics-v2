using UnityEngine;
using System.Collections.Generic;

public enum EnemyTier { Basico, Medio, Elite, EliteFuerte, Jefe }

public static class LootSystem
{
    private static List<string> combatLog = new List<string>();

    public static void ClearCombatLog()
    {
        combatLog.Clear();
    }

    public static string GetCombatSummary()
    {
        if (combatLog.Count == 0) return "Sin botín.";
        return string.Join("\n", combatLog);
    }

    public static void DropFrom(Unit enemy, EnemyTier tier)
    {
        int gold = RollGold(tier);
        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.gold += gold;
            Debug.Log("Oro obtenido: " + gold + " (total: " + CharacterData.Instance.gold + ")");
        }
        combatLog.Add("+" + gold + " de oro");
        if (enemy != null) CombatFeedback.SpawnText(enemy.transform.position, "+" + gold + " oro", Color.yellow);

        int xp = XpForTier(tier);
        // 4.1: el mundo otorga la mitad de EXP que las mazmorras
        if (GameFlow.pendingIsWorld) xp = Mathf.Max(1, Mathf.RoundToInt(xp * 0.5f));
        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.GainXP(xp);
        }
        combatLog.Add("+" + xp + " EXP");
        if (enemy != null) CombatFeedback.SpawnText(enemy.transform.position, "+" + xp + " EXP", Color.cyan);

        int drops = tier == EnemyTier.Jefe ? 2 : (Random.Range(0f, 1f) < ItemChance(tier) ? 1 : 0);
        for (int i = 0; i < drops; i++)
        {
            ClassData cd = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
            ItemData item = ItemGenerator.GenerateWithRarity(cd, RollRarity(tier));
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(item);
            }
            combatLog.Add("+ " + item.itemName + " [" + item.rarity + "]");
        }
    }

    static int RollGold(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico: return Random.Range(3, 9);
            case EnemyTier.Medio: return Random.Range(8, 17);
            case EnemyTier.Elite: return Random.Range(15, 31);
            case EnemyTier.EliteFuerte: return Random.Range(25, 51);
            default: return Random.Range(50, 101);
        }
    }

    static float ItemChance(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico: return 0.35f;
            case EnemyTier.Medio: return 0.5f;
            case EnemyTier.Elite: return 0.75f;
            default: return 1f;
        }
    }

    static Rarity RollRarity(EnemyTier tier)
    {
        int roll = Random.Range(0, 100);
        switch (tier)
        {
            case EnemyTier.Basico:
                if (roll < 70) return Rarity.Common;
                if (roll < 95) return Rarity.Rare;
                return Rarity.Epic;
            case EnemyTier.Medio:
                if (roll < 55) return Rarity.Common;
                if (roll < 85) return Rarity.Rare;
                if (roll < 97) return Rarity.Epic;
                return Rarity.Legendary;
            case EnemyTier.Elite:
                if (roll < 40) return Rarity.Common;
                if (roll < 75) return Rarity.Rare;
                if (roll < 95) return Rarity.Epic;
                return Rarity.Legendary;
            case EnemyTier.EliteFuerte:
                if (roll < 25) return Rarity.Common;
                if (roll < 65) return Rarity.Rare;
                if (roll < 90) return Rarity.Epic;
                return Rarity.Legendary;
            default:
                if (roll < 10) return Rarity.Rare;
                if (roll < 50) return Rarity.Epic;
                return Rarity.Legendary;
        }
    }
        static int XpForTier(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico: return 10;
            case EnemyTier.Medio: return 20;
            case EnemyTier.Elite: return 35;
            case EnemyTier.EliteFuerte: return 50;
            default: return 100;
        }
    }
}