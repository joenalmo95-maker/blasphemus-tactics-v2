using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }
    
    private GameObject root;
    private Image fillBar;
    private Text nameText;
    private Text subtitleText;
    private Unit currentBoss;
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    void Update()
    {
        if (currentBoss == null) return;
        
        if (currentBoss.currentHealth <= 0)
        {
            Hide();
            return;
        }
        
        UpdateBar();
    }
    
    public static void Show(Unit boss, string bossName, string subtitle)
    {
        if (Instance == null) new GameObject("BossHealthBarUI").AddComponent<BossHealthBarUI>();
        Instance.currentBoss = boss;
        Instance.Build(bossName, subtitle);
    }
    
    public static void Hide()
    {
        if (Instance != null && Instance.root != null)
        {
            Destroy(Instance.root);
            Instance.root = null;
            Instance.currentBoss = null;
        }
    }
    
    void Build(string bossName, string subtitle)
    {
        if (root != null) Destroy(root);
        
        root = new GameObject("BossHealthBarCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        root.AddComponent<GraphicRaycaster>();
        
        // Panel superior centrado
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.1f, 0.92f);
        prt.anchorMax = new Vector2(0.9f, 0.98f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        
        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = SpriteFactory.Square();
        panelImg.color = new Color(0.05f, 0.03f, 0.02f, 0.95f);
        
        // Borde dorado grueso
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-4, -4);
        brt.offsetMax = new Vector2(4, 4);
        Image brdImg = border.AddComponent<Image>();
        brdImg.sprite = SpriteFactory.Square();
        brdImg.color = new Color(0.8f, 0.7f, 0.2f, 1f);
        
        // Barra de relleno
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(panel.transform, false);
        RectTransform frt = fillObj.AddComponent<RectTransform>();
        frt.anchorMin = new Vector2(0.02f, 0.3f);
        frt.anchorMax = new Vector2(0.98f, 0.7f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;
        fillBar = fillObj.AddComponent<Image>();
        fillBar.sprite = SpriteFactory.Square();
        fillBar.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        fillBar.type = Image.Type.Filled;
        fillBar.fillMethod = Image.FillMethod.Horizontal;
        
        // Nombre del boss
        nameText = MakeText(panel.transform, bossName, new Vector2(0.5f, 0.85f), 24, new Color(1f, 0.9f, 0.4f));
        
        // Subtítulo
        if (!string.IsNullOrEmpty(subtitle))
        {
            subtitleText = MakeText(panel.transform, subtitle, new Vector2(0.5f, 0.15f), 14, new Color(0.7f, 0.7f, 0.7f));
        }
        
        UpdateBar();
    }
    
    void UpdateBar()
    {
        if (fillBar == null || currentBoss == null) return;
        float t = Mathf.Clamp01((float)currentBoss.currentHealth / currentBoss.maxHealth);
        fillBar.fillAmount = t;
    }
    
    Text MakeText(Transform parent, string content, Vector2 anchor, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(800, 40);
        rt.anchoredPosition = Vector2.zero;
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        return t;
    }
    
    Font GetFont()
    {
        return UIFactory.GetFont();
    }
    
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}