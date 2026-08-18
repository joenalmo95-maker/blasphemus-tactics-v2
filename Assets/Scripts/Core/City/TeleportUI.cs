using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TeleportUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static TeleportUI Instance { get; private set; }
    public static Vector2Int? PendingDestination = null;

    private GameObject root;

    private const float FULL_W = 600f;
    private const float FULL_H = 400f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public static void Toggle()
    {
        if (Instance == null) new GameObject("TeleportUI").AddComponent<TeleportUI>();
        IsOpen = !IsOpen;
        if (IsOpen) Instance.Rebuild(); else Instance.Close();
    }

    void Close()
    {
        IsOpen = false;
        if (root != null) Destroy(root);
        root = null;
    }

    void Rebuild()
    {
        if (root != null) Destroy(root);

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        root = new GameObject("TeleportCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 97;
        root.AddComponent<GraphicRaycaster>();

        RectTransform fullRt = UIFactory.CreatePanel(root.transform, "GreatMap",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.03f, 0.96f));

        UIFactory.CreateText(fullRt, "Title", "GRAN MAPA — ELIGE DESTINO DE TELETRANSPORTE", 24, TextAnchor.MiddleCenter, Color.cyan,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -30), new Vector2(800, 40));

        // Fondo: snapshot del mundo capturado por WorldMapUI
        GameObject imgObj = new GameObject("GreatMapImage");
        imgObj.transform.SetParent(fullRt, false);
        RectTransform imgRt = imgObj.AddComponent<RectTransform>();
        imgRt.anchorMin = new Vector2(0.5f, 0.5f);
        imgRt.anchorMax = new Vector2(0.5f, 0.5f);
        imgRt.anchoredPosition = Vector2.zero;
        imgRt.sizeDelta = new Vector2(FULL_W, FULL_H);
        RawImage raw = imgObj.AddComponent<RawImage>();
        raw.texture = WorldMapUI.WorldTexture != null ? WorldMapUI.WorldTexture : Texture2D.blackTexture;
        raw.color = new Color(1f, 1f, 1f, 0.9f);

        // Zonas como botones clicables sobre el mapa
        int w = WorldBootstrap.WorldWidth;
        int h = WorldBootstrap.WorldHeight;
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            float lx = ((z.center.x + 0.5f) / w - 0.5f) * FULL_W;
            float ly = ((z.center.y + 0.5f) / h - 0.5f) * FULL_H;
            WorldBootstrap.ZoneDef captured = z;

            GameObject btnObj = new GameObject("ZoneBtn_" + z.name);
            btnObj.transform.SetParent(imgRt, false);
            RectTransform brt = btnObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f);
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(lx, ly);
            brt.sizeDelta = new Vector2(160, 40);
            Image bimg = btnObj.AddComponent<Image>();
            bimg.sprite = SpriteFactory.Square();
            // 5.3: zonas de jefe en rojo en el Gran Mapa
            bimg.color = ObjectiveSystem.HasBoss(z)
                ? new Color(0.8f, 0.1f, 0.1f, 0.65f)
                : new Color(0.5f, 0.1f, 0.7f, 0.55f);
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bimg;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            Text t = txtObj.AddComponent<Text>();
            t.text = z.name + " [" + z.tier + "]" + (ObjectiveSystem.HasBoss(z) ? " [JEFE]" : "");
            t.font = GetFont();
            t.fontSize = 12;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            btn.onClick.AddListener(() => RequestTeleport(captured.center));
        }

        UIFactory.CreateText(fullRt, "Hint", "Clic en una zona para viajar · ESC cancela", 14, TextAnchor.MiddleCenter, Color.gray,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 30), new Vector2(600, 30));
    }

    void RequestTeleport(Vector2Int worldCell)
    {
        PendingDestination = worldCell;
        Close();
        GameFlow.ReturnToWorld();
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}