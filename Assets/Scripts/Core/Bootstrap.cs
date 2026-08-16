using UnityEngine;
using System.Collections.Generic;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }
    public List<ClassData> availableClasses = new List<ClassData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LootSystem.ClearCombatLog();

        if (FindAnyObjectByType<CharacterData>() == null)
            new GameObject("CharacterData").AddComponent<CharacterData>();
        if (FindAnyObjectByType<InventorySystem>() == null)
            new GameObject("InventorySystem").AddComponent<InventorySystem>();

        SpawnEnemyByTier(GameFlow.pendingTier);

        new GameObject("InventoryUI").AddComponent<InventoryUI>();
        new GameObject("ShopUI").AddComponent<ShopUI>();
        new GameObject("GameOverUI").AddComponent<GameOverUI>();
        new GameObject("HUDUI").AddComponent<HUDUI>();

        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            SpawnPlayer();
            if (TurnManager.Instance != null) TurnManager.Instance.BeginGame();
        }
        else
        {
            GameObject c = new GameObject("CharacterCreation");
            CharacterCreationUI ui = c.AddComponent<CharacterCreationUI>();
            ui.availableClasses = availableClasses;
            ui.showContinue = SaveSystem.HasSave();
            ui.onFinished = () => { SpawnPlayer(); if (TurnManager.Instance != null) TurnManager.Instance.BeginGame(); };
            ui.Build();
        }
    }

    Unit SpawnEnemyByTier(EnemyTier tier)
    {
        float hpMult = 1f;
        int dmgBonus = 0;
        switch (tier)
        {
            case EnemyTier.Medio: hpMult = 1.4f; dmgBonus = 1; break;
            case EnemyTier.Elite: hpMult = 1.8f; dmgBonus = 2; break;
            case EnemyTier.EliteFuerte: hpMult = 2.2f; dmgBonus = 3; break;
            case EnemyTier.Jefe: hpMult = 3f; dmgBonus = 4; break;
        }

        int hp = Mathf.RoundToInt(10 * hpMult);
        Unit enemy = Unit.Create("Cruzado", new Vector2Int(7, 4), true, Color.white, 0.8f, hp, 3, "penitent");
        EnemyAI ai = enemy.gameObject.AddComponent<EnemyAI>();
        ai.tier = tier;
        ai.attackDamage = 2 + dmgBonus;
        return enemy;
    }

    public void SpawnPlayer()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) return;
        }

        StatBlock stats = CharacterData.Instance != null
            ? CharacterData.Instance.GetTotalStats()
            : new StatBlock();

        string art = "circle";
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            switch (CharacterData.Instance.classData.role)
            {
                case ClassRole.Tank: art = "tank"; break;
                case ClassRole.Healer: art = "healer"; break;
                default: art = "dps"; break;
            }
        }

        Unit player = Unit.Create("Renacido", new Vector2Int(1, 1), false,
            Color.white, 0.8f, stats.maxHP, stats.apMove, art);
        player.stats = stats.Clone();
    }
}