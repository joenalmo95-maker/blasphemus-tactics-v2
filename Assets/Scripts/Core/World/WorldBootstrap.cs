using UnityEngine;
using System.Collections.Generic;

public class WorldBootstrap : MonoBehaviour
{
    public List<ClassData> availableClasses = new List<ClassData>();

    public const int WorldWidth = 30;
    public const int WorldHeight = 20;

    [System.Serializable]
    public struct Zone
    {
        public Vector2 center;
        public Vector2 size;
        public EnemyTier tier;
        public string label;
        public List<WaveDef> waves;
    }

    public static List<Zone> zones = new List<Zone>();
    public static GameObject worldPlayer;

    void Awake()
    {
        GameObject pm = new GameObject("PersistentManagers");
        PersistentManagers managers = pm.AddComponent<PersistentManagers>();
        managers.availableClasses = availableClasses;

        BuildGround();
        BuildZones();

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.03f, 0.03f, 0.05f);
        }

        new GameObject("InventoryUI").AddComponent<InventoryUI>();
        new GameObject("ShopUI").AddComponent<ShopUI>();
        new GameObject("HUDUI").AddComponent<HUDUI>();

        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            SpawnWorldPlayer();
        }
        else
        {
            GameObject c = new GameObject("CharacterCreation");
            CharacterCreationUI ui = c.AddComponent<CharacterCreationUI>();
            ui.availableClasses = availableClasses;
            ui.showContinue = SaveSystem.HasSave();
            ui.onFinished = SpawnWorldPlayer;
            ui.Build();
        }
    }

    void BuildGround()
    {
        for (int x = 0; x < WorldWidth; x++)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                GameObject t = new GameObject("WTile_" + x + "_" + y);
                t.transform.position = new Vector3(x, y, 0);
                SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
                sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
            }
        }
    }

    void BuildZones()
    {
        zones.Clear();

        List<WaveDef> d1 = new List<WaveDef>
        {
            Wave(S("penitent", EnemyTier.Basico, 7, 4)),
            Wave(S("penitent", EnemyTier.Basico, 6, 2), S("cherub", EnemyTier.Basico, 8, 6))
        };

        List<WaveDef> d2 = new List<WaveDef>
        {
            Wave(S("penitent", EnemyTier.Medio, 7, 4), S("cherub", EnemyTier.Medio, 5, 6)),
            Wave(S("inquisitor", EnemyTier.Medio, 7, 5), S("penitent", EnemyTier.Medio, 5, 3)),
            Wave(S("capitan", EnemyTier.Elite, 7, 5))
        };

        List<WaveDef> d3 = new List<WaveDef>
        {
            Wave(S("penitent", EnemyTier.Medio, 7, 4), S("inquisitor", EnemyTier.Medio, 5, 6)),
            Wave(S("capitan", EnemyTier.Elite, 7, 5), S("cherub", EnemyTier.Medio, 4, 3)),
            Wave(S("capitan", EnemyTier.EliteFuerte, 6, 4), S("inquisitor", EnemyTier.Elite, 8, 6)),
            Wave(S("boss", EnemyTier.Jefe, 7, 5))
        };

        zones.Add(new Zone { center = new Vector2(8, 10), size = new Vector2(5, 5), tier = EnemyTier.Basico, label = "Campos Penitentes", waves = d1 });
        zones.Add(new Zone { center = new Vector2(18, 6), size = new Vector2(5, 5), tier = EnemyTier.Medio, label = "Vigilia del Inquisidor", waves = d2 });
        zones.Add(new Zone { center = new Vector2(24, 14), size = new Vector2(5, 5), tier = EnemyTier.Elite, label = "Bastión Templario", waves = d3 });

        foreach (Zone z in zones)
        {
            GameObject zObj = new GameObject("Zone_" + z.label);
            zObj.transform.position = new Vector3(z.center.x, z.center.y, 0);
            SpriteRenderer sr = zObj.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square();
            sr.color = new Color(0.5f, 0.05f, 0.05f, 0.25f);
            zObj.transform.localScale = new Vector3(z.size.x, z.size.y, 1);
            sr.sortingOrder = 1;
        }
    }

    static WaveDef Wave(params SpawnDef[] s)
    {
        WaveDef w = new WaveDef();
        w.spawns.AddRange(s);
        return w;
    }

    static SpawnDef S(string archetype, EnemyTier tier, int x, int y)
    {
        return new SpawnDef { archetype = archetype, tier = tier, cell = new Vector2Int(x, y) };
    }

    public void SpawnWorldPlayer()
    {
        if (worldPlayer != null) return;

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

        GameObject p = new GameObject("WorldPlayer");
        p.transform.position = new Vector3(2, 10, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get(art);
        sr.color = Color.white;
        sr.sortingOrder = 5;
        p.transform.localScale = Vector3.one * 0.9f;
        p.AddComponent<WorldPlayerController>();
        worldPlayer = p;

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(2, 10, -10);
            Camera.main.orthographicSize = 6;
        }
    }
}