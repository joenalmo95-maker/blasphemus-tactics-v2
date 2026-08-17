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
}