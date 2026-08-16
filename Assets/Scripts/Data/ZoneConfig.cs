using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnConfig { public string archetype; public string tier; public int x; public int y; }

[System.Serializable]
public class WaveConfig { public SpawnConfig[] spawns; }

[System.Serializable]
public class ZoneConfig { public string name; public string tier; public int x; public int y; public WaveConfig[] waves; }

[System.Serializable]
public class ZonesConfigFile { public ZoneConfig[] zones; }

public static class ZoneConfigLoader
{
    public static List<WorldBootstrap.ZoneDef> Load()
    {
        List<WorldBootstrap.ZoneDef> list = new List<WorldBootstrap.ZoneDef>();

        TextAsset asset = Resources.Load<TextAsset>("ZonesConfig");
        if (asset != null)
        {
            ZonesConfigFile file = JsonUtility.FromJson<ZonesConfigFile>(asset.text);
            if (file != null && file.zones != null)
            {
                foreach (ZoneConfig zc in file.zones)
                {
                    WorldBootstrap.ZoneDef z = new WorldBootstrap.ZoneDef();
                    z.name = zc.name;
                    z.center = new Vector2Int(zc.x, zc.y);
                    z.tier = ParseTier(zc.tier, EnemyTier.Basico);
                    z.dungeon = new List<WaveDef>();

                    if (zc.waves != null)
                    {
                        foreach (WaveConfig wc in zc.waves)
                        {
                            WaveDef wd = new WaveDef();
                            wd.spawns = new List<SpawnDef>();
                            if (wc.spawns != null)
                            {
                                foreach (SpawnConfig sc in wc.spawns)
                                {
                                    wd.spawns.Add(new SpawnDef
                                    {
                                        archetype = sc.archetype,
                                        tier = ParseTier(sc.tier, z.tier),
                                        cell = new Vector2Int(sc.x, sc.y)
                                    });
                                }
                            }
                            z.dungeon.Add(wd);
                        }
                    }
                    list.Add(z);
                }
            }
        }

        if (list.Count == 0) list = DefaultZones();
        return list;
    }

    static EnemyTier ParseTier(string s, EnemyTier def)
    {
        EnemyTier t;
        if (!string.IsNullOrEmpty(s) && System.Enum.TryParse(s, true, out t)) return t;
        return def;
    }

    static List<WorldBootstrap.ZoneDef> DefaultZones()
    {
        List<WorldBootstrap.ZoneDef> d = new List<WorldBootstrap.ZoneDef>();
        d.Add(new WorldBootstrap.ZoneDef
        {
            name = "Cripta de los Penitentes",
            center = new Vector2Int(10, 8),
            tier = EnemyTier.Basico,
            dungeon = new List<WaveDef>
            {
                new WaveDef { spawns = new List<SpawnDef> { S("penitent", EnemyTier.Basico, 7, 4) } }
            }
        });
        return d;
    }

    static SpawnDef S(string archetype, EnemyTier tier, int x, int y)
    {
        return new SpawnDef { archetype = archetype, tier = tier, cell = new Vector2Int(x, y) };
    }
}