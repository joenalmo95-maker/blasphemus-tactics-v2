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

    // 0.7-E.4: drop de pieza de set al ganar DNG + pity gacha (80 runs)
    public static void OnDungeonVictory(WorldBootstrap.ZoneDef zone)
    {
        if (zone == null) return;
        if (zone.setPiece == SetPieceType.Ninguna) return;

        string pityKey = "SetPity_" + zone.name;
        int pity = PlayerPrefs.GetInt(pityKey, 0);

        // Probabilidades gacha extremo: 3% fija, 1% aleatoria
        bool isFixed = zone.setId != SetType.Ninguno;
        float baseProb = isFixed ? 0.03f : 0.01f;
        int pityCap = 80;

        // Forzar drop si se alcanzó el pity gacha
        bool force = pity >= pityCap;
        float totalProb = force ? 1f : baseProb;

        bool dropMain = Random.Range(0f, 1f) < totalProb;
        bool dropBonus = Random.Range(0f, 1f) < 0.03f; // 3% bonus aleatorio

        if (dropMain)
        {
            SetType setId = zone.setId;
            SetPieceType pieceId = zone.setPiece;

            // Si es zona aleatoria (setId == Ninguno), elegir set al azar
            if (setId == SetType.Ninguno)
            {
                SetType[] sets = { SetType.Rojo, SetType.Amarillo, SetType.Verde };
                setId = sets[Random.Range(0, sets.Length)];
            }

            ItemData piece = ItemGenerator.GenerateSetPiece(setId, pieceId, zone.tier);
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(piece);
                combatLog.Add("★ PIEZA DE SET: " + piece.itemName + " [" + piece.rarity + "]");
                Debug.Log("[LootSystem] ★ PIEZA DE SET: " + piece.itemName);
            }
            // Resetear pity al conseguir pieza
            PlayerPrefs.SetInt(pityKey, 0);
        }
        else
        {
            // Incrementar pity si no sale
            PlayerPrefs.SetInt(pityKey, pity + 1);
            Debug.Log("[LootSystem] Sin pieza de set. Pity: " + (pity + 1) + "/" + pityCap);
        }

        // Drop bonus aleatorio (3% prob de otra pieza de cualquier set)
        if (dropBonus)
        {
            SetType[] sets = { SetType.Rojo, SetType.Amarillo, SetType.Verde };
            SetPieceType[] pieces = { SetPieceType.Casco, SetPieceType.Peto, SetPieceType.Pantalon, SetPieceType.Guantes };
            SetType randSet = sets[Random.Range(0, sets.Length)];
            SetPieceType randPiece = pieces[Random.Range(0, pieces.Length)];
            ItemData bonus = ItemGenerator.GenerateSetPiece(randSet, randPiece, zone.tier);
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(bonus);
                combatLog.Add("✦ BONUS ALEATORIO: " + bonus.itemName);
                Debug.Log("[LootSystem] ✦ BONUS: " + bonus.itemName);
            }
        }

        PlayerPrefs.Save();
    }
}