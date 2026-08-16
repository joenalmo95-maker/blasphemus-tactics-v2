using UnityEngine;
using System.Collections.Generic;

public class WorldBootstrap : MonoBehaviour
{
    public const int WorldWidth = 30;
    public const int WorldHeight = 20;

    public static Vector2Int PlayerSpawn = new Vector2Int(2, 2);

    public class ZoneDef
    {
        public string name;
        public Vector2Int center;
        public EnemyTier tier;
        public List<WaveDef> dungeon;
    }

    // 3 zonas-mazmorra data-driven (básicos → elite → elite fuerte → jefe)
    public static List<ZoneDef> Zones = new List<ZoneDef>
    {
        new ZoneDef
        {
            name = "Cripta de los Penitentes",
            center = new Vector2Int(8, 5),
            tier = EnemyTier.Basico,
            dungeon = new List<WaveDef>
            {
                new WaveDef { spawns = new List<SpawnDef> { S("penitent", EnemyTier.Basico, 7, 4), S("penitent", EnemyTier.Basico, 8, 5) } },
                new WaveDef { spawns = new List<SpawnDef> { S("cherub", EnemyTier.Basico, 7, 4) } }
            }
        },
        new ZoneDef
        {
            name = "Coro de Querubines",
            center = new Vector2Int(15, 10),
            tier = EnemyTier.Medio,
            dungeon = new List<WaveDef>
            {
                new WaveDef { spawns = new List<SpawnDef> { S("cherub", EnemyTier.Medio, 7, 4), S("inquisitor", EnemyTier.Medio, 8, 5) } },
                new WaveDef { spawns = new List<SpawnDef> { S("inquisitor", EnemyTier.Medio, 7, 4) } }
            }
        },
        new ZoneDef
        {
            name = "Trono del Capitán",
            center = new Vector2Int(25, 15),
            tier = EnemyTier.EliteFuerte,
            dungeon = new List<WaveDef>
            {
                new WaveDef { spawns = new List<SpawnDef> { S("capitan", EnemyTier.Elite, 7, 4), S("cherub", EnemyTier.Elite, 8, 5) } },
                new WaveDef { spawns = new List<SpawnDef> { S("angel", EnemyTier.Jefe, 7, 4) } }
            }
        }
    };

    void Awake()
    {
        // Regla documentada: garantizar cámara con Tag MainCamera en WorldMap
        if (Camera.main == null)
        {
            Camera cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null) cam.tag = "MainCamera";
        }

        TerrainMap.GenerateWorldObstacles(WorldWidth, WorldHeight);

        // Nunca bloquear spawn del jugador ni accesos a zonas
        ClearAround(PlayerSpawn);
        foreach (ZoneDef z in Zones) ClearAround(z.center);

        BuildGround();
        BuildZoneMarkers();
        SpawnPlayer();
    }

    static void ClearAround(Vector2Int c)
    {
        TerrainMap.Set(c, TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(1, 0), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(-1, 0), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(0, 1), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(0, -1), TerrainType.Caminable);
    }

    void BuildGround()
    {
        for (int x = 0; x < WorldWidth; x++)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                GameObject t = new GameObject("WTile_" + x + "_" + y);
                t.transform.position = new Vector3(x, y, 0);
                SpriteRenderer sr = t.AddComponent<SpriteRenderer>();

                TerrainType terrain = TerrainMap.Get(cell);
                if (terrain != TerrainType.Caminable)
                {
                    sr.sprite = ArtProvider.Get(terrain == TerrainType.Roca ? "rock" : (terrain == TerrainType.Agua ? "water" : "ruins"));
                    sr.sortingOrder = 1;
                }
                else
                {
                    sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
                    sr.sortingOrder = 0;
                }
            }
        }
    }

    void BuildZoneMarkers()
    {
        foreach (ZoneDef z in Zones)
        {
            GameObject m = new GameObject("Zone_" + z.name);
            m.transform.position = new Vector3(z.center.x, z.center.y, 0);
            SpriteRenderer sr = m.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square();
            sr.color = new Color(0.6f, 0.2f, 0.8f, 0.35f);
            m.transform.localScale = Vector3.one * 0.9f;
            sr.sortingOrder = 1;
        }
    }

    void SpawnPlayer()
    {
        GameObject p = new GameObject("WorldPlayer");
        p.transform.position = new Vector3(PlayerSpawn.x, PlayerSpawn.y, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get(PlayerArt());
        sr.sortingOrder = 2;
        p.AddComponent<WorldPlayerController>();
    }

    string PlayerArt()
    {
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            switch (CharacterData.Instance.classData.role)
            {
                case ClassRole.Tank: return "tank";
                case ClassRole.Healer: return "healer";
                default: return "dps";
            }
        }
        return "dps";
    }

    static SpawnDef S(string archetype, EnemyTier tier, int x, int y)
    {
        return new SpawnDef { archetype = archetype, tier = tier, cell = new Vector2Int(x, y) };
    }
}