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

        if (GameFlow.pendingDungeon != null && GameFlow.pendingDungeon.Count > 0)
        {
            DungeonManager dm = new GameObject("DungeonManager").AddComponent<DungeonManager>();
            dm.waves = GameFlow.pendingDungeon;
            dm.StartDungeon();
        }
        else
        {
            EnemyFactory.Spawn("penitent", GameFlow.pendingTier, new Vector2Int(7, 4));
        }

        new GameObject("InventoryUI").AddComponent<InventoryUI>();
        new GameObject("ShopUI").AddComponent<ShopUI>();
        new GameObject("GameOverUI").AddComponent<GameOverUI>();
        new GameObject("HUDUI").AddComponent<HUDUI>();

        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            SpawnPlayer();
            // TurnManager.Start() iniciará el turno automáticamente tras todos los Awake.
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