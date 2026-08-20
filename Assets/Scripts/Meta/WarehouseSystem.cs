using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class WarehouseSystem : MonoBehaviour
{
    public static WarehouseSystem Instance { get; private set; }

    public List<ItemData> stored = new List<ItemData>();

    [System.Serializable]
    class WarehouseData
    {
        public List<ItemData> items = new List<ItemData>();
    }

    string SavePath { get { return Path.Combine(Application.persistentDataPath, "warehouse.json"); } }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Save()
    {
        WarehouseData d = new WarehouseData();
        d.items = stored;
        File.WriteAllText(SavePath, JsonUtility.ToJson(d, true));
    }

    void Load()
    {
        if (!File.Exists(SavePath)) return;
        WarehouseData d = JsonUtility.FromJson<WarehouseData>(File.ReadAllText(SavePath));
        if (d != null && d.items != null) stored = d.items;
    }

    // 0.3: Método Serialize para SaveSystem
    public WarehouseSaveData Serialize()
    {
        WarehouseSaveData data = new WarehouseSaveData();
        data.stored = stored;
        return data;
    }

    // 0.3: Método Deserialize para SaveSystem
    public void Deserialize(WarehouseSaveData data)
    {
        if (data == null) return;
        stored = data.stored != null ? data.stored : new List<ItemData>();
    }
}