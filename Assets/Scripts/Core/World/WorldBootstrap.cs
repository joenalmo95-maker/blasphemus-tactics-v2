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
        zones.Add(new Zone { center = new Vector2(8, 10), size = new Vector2(5, 5), tier = EnemyTier.Basico, label = "Campos Penitentes" });
        zones.Add(new Zone { center = new Vector2(18, 6), size = new Vector2(5, 5), tier = EnemyTier.Medio, label = "Vigilia del Inquisidor" });
        zones.Add(new Zone { center = new Vector2(24, 14), size = new Vector2(5, 5), tier = EnemyTier.Elite, label = "Bastión Templario" });

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