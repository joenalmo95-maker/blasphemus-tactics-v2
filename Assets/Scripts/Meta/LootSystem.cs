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

        // 5.3: al caer un jefe, el objetivo final queda completado
        if (tier == EnemyTier.Jefe) ObjectiveSystem.MarkBossDefeated();

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

    // 5.2: rangos públicos para la tarjeta de mazmorra (fuente única)
    public static void GoldRange(EnemyTier tier, out int min, out int max)
    {
        switch (tier)
        {
            case EnemyTier.Basico: min = 3; max = 9; break;
            case EnemyTier.Medio: min = 8; max = 17; break;
            case EnemyTier.Elite: min = 15; max = 31; break;
            case EnemyTier.EliteFuerte: min = 25; max = 51; break;
            default: min = 50; max = 101; break;
        }
    }

    static int RollGold(EnemyTier tier)
    {
        GoldRange(tier, out int min, out int max);
        return Random.Range(min, max);
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
        public static int XpForTier(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico: return 6;
            case EnemyTier.Medio: return 12;
            case EnemyTier.Elite: return 20;
            case EnemyTier.EliteFuerte: return 30;
            default: return 60;
        }
    }
}