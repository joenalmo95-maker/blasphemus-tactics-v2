using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class WorldBootstrap : MonoBehaviour
{
    // 0.1: Mundo expandido 4x (120x80 = 9,600 celdas)
    public const int WorldWidth = 120;
    public const int WorldHeight = 80;
    
    // Spawn del jugador en el Bastión de San Veritas (Región I)
    public static Vector2Int PlayerSpawn = new Vector2Int(2, 38);

    // Portal de salida a la ciudad (junto al bastión)
    public static Vector2Int CityPortal = new Vector2Int(5, 38);
    
    // Última posición conocida del jugador (para restaurar al volver de la ciudad)
    public static Vector2Int LastKnownPosition = PlayerSpawn;

    // === CLASES ANIDADAS PARA ZONAS ===
    public class ZoneDef
    {
        public string name;
        public Vector2Int center;
        public EnemyTier tier;
        public List<WaveDef> dungeon;
    }

    public class WaveDef
    {
        public List<SpawnDef> spawns;
    }

    public class SpawnDef
    {
        public string archetype;
        public EnemyTier tier;
        public Vector2Int cell;
    }

    // Lista de zonas (se carga desde ZoneConfigLoader)
    public static List<ZoneDef> Zones = new List<ZoneDef>();

    // 0.2: Lista de regiones (se carga desde RegionConfigLoader)
    public static List<RegionData> Regions = new List<RegionData>();

    void Awake()
    {
        // Guard anti-secuestro
        if (Object.FindObjectsByType<WorldBootstrap>(FindObjectsInactive.Exclude).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Inicializar managers persistentes
        if (Object.FindAnyObjectByType<CharacterData>() == null)
            new GameObject("CharacterData").AddComponent<CharacterData>();
        if (Object.FindAnyObjectByType<InventorySystem>() == null)
            new GameObject("InventorySystem").AddComponent<InventorySystem>();
        if (Object.FindAnyObjectByType<PersistentManagers>() == null)
            new GameObject("PersistentManagers").AddComponent<PersistentManagers>();

        // Configurar cámara
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.transform.position = new Vector3(PlayerSpawn.x, PlayerSpawn.y, -10f);

        // Restaurar posición si venimos de la ciudad
        int savedX = PlayerPrefs.GetInt("LastWorldX", -1);
        int savedY = PlayerPrefs.GetInt("LastWorldY", -1);
        if (savedX >= 0 && savedY >= 0)
        {
            PlayerSpawn = new Vector2Int(savedX, savedY);
            LastKnownPosition = PlayerSpawn;
            PlayerPrefs.DeleteKey("LastWorldX");
            PlayerPrefs.DeleteKey("LastWorldY");
        }

        // Cargar zonas usando el loader correcto
        Zones = ZoneConfigLoader.Load();
        Debug.Log("[WorldBootstrap] Zonas cargadas: " + Zones.Count);

        // 0.2: Cargar regiones
        Regions = RegionConfigLoader.Load();
        Debug.Log("[WorldBootstrap] Regiones cargadas: " + Regions.Count);

        // Cargar mapa (generar si no existe o es antiguo)
        EnsureWorldMapExists();
        TerrainMap.TryLoadWorldMap(WorldWidth, WorldHeight);
        BuildWorld();
        SpawnPlayer();
        BuildUI();
        if (!SaveSystem.HasLoadedThisSession && SaveSystem.HasSave())
        {
            SaveSystem.Load();
        }
    }

    void EnsureWorldMapExists()
    {
        string path = Path.Combine(Application.dataPath, "Resources", "WorldMapData.txt");
        
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == WorldHeight && lines[0].Length == WorldWidth)
            {
                Debug.Log("[WorldBootstrap] WorldMapData.txt cargado (120x80).");
                return;
            }
            Debug.Log("[WorldBootstrap] WorldMapData.txt antiguo o corrupto. Regenerando...");
        }

        Debug.Log("[WorldBootstrap] Generando mapa mundial 120x80 con Región I (Valle de la Luz Eterna)...");
        char[,] grid = new char[WorldWidth, WorldHeight];

        // Rellenar todo con X (zonas bloqueadas)
        for (int x = 0; x < WorldWidth; x++)
            for (int y = 0; y < WorldHeight; y++)
                grid[x, y] = 'X';

        // Pintar Región I (filas 0-39, columnas 0-59)
        for (int x = 0; x < 60; x++)
            for (int y = 0; y < 40; y++)
                grid[x, y] = '.';

        // Río inferior
        for (int x = 0; x < 60; x++)
            for (int y = 0; y < 3; y++)
                grid[x, y] = '~';

        // Bastión de San Veritas
        for (int x = 1; x <= 5; x++)
            for (int y = 37; y <= 39; y++)
                grid[x, y] = 'B';

        // Spawn del jugador (fuera del bastión, en terreno caminable)
        grid[6, 38] = 'P';
        // Portal a la ciudad (junto al spawn, en terreno caminable)
        grid[7, 38] = '.'; // Será marcado como portal por código

        // Campos de Penitencia
        for (int i = 0; i < 15; i++)
        {
            int tx = 8 + Random.Range(0, 18);
            int ty = 3 + Random.Range(0, 8);
            if (tx < 60 && ty < 40) grid[tx, ty] = 'T';
        }

        // Camino de los Peregrinos
        for (int x = 0; x < 60; x++)
        {
            grid[x, 15] = '.';
            if (x % 7 == 0) grid[x, 14] = 'T';
            if (x % 9 == 0) grid[x, 16] = 'T';
        }

        // Bosque de los Cirios
        for (int i = 0; i < 80; i++)
        {
            int tx = 10 + Random.Range(0, 31);
            int ty = 20 + Random.Range(0, 11);
            if (tx < 60 && ty < 40) grid[tx, ty] = 'T';
        }

        // Monasterio de Santa Lucía
        for (int x = 45; x <= 55; x++)
            for (int y = 30; y <= 35; y++)
            {
                if (x == 45 || x == 55 || y == 30 || y == 35)
                    grid[x, y] = '#';
                else
                    grid[x, y] = 'R';
            }

        // Canteras de Aurelia
        for (int i = 0; i < 40; i++)
        {
            int rx = 40 + Random.Range(0, 16);
            int ry = 5 + Random.Range(0, 8);
            if (rx < 60 && ry < 40) grid[rx, ry] = '#';
        }

        // Cementerio de los Devotos
        for (int i = 0; i < 25; i++)
        {
            int rx = 2 + Random.Range(0, 14);
            int ry = 25 + Random.Range(0, 8);
            if (rx < 60 && ry < 40) grid[rx, ry] = 'R';
        }

        // Ruinas de la Primera Catedral
        for (int i = 0; i < 30; i++)
        {
            int rx = 20 + Random.Range(0, 16);
            int ry = 10 + Random.Range(0, 9);
            if (rx < 60 && ry < 40) grid[rx, ry] = (i % 2 == 0) ? 'R' : '#';
        }

        // Lago sagrado
        for (int x = 25; x <= 32; x++)
            for (int y = 8; y <= 12; y++)
            {
                int dx = x - 28;
                int dy = y - 10;
                if (dx * dx + dy * dy <= 9)
                    grid[x, y] = '~';
            }

        StringBuilder sb = new StringBuilder();
        for (int y = WorldHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < WorldWidth; x++)
                sb.Append(grid[x, y]);
            if (y > 0) sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString());
        Debug.Log("[WorldBootstrap] Mapa 120x80 generado y guardado en WorldMapData.txt");
    }

    void BuildWorld()
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
                
                if (terrain == TerrainType.Caminable)
                {
                    sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
                    sr.sortingOrder = 0;
                }
                else
                {
                    sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
                    sr.sortingOrder = 0;
                    
                    GameObject ob = new GameObject("Obstacle");
                    ob.transform.SetParent(t.transform);
                    ob.transform.localPosition = Vector3.zero;
                    SpriteRenderer obsr = ob.AddComponent<SpriteRenderer>();
                    obsr.sortingOrder = 1;
                    
                    switch (terrain)
                    {
                        case TerrainType.Roca:
                            obsr.sprite = ArtProvider.Get("rock");
                            break;
                        case TerrainType.Agua:
                            obsr.sprite = ArtProvider.Get("water");
                            break;
                        case TerrainType.Ruinas:
                            obsr.sprite = ArtProvider.Get("ruins");
                            break;
                    }
                    
                    BoxCollider2D col = ob.AddComponent<BoxCollider2D>();
                    col.size = Vector2.one * 0.9f;
                }
            }
        }

        // Bastión
        for (int x = 1; x <= 5; x++)
        {
            for (int y = 37; y <= 39; y++)
            {
                GameObject bastion = new GameObject("Bastion_" + x + "_" + y);
                bastion.transform.position = new Vector3(x, y, 0);
                SpriteRenderer sr = bastion.AddComponent<SpriteRenderer>();
                sr.sprite = ArtProvider.Get("ruins");
                sr.color = new Color(0.6f, 0.5f, 0.3f);
                sr.sortingOrder = 2;
                BoxCollider2D col = bastion.AddComponent<BoxCollider2D>();
                col.size = Vector2.one * 0.9f;
            }
        }

        // Portal
        GameObject portal = new GameObject("CityPortal");
        portal.transform.position = new Vector3(CityPortal.x, CityPortal.y, 0);
        SpriteRenderer psr = portal.AddComponent<SpriteRenderer>();
        psr.sprite = SpriteFactory.Square();
        psr.color = new Color(0.2f, 0.6f, 0.9f, 0.6f);
        psr.sortingOrder = 1;
        portal.AddComponent<CityPortalTrigger>();
    }

    void SpawnPlayer()
    {
        GameObject p = new GameObject("WorldPlayer");
        p.transform.position = new Vector3(PlayerSpawn.x, PlayerSpawn.y, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get("inquisitor");
        sr.sortingOrder = 2;
        p.transform.localScale = Vector3.one * 0.8f;
        p.AddComponent<SelectionIndicator>();
        p.AddComponent<WorldPlayerController>();
    }

    void BuildUI()
    {
        if (Object.FindAnyObjectByType<HUDUI>() == null)
            new GameObject("HUDUI").AddComponent<HUDUI>();
        if (Object.FindAnyObjectByType<ActionBarUI>() == null)
            new GameObject("ActionBarUI").AddComponent<ActionBarUI>();
        if (Object.FindAnyObjectByType<WorldMapUI>() == null)
            new GameObject("WorldMapUI").AddComponent<WorldMapUI>();
        
        // FIX: Agregar InventoryUI para que funcione en el mundo
        if (Object.FindAnyObjectByType<InventoryUI>() == null)
            new GameObject("InventoryUI").AddComponent<InventoryUI>();
    }
}