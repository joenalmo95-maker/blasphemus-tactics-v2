using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static bool HasLoadedThisSession = false;  // ← AGREGAR ESTA LÍNEA
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save()
    {
        SaveData data = new SaveData();
        data.version = 6;
        data.playerName = CharacterData.Instance != null ? CharacterData.Instance.playerName : "Valerius";
        data.level = CharacterData.Instance != null ? CharacterData.Instance.level : 1;
        data.xp = CharacterData.Instance != null ? CharacterData.Instance.xp : 0;
        data.gold = CharacterData.Instance != null ? CharacterData.Instance.gold : 0;

        LoadoutSystem.SnapshotToSave(data);

        if (InventorySystem.Instance != null)
        {
            data.inventory = InventorySystem.Instance.Serialize();
        }

        if (WarehouseSystem.Instance != null)
        {
            data.warehouse = WarehouseSystem.Instance.Serialize();
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
        Debug.Log("[SaveSystem] Guardado: " + Path);
    }

    public static void Load()
    {
        if (HasLoadedThisSession) return;  // ← AGREGAR ESTA LÍNEA AL INICIO
        
        if (!File.Exists(Path))
        {
            Debug.Log("[SaveSystem] No hay save. Creando nuevo.");
            return;
        }

        string json = File.ReadAllText(Path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data.version < 6)
        {
            Debug.Log("[SaveSystem] Migrando save v" + data.version + " → v6 (Valerius único)");
            data.playerName = "Valerius";
            data.version = 6;
        }

        if (CharacterData.Instance == null)
        {
            new GameObject("CharacterData").AddComponent<CharacterData>();
        }

        CharacterData.Instance.playerName = data.playerName;
        CharacterData.Instance.level = data.level;
        CharacterData.Instance.xp = data.xp;
        CharacterData.Instance.gold = data.gold;

        LoadoutSystem.ApplyFromSave(data);

        if (InventorySystem.Instance != null && data.inventory != null)
        {
            InventorySystem.Instance.Deserialize(data.inventory);
        }

        if (WarehouseSystem.Instance != null && data.warehouse != null)
        {
            WarehouseSystem.Instance.Deserialize(data.warehouse);
        }

        Debug.Log("[SaveSystem] Cargado: " + data.playerName + " nivel " + data.level);
        HasLoadedThisSession = true;  // ← AGREGAR ESTA LÍNEA AL FINAL

    }

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static void DeleteSave()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
            Debug.Log("[SaveSystem] Save eliminado.");
        }
    }
}

[System.Serializable]
public class SaveData
{
    public int version = 6;
    public string playerName = "Valerius";
    public string className = "Inquisidor";
    public int level = 1;
    public int xp = 0;
    public int gold = 0;
    
    // Campos para InventorySystem
    public System.Collections.Generic.List<ItemData> items = new System.Collections.Generic.List<ItemData>();
    public System.Collections.Generic.List<ItemData> equipped = new System.Collections.Generic.List<ItemData>();
    public System.Collections.Generic.List<ConsumableData> consumables = new System.Collections.Generic.List<ConsumableData>();
    
        // Campos para LoadoutSystem
    public System.Collections.Generic.List<string> learnedSkills;
    public System.Collections.Generic.List<string> activeSkills;
    public string ultimateSkill = "";
    public System.Collections.Generic.List<string> passiveSkills;
    
    // Campos para SkillTrainer
    public TrainingSnapshot training;
    
    // Campos para QuestSystem
    public System.Collections.Generic.List<QuestSaveEntry> activeQuests;
    public long lastDailyReset;
    public long lastWeeklyReset;
    public int seasonPhase;
    
    // Campos legacy (para compatibilidad)
    public InventorySaveData inventory;
    public WarehouseSaveData warehouse;
}