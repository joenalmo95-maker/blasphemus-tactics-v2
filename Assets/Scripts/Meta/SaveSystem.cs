using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public string className;
    public int level;
    public int xp;
    public int gold;
    public List<ItemData> items = new List<ItemData>();
    public List<ItemData> equipped = new List<ItemData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();
}

public static class SaveSystem
{
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static void Save()
    {
        if (CharacterData.Instance == null || InventorySystem.Instance == null) return;

        SaveData data = new SaveData();
        data.className = CharacterData.Instance.classData != null ? CharacterData.Instance.classData.className : "";
        data.level = CharacterData.Instance.level;
        data.xp = CharacterData.Instance.xp;
        data.gold = CharacterData.Instance.gold;
        data.items = InventorySystem.Instance.items;
        data.equipped = InventorySystem.Instance.GetAllEquipped();
        data.consumables = InventorySystem.Instance.consumables;

        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        Debug.Log("Partida guardada.");
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
    }
}