using UnityEngine;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    private Texture2D mapTex;
    public static Texture2D WorldTexture;
    private RectTransform miniPlayerDot;
    private RectTransform fullPlayerDot;
    private GameObject fullMapRoot;

    private const float MINI_W = 180f;
    private const float MINI_H = 120f;
    private const float FULL_W = 600f;
    private const float FULL_H = 400f;

    private int W { get { return WorldBootstrap.WorldWidth; } }
    private int H { get { return WorldBootstrap.WorldHeight; } }

    void Awake()
    {
        BuildTexture();
        Build();
    }

    void BuildTexture()
    {
        mapTex = new Texture2D(W, H);
        mapTex.filterMode = FilterMode.Point;

        Color[] px = new Color[W * H];
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                TerrainType t = TerrainMap.Get(new Vector2Int(x, y));
                Color c = new Color(0.10f, 0.10f, 0.10f);
                if (t == TerrainType.Roca) c = new Color(0.45f, 0.45f, 0.50f);
                else if (t == TerrainType.Agua) c = new Color(0.15f, 0.40f, 0.80f);
                else if (t == TerrainType.Ruinas) c = new Color(0.55f, 0.42f, 0.28f);
                px[y * W + x] = c;
            }
        }

        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (z.center.x >= 0 && z.center.x < W && z.center.y >= 0 && z.center.y < H)
                px[z.center.y * W + z.center.x] = new Color(0.70f, 0.30f, 0.90f);
        }

        mapTex.SetPixels(px);
        mapTex.Apply();

        // 3.4: snapshot del mapa mundial para el Gran Mapa del teletransporte
        WorldTexture = mapTex;
    }

    void Build()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldMapUICanvas", 45);

        // --- MINIMAPA PERMANENTE (superior derecha) ---
        RectTransform miniPanel = UIFactory.CreatePanel(canvas.transform, "Minimap",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-10, -10), new Vector2(MINI_W + 6, MINI_H + 6), new Color(0.05f, 0.05f, 0.08f, 0.9f));

        RectTransform miniImgRt = CreateRawImage(miniPanel.transform, "MinimapImage", MINI_W, MINI_H);
        miniPlayerDot = CreateDot(miniImgRt, new Vector2(4, 4), Color.green);

        UIFactory.CreateText(canvas.transform, "MinimapHint", "M: mapa completo", 12, TextAnchor.UpperRight, Color.gray,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-10, -(MINI_H + 22)), new Vector2(MINI_W + 6, 20));

        // --- MAPA COMPLETO (tecla M) ---
        RectTransform fullRt = UIFactory.CreatePanel(canvas.transform, "FullMap",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.03f, 0.95f));
        fullMapRoot = fullRt.gameObject;

        UIFactory.CreateText(fullRt, "Title", "MAPA DE LA CRUZADA", 24, TextAnchor.MiddleCenter, Color.yellow,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -30), new Vector2(600, 40));

        RectTransform fullImgRt = CreateRawImage(fullRt, "FullMapImage", FULL_W, FULL_H);
        fullPlayerDot = CreateDot(fullImgRt, new Vector2(8, 8), Color.green);

        // Nombres de zona sobre el mapa completo
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            float lx = ((z.center.x + 0.5f) / W - 0.5f) * FULL_W;
            float ly = ((z.center.y + 0.5f) / H - 0.5f) * FULL_H;
            UIFactory.CreateText(fullImgRt, "Zone_" + z.name, z.name, 12, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(lx, ly + 14), new Vector2(220, 24));
        }

        UIFactory.CreateText(fullRt, "CloseHint", "M para cerrar", 14, TextAnchor.MiddleCenter, Color.gray,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 30), new Vector2(400, 30));

        fullMapRoot.SetActive(false);
    }

    RectTransform CreateRawImage(Transform parent, string name, float w, float h)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(w, h);
        RawImage raw = go.AddComponent<RawImage>();
        raw.texture = mapTex;
        return rt;
    }

    RectTransform CreateDot(Transform parent, Vector2 size, Color color)
    {
        RectTransform rt = UIFactory.CreatePanel(parent, "PlayerDot",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, size, color);
        rt.SetAsLastSibling();
        return rt;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !InventoryUI.IsOpen && !ShopUI.IsOpen)
        {
            fullMapRoot.SetActive(!fullMapRoot.activeSelf);
        }

        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc != null)
        {
            Vector3 p = pc.transform.position;
            SetDot(miniPlayerDot, p, MINI_W, MINI_H);
            SetDot(fullPlayerDot, p, FULL_W, FULL_H);
        }
    }

    void SetDot(RectTransform dot, Vector3 p, float mapW, float mapH)
    {
        if (dot == null) return;
        float nx = Mathf.Clamp01((p.x + 0.5f) / W);
        float ny = Mathf.Clamp01((p.y + 0.5f) / H);
        dot.anchoredPosition = new Vector2((nx - 0.5f) * mapW, (ny - 0.5f) * mapH);
    }
}