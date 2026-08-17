using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SaveData
{
    // 5.1: versionado (los saves antiguos quedan en 0 y migran al guardar)
    public int version = 3;

    public string className;
    public int level;
    public int xp;
    public int gold;
    public List<ItemData> items = new List<ItemData>();
    public List<ItemData> equipped = new List<ItemData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();

    // Bloques extendidos 5.1
    public List<ItemData> warehouse = new List<ItemData>();
    public TrainingSnapshot training;
    public bool passiveEnabled = true;
    public List<TimerEntry> spawnTimers = new List<TimerEntry>();
    public List<TimerEntry> chestTimers = new List<TimerEntry>();
    public bool hasLastWorld;
    public int lastWorldX;
    public int lastWorldY;

    // 1.1-B: skills aprendidas y loadout (v3)
    public List<string> learnedSkills = new List<string>();
    public List<string> activeSkills = new List<string>();
    public string ultimateSkill = "";
    public List<string> passiveSkills = new List<string>();
}

[System.Serializable]
public class TrainingSnapshot
{
    public bool learned2;
    public bool learned3;
    public bool learned4;
    public int train1;
    public int train2;
    public int train3;
    public int train4;
}

[System.Serializable]
public class TimerEntry
{
    public int id;
    public long ticks;
}

public static class SaveSystem
{
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");
    private static bool extendedApplied;

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static void Save()
    {
        if (CharacterData.Instance == null || InventorySystem.Instance == null) return;

        // Carry-over: si algún sistema no existe en esta escena, se conserva lo último guardado
        SaveData prev = HasSave() ? Load() : null;

        SaveData data = new SaveData();
        data.className = CharacterData.Instance.classData != null ? CharacterData.Instance.classData.className : "";
        data.level = CharacterData.Instance.level;
        data.xp = CharacterData.Instance.xp;
        data.gold = CharacterData.Instance.gold;
        data.items = InventorySystem.Instance.items;
        data.equipped = InventorySystem.Instance.GetAllEquipped();
        data.consumables = InventorySystem.Instance.consumables;

        // 5.1: almacén unificado
        data.warehouse = WarehouseSystem.Instance != null
            ? WarehouseSystem.Instance.stored
            : (prev != null && prev.warehouse != null ? prev.warehouse : new List<ItemData>());

        // 5.1: entrenos, pasiva y timers
        data.training = SkillTrainer.GetSnapshot();
        data.passiveEnabled = PassiveSystem.Enabled;
        data.spawnTimers = SnapshotTimers(WorldSpawnManager.DefeatedAt);
        data.chestTimers = SnapshotTimers(WorldChestManager.OpenedAt);

        // 5.1: posición de mundo pendiente de restaurar
        if (PlayerPrefs.HasKey("LastWorldX"))
        {
            data.hasLastWorld = true;
            data.lastWorldX = PlayerPrefs.GetInt("LastWorldX", 2);
            data.lastWorldY = PlayerPrefs.GetInt("LastWorldY", 2);
        }

        // 1.1-B: loadout en el guardado unificado
        LoadoutSystem.SnapshotToSave(data);

        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        Debug.Log("Partida guardada (v" + data.version + ").");
    }

    static List<TimerEntry> SnapshotTimers(Dictionary<int, float> live)
    {
        List<TimerEntry> list = new List<TimerEntry>();
        float now = Time.realtimeSinceStartup;
        foreach (var kv in live)
        {
            double age = now - kv.Value;
            list.Add(new TimerEntry
            {
                id = kv.Key,
                ticks = DateTime.UtcNow.Ticks - (long)(age * TimeSpan.TicksPerSecond)
            });
        }
        return list;
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
    }

    // 5.1: restaura bloques extendidos (una vez por sesión; legacy queda en sus stores)
    public static void ApplyExtendedOnce()
    {
        if (extendedApplied) return;
        extendedApplied = true;

        SaveData data = Load();
        if (data == null || data.version < 2) return;

        // 1.1-B: restaura loadout (los saves v2 migran vía EnsureInitialized)
        LoadoutSystem.ApplyFromSave(data);

        if (WarehouseSystem.Instance == null)
            new GameObject("WarehouseSystem").AddComponent<WarehouseSystem>();
        if (data.warehouse != null) WarehouseSystem.Instance.stored = data.warehouse;

        SkillTrainer.ApplySnapshot(data.training);
        PassiveSystem.SetEnabled(data.passiveEnabled);

        RestoreTimers(data.spawnTimers, WorldSpawnManager.DefeatedAt, WorldSpawnManager.RespawnSeconds);
        RestoreTimers(data.chestTimers, WorldChestManager.OpenedAt, WorldChestManager.RespawnSeconds);

        if (data.hasLastWorld)
        {
            PlayerPrefs.SetInt("LastWorldX", data.lastWorldX);
            PlayerPrefs.SetInt("LastWorldY", data.lastWorldY);
        }
    }

    static void RestoreTimers(List<TimerEntry> entries, Dictionary<int, float> live, float respawnSeconds)
    {
        if (entries == null) return;
        float now = Time.realtimeSinceStartup;
        foreach (TimerEntry e in entries)
        {
            double age = (DateTime.UtcNow.Ticks - e.ticks) / (double)TimeSpan.TicksPerSecond;
            if (age < respawnSeconds) live[e.id] = now - (float)age;
        }
    }
}