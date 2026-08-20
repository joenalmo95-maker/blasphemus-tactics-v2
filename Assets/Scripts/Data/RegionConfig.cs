using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RegionData
{
    public string id;              // "region_1", "region_2", etc.
    public string name;            // "Valle de la Luz Eterna"
    public int tier;               // 1-9 (dificultad)
    public string mapFile;         // "WorldMapData_Region1" (sin extensión)
    public string tilesetKey;      // "tileset_valle" (para sprites futuros)
    public string musicTrack;      // "music_valle" (para audio futuro)
    public bool isActive;          // true = jugable, false = bloqueada
    public string unlockCondition; // "default", "dlc_1", "dlc_2", etc.
    public string description;     // Texto para el teletransporte
}

[System.Serializable]
public class RegionsConfigFile
{
    public RegionData[] regions;
}

public static class RegionConfigLoader
{
    public static List<RegionData> Load()
    {
        List<RegionData> list = new List<RegionData>();

        TextAsset asset = Resources.Load<TextAsset>("RegionsConfig");
        if (asset != null)
        {
            try
            {
                RegionsConfigFile file = JsonUtility.FromJson<RegionsConfigFile>(asset.text);
                if (file != null && file.regions != null)
                {
                    foreach (RegionData r in file.regions)
                    {
                        list.Add(r);
                    }
                    Debug.Log("[RegionConfig] Regiones cargadas: " + list.Count);
                    return list;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[RegionConfig] Error cargando RegionsConfig.json: " + e.Message);
            }
        }

        // Fallback: solo Región I activa
        Debug.Log("[RegionConfig] Usando regiones por defecto (fallback).");
        return DefaultRegions();
    }

    static List<RegionData> DefaultRegions()
    {
        List<RegionData> d = new List<RegionData>();
        d.Add(new RegionData
        {
            id = "region_1",
            name = "Valle de la Luz Eterna",
            tier = 1,
            mapFile = "WorldMapData_Region1",
            tilesetKey = "tileset_valle",
            musicTrack = "music_valle",
            isActive = true,
            unlockCondition = "default",
            description = "Región inicial bañada por luz eterna. Aquí comienza tu cruzada."
        });
        return d;
    }

    public static RegionData GetActiveRegion()
    {
        foreach (RegionData r in WorldBootstrap.Regions)
        {
            if (r.isActive) return r;
        }
        return null;
    }

    public static bool IsRegionUnlocked(string regionId)
    {
        foreach (RegionData r in WorldBootstrap.Regions)
        {
            if (r.id == regionId) return r.isActive;
        }
        return false;
    }
}