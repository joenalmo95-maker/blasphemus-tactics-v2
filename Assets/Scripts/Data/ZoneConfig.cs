using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnConfig { public string archetype; public string tier; public int x; public int y; }

[System.Serializable]
public class WaveConfig { public SpawnConfig[] spawns; }

[System.Serializable]
public class ZoneConfig {
    public string name;
    public string tier;
    public int x;
    public int y;
    public WaveConfig[] waves;
    // 0.7-E.4: metadata de set (rojo/amarillo/verde + casco/peto/pantalon/guantes)
    public string setId = "";
    public string setPiece = "";
}

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
                    z.setId = ParseSetType(zc.setId);
                    z.setPiece = ParseSetPiece(zc.setPiece);
                    z.dungeon = new List<WorldBootstrap.WaveDef>();
                    if (zc.waves != null)
                    {
                        foreach (WaveConfig wc in zc.waves)
                        {
                            WorldBootstrap.WaveDef wd = new WorldBootstrap.WaveDef();
                            wd.spawns = new List<WorldBootstrap.SpawnDef>();
                            if (wc.spawns != null)
                            {
                                foreach (SpawnConfig sc in wc.spawns)
                                {
                                    wd.spawns.Add(new WorldBootstrap.SpawnDef
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

    static EnemyTier ParseTier(string s, EnemyTier fallback)
    {
        if (string.IsNullOrEmpty(s)) return fallback;
        if (s.ToLower().Contains("jefe") || s.ToLower().Contains("boss")) return EnemyTier.Jefe;
        if (s.ToLower().Contains("elite")) return EnemyTier.Elite;
        if (s.ToLower().Contains("medio")) return EnemyTier.Medio;
        return EnemyTier.Basico;
    }

    static List<WorldBootstrap.ZoneDef> DefaultZones()
    {
        List<WorldBootstrap.ZoneDef> d = new List<WorldBootstrap.ZoneDef>();
        d.Add(new WorldBootstrap.ZoneDef
        {
            name = "Cripta de los Penitentes",
            center = new Vector2Int(10, 8),
            tier = EnemyTier.Basico,
            dungeon = new List<WorldBootstrap.WaveDef>
            {
                new WorldBootstrap.WaveDef { spawns = new List<WorldBootstrap.SpawnDef> { S("penitent", EnemyTier.Basico, 7, 4) } }
            }
        });
        return d;
    }

    static WorldBootstrap.SpawnDef S(string archetype, EnemyTier tier, int x, int y)
    {
        return new WorldBootstrap.SpawnDef { archetype = archetype, tier = tier, cell = new Vector2Int(x, y) };
    }

    // 0.7-E.4: parsers de set
    static SetType ParseSetType(string s)
    {
        if (string.IsNullOrEmpty(s)) return SetType.Ninguno;
        string lower = s.ToLower();
        if (lower.Contains("rojo") || lower.Contains("judgment") || lower.Contains("juicio")) return SetType.Rojo;
        if (lower.Contains("amarillo") || lower.Contains("halo")) return SetType.Amarillo;
        if (lower.Contains("verde") || lower.Contains("plegaria")) return SetType.Verde;
        if (lower.Contains("aleatorio") || lower.Contains("random")) return SetType.Ninguno; // se resuelve al dropear
        return SetType.Ninguno;
    }

    static SetPieceType ParseSetPiece(string s)
    {
        if (string.IsNullOrEmpty(s)) return SetPieceType.Ninguna;
        string lower = s.ToLower();
        if (lower.Contains("casco") || lower.Contains("helm")) return SetPieceType.Casco;
        if (lower.Contains("peto") || lower.Contains("chest")) return SetPieceType.Peto;
        if (lower.Contains("pantalon") || lower.Contains("legs")) return SetPieceType.Pantalon;
        if (lower.Contains("guantes") || lower.Contains("gloves")) return SetPieceType.Guantes;
        return SetPieceType.Ninguna;
    }
}