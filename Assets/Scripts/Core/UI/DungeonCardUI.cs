using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class DungeonCardUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static DungeonCardUI Instance { get; private set; }

    private GameObject root;
    private WorldBootstrap.ZoneDef zone;
    private System.Action onConfirm;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public static void Show(WorldBootstrap.ZoneDef z, System.Action confirm)
    {
        if (Instance == null) new GameObject("DungeonCardUI").AddComponent<DungeonCardUI>();
        Instance.zone = z;
        Instance.onConfirm = confirm;
        IsOpen = true;
        Instance.Rebuild();
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

        root = new GameObject("DungeonCardCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 97;
        root.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        Image bimg = bg.AddComponent<Image>();
        bimg.sprite = SpriteFactory.Square();
        bimg.color = new Color(0.02f, 0.02f, 0.03f, 0.92f);

        if (zone == null) { Close(); return; }

        MakeText(root.transform, "TARJETA DE MAZMORRA", 0, 250, 22, Color.white);
        MakeText(root.transform, zone.name + " [" + zone.tier + "]", 0, 210, 20, Color.magenta);
        MakeText(root.transform, "Entradas de hoy: " + DungeonDaily.Count + "/" + DungeonDaily.MaxPerDay
            + " · Restantes: " + DungeonDaily.Remaining(), 0, 178, 15, Color.yellow);

        StringBuilder sb = new StringBuilder();
        int totalXp = 0;
        int gmin = 0;
        int gmax = 0;
        for (int i = 0; i < zone.dungeon.Count; i++)
        {
            sb.AppendLine("Oleada " + (i + 1) + ":");
            foreach (WorldBootstrap.SpawnDef sp in zone.dungeon[i].spawns)
            {
                sb.AppendLine("  - " + sp.archetype + " (" + sp.tier + ")");
                totalXp += LootSystem.XpForTier(sp.tier);
                LootSystem.GoldRange(sp.tier, out int a, out int b);
                gmin += a;
                gmax += b;
            }
        }

        MakeText(root.transform, sb.ToString(), 0, 40, 14, Color.white, new Vector2(700, 220));
        MakeText(root.transform, "Recompensas estimadas: Oro " + gmin + "-" + gmax + " ┬À EXP " + totalXp, 0, -110, 15, Color.cyan);

        // 0.7-E.4b2: mostrar pieza de set que dropea y pity actual
        string setInfo = "";
        if (zone.setPiece != SetPieceType.Ninguna)
        {
            string pityKey = "SetPity_" + zone.name;
            int pity = PlayerPrefs.GetInt(pityKey, 0);
            bool isFixed = zone.setId != SetType.Ninguno;
            string setName = isFixed ? SetBonusSystem.SetName(zone.setId) : "aleatorio";
            string pieceName = SetBonusSystem.PieceName(zone.setPiece);
            setInfo = "Dropea: " + pieceName + " (" + setName + ") - Pity " + pity + "/80";
        }
        if (!string.IsNullOrEmpty(setInfo))
        MakeText(root.transform, setInfo, 0, -140, 14, new Color(1f, 0.85f, 0.3f));
        float escY = string.IsNullOrEmpty(setInfo) ? -140 : -170;
        MakeText(root.transform, "ESC cancela sin consumir entrada", 0, escY, 12, Color.gray);

        MakeButton(root.transform, "ENTRAR", -110, -200, 200, 44, Color.green, () =>
        {
            System.Action cb = onConfirm;
            Close();
            if (cb != null) cb();
        });

        MakeButton(root.transform, "CANCELAR", 110, -200, 200, 44, Color.red, () => Close());
    }

    void MakeText(Transform parent, string content, float x, float y, int size, Color color)
    {
        MakeText(parent, content, x, y, size, color, new Vector2(900, 40));
    }

    void MakeText(Transform parent, string content, float x, float y, int size, Color color, Vector2 sizeDelta)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = sizeDelta;
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void MakeButton(Transform parent, string label, float x, float y, float w, float h, Color textColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        Image img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(go.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = GetFont();
        t.fontSize = 16;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = textColor;
    }

    static Font GetFont()
    {
        return UIFactory.GetFont();
    }
}