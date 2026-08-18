using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldBootstrap : MonoBehaviour
{
    public const int WorldWidth = 60;
    public const int WorldHeight = 40;

    public static Vector2Int PlayerSpawn = new Vector2Int(2, 2);

    // Si aparece vacío en el Inspector de WorldMap, arrastre aquí los 3 assets
    // de ScriptableObjects/Classes (igual que en el Bootstrap de combate).
    public List<ClassData> availableClasses = new List<ClassData>();

    public class ZoneDef
    {
        public string name;
        public Vector2Int center;
        public EnemyTier tier;
        public List<WaveDef> dungeon;
    }

    // Data-driven: se pobla desde Assets/Resources/ZonesConfig.json (Bloque 2.4)
    public static List<ZoneDef> Zones = new List<ZoneDef>();

    void Awake()
    {
        // GUARD: WorldBootstrap solo vive en WorldMap (protege CityScene duplicada)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameFlow.CityScene)
        {
            Destroy(gameObject);
            return;
        }

        // Singletons persistentes (patrón del Bootstrap de combate)
        if (Object.FindAnyObjectByType<CharacterData>() == null)
            new GameObject("CharacterData").AddComponent<CharacterData>();
        if (Object.FindAnyObjectByType<InventorySystem>() == null)
            new GameObject("InventorySystem").AddComponent<InventorySystem>();
        if (Object.FindAnyObjectByType<PersistentManagers>() == null)
            new GameObject("PersistentManagers").AddComponent<PersistentManagers>();

        // Cámara: Tag MainCamera + fondo NEGRO
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null) cam.tag = "MainCamera";
        }
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // Zonas data-driven con fallback seguro
        Zones = ZoneConfigLoader.Load();

        // Mapa dibujado por el usuario; si no existe, respaldo procedural
        if (!TerrainMap.TryLoadWorldMap(WorldWidth, WorldHeight))
            TerrainMap.GenerateWorldObstacles(WorldWidth, WorldHeight);

        // 3.4: el teletransporte desde la ciudad tiene prioridad absoluta
        if (TeleportUI.PendingDestination.HasValue)
        {
            PlayerSpawn = TeleportUI.PendingDestination.Value;
            TeleportUI.PendingDestination = null;
            PlayerPrefs.DeleteKey("LastWorldX");
            PlayerPrefs.DeleteKey("LastWorldY");
        }
        // 3.1: restaura la posición guardada al volver desde la ciudad por el portal
        else if (PlayerPrefs.HasKey("LastWorldX"))
        {
            PlayerSpawn = new Vector2Int(PlayerPrefs.GetInt("LastWorldX", 2), PlayerPrefs.GetInt("LastWorldY", 2));
            PlayerPrefs.DeleteKey("LastWorldX");
            PlayerPrefs.DeleteKey("LastWorldY");
        }

        ClearAround(PlayerSpawn);
        foreach (ZoneDef z in Zones) ClearAround(z.center);

        BuildGround();
        BuildZoneMarkers();
        BuildCityPortal();

        new GameObject("WorldSpawnManager").AddComponent<WorldSpawnManager>();
        new GameObject("WorldChestManager").AddComponent<WorldChestManager>();
        new GameObject("WorldEncounterManager").AddComponent<WorldEncounterManager>();

        // UI global disponible en mundo
        new GameObject("HUDUI").AddComponent<HUDUI>();
        new GameObject("InventoryUI").AddComponent<InventoryUI>();
        new GameObject("ShopUI").AddComponent<ShopUI>();
        new GameObject("WorldMapUI").AddComponent<WorldMapUI>();

        // La creación/continuación de personaje vive en MUNDO, no en mazmorra
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            SpawnPlayer();
        }
        else
        {
            GameObject c = new GameObject("CharacterCreation");
            CharacterCreationUI ui = c.AddComponent<CharacterCreationUI>();
            ui.availableClasses = availableClasses;
            ui.showContinue = SaveSystem.HasSave();
            ui.onFinished = () => { SpawnPlayer(); };
            ui.Build();
        }
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
                sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
                sr.sortingOrder = 0;

                TerrainType terrain = TerrainMap.Get(cell);
                if (terrain != TerrainType.Caminable)
                {
                    GameObject ob = new GameObject("Obstacle");
                    ob.transform.SetParent(t.transform);
                    ob.transform.localPosition = Vector3.zero;
                    SpriteRenderer osr = ob.AddComponent<SpriteRenderer>();
                    osr.sprite = ArtProvider.Get(terrain == TerrainType.Roca ? "rock" : (terrain == TerrainType.Agua ? "water" : "ruins"));
                    osr.sortingOrder = 1;
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

    void BuildCityPortal()
    {
        GameObject p = new GameObject("WorldToCityPortal");
        p.transform.position = new Vector3(2, 38, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = new Color(0.2f, 0.6f, 0.9f, 0.6f);
        sr.sortingOrder = 1;
        p.AddComponent<WorldToCityPortalTrigger>();
    }

    void SpawnPlayer()
    {
        // 5.1: restaura bloques extendidos del guardado versionado
        SaveSystem.ApplyExtendedOnce();
        // 1.1-B: garantiza loadout (migra starters si el save es anterior)
        LoadoutSystem.EnsureInitialized();

        GameObject p = new GameObject("WorldPlayer");
        p.transform.position = new Vector3(PlayerSpawn.x, PlayerSpawn.y, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get(PlayerArt());
        sr.sortingOrder = 2;
        p.transform.localScale = Vector3.one * 0.8f;
        p.AddComponent<SelectionIndicator>();
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
}

public class WorldToCityPortalTrigger : MonoBehaviour
{
    private Text promptText;

    void Awake()
    {
        GameObject canvas = UIFactory.CreateCanvas("CityPortalPromptCanvas", 43);
        promptText = UIFactory.CreateText(canvas.transform, "PortalPrompt", "", 16, TextAnchor.MiddleCenter,
            new Color(0.4f, 0.8f, 1f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 160), new Vector2(500, 30));
    }

    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));
        Vector2Int portal = new Vector2Int(2, 38);

        if (Mathf.Abs(myCell.x - portal.x) <= 1 && Mathf.Abs(myCell.y - portal.y) <= 1)
        {
            promptText.text = "Pulsa E para entrar a la Ciudad";
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerPrefs.SetInt("LastWorldX", myCell.x);
                PlayerPrefs.SetInt("LastWorldY", myCell.y);
                GameFlow.EnterCity();
            }
        }
        else
        {
            promptText.text = "";
        }
    }
}