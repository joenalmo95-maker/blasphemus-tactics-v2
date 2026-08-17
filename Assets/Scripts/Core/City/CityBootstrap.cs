using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CityBootstrap : MonoBehaviour
{
    public const int CityWidth = 30;
    public const int CityHeight = 30;

    // Spawn separado del portal para evitar prompts instantáneos
    public static Vector2Int CitySpawn = new Vector2Int(15, 3);
    public static Vector2Int ExitPortal = new Vector2Int(15, 1);

    // FIX 3.1: auto-arranque. La ciudad se construye aunque la escena no tenga el componente.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != GameFlow.CityScene) return;

        // Elimina un WorldBootstrap residual heredado de una escena duplicada
        WorldBootstrap wb = Object.FindAnyObjectByType<WorldBootstrap>();
        if (wb != null) Object.Destroy(wb.gameObject);

        if (Object.FindAnyObjectByType<CityBootstrap>() != null) return;
        new GameObject("CityBootstrap").AddComponent<CityBootstrap>();
    }

    void Awake()
    {
        if (Object.FindAnyObjectByType<CharacterData>() == null)
            new GameObject("CharacterData").AddComponent<CharacterData>();
        if (Object.FindAnyObjectByType<InventorySystem>() == null)
            new GameObject("InventorySystem").AddComponent<InventorySystem>();
        if (Object.FindAnyObjectByType<PersistentManagers>() == null)
            new GameObject("PersistentManagers").AddComponent<PersistentManagers>();

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null) cam.tag = "MainCamera";
        }
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
        }

        GenerateCityObstacles();
        ClearAround(CitySpawn);
        ClearAround(ExitPortal);

        BuildGround();
        BuildExitPortal();
        BuildMerchant();
        MerchantUI.RefreshStock();
        BuildWarehouse();

        // UIs globales
        new GameObject("HUDUI").AddComponent<HUDUI>();
        new GameObject("InventoryUI").AddComponent<InventoryUI>();
        new GameObject("ShopUI").AddComponent<ShopUI>();

        SpawnPlayer();
    }

    void GenerateCityObstacles()
    {
        TerrainMap.Clear();

        // Perímetro amurallado
        for (int x = 0; x < CityWidth; x++)
        {
            TerrainMap.Set(new Vector2Int(x, 0), TerrainType.Roca);
            TerrainMap.Set(new Vector2Int(x, CityHeight - 1), TerrainType.Roca);
        }
        for (int y = 0; y < CityHeight; y++)
        {
            TerrainMap.Set(new Vector2Int(0, y), TerrainType.Roca);
            TerrainMap.Set(new Vector2Int(CityWidth - 1, y), TerrainType.Roca);
        }

        // Edificios
        for (int x = 5; x <= 8; x++) for (int y = 10; y <= 13; y++) TerrainMap.Set(new Vector2Int(x, y), TerrainType.Ruinas);   // Mercado
        for (int x = 20; x <= 24; x++) for (int y = 18; y <= 22; y++) TerrainMap.Set(new Vector2Int(x, y), TerrainType.Roca);   // Templo
        for (int x = 3; x <= 5; x++) for (int y = 20; y <= 22; y++) TerrainMap.Set(new Vector2Int(x, y), TerrainType.Ruinas);   // Herrería
        for (int x = 22; x <= 25; x++) for (int y = 6; y <= 9; y++) TerrainMap.Set(new Vector2Int(x, y), TerrainType.Roca);      // Barracones
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
        for (int x = 0; x < CityWidth; x++)
        {
            for (int y = 0; y < CityHeight; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                GameObject t = new GameObject("CTile_" + x + "_" + y);
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

    void BuildExitPortal()
    {
        GameObject p = new GameObject("CityExitPortal");
        p.transform.position = new Vector3(ExitPortal.x, ExitPortal.y, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = new Color(0.2f, 0.6f, 0.9f, 0.6f);
        sr.sortingOrder = 1;
        p.AddComponent<CityPortalTrigger>();
    }

    void BuildMerchant()
    {
        GameObject m = new GameObject("MerchantNPC");
        m.transform.position = new Vector3(12, 8, 0);
        SpriteRenderer sr = m.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get("healer");
        sr.color = new Color(1f, 0.9f, 0.4f);
        sr.sortingOrder = 2;
        m.transform.localScale = Vector3.one * 0.8f;
        m.AddComponent<MerchantNPC>();
    }

    void BuildWarehouse()
    {
        if (WarehouseSystem.Instance == null)
            new GameObject("WarehouseSystem").AddComponent<WarehouseSystem>();

        GameObject w = new GameObject("WarehouseNPC");
        w.transform.position = new Vector3(7, 21, 0);
        SpriteRenderer sr = w.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get("tank");
        sr.color = new Color(0.5f, 0.9f, 1f);
        sr.sortingOrder = 2;
        w.transform.localScale = Vector3.one * 0.8f;
        w.AddComponent<WarehouseNPC>();
    }    
    void SpawnPlayer()
    {
        GameObject p = new GameObject("CityPlayer");
        p.transform.position = new Vector3(CitySpawn.x, CitySpawn.y, 0);
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get(PlayerArt());
        sr.sortingOrder = 2;
        p.transform.localScale = Vector3.one * 0.8f;
        p.AddComponent<SelectionIndicator>();
        p.AddComponent<CityPlayerController>();
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

// FIX 3.1: el portal de la ciudad regresa al MUNDO (antes recargaba la ciudad)
public class CityPortalTrigger : MonoBehaviour
{
    void Update()
    {
        CityPlayerController pc = Object.FindAnyObjectByType<CityPlayerController>();
        if (pc == null) return;

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));
        if (Mathf.Abs(myCell.x - CityBootstrap.ExitPortal.x) <= 1 && Mathf.Abs(myCell.y - CityBootstrap.ExitPortal.y) <= 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameFlow.ReturnToWorld();
            }
        }
    }
}