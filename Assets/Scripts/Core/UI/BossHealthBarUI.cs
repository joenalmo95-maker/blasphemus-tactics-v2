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
        // 1. Priorizar el enemigo seleccionado por el jugador (Bug 0.2 + 0.6)
        Unit target = InputController.SelectedEnemy;
        
        // 2. Fallback: si no hay selección válida, mantener el currentBoss actual
        if (target == null || !target.isEnemy || target.currentHealth <= 0)
        {
            target = currentBoss;
        }

        // 3. Si no hay target válido, ocultar la barra
        if (target == null || target.currentHealth <= 0)
        {
            if (root != null) root.SetActive(false);
            return;
        }

        // 4. Mostrar la barra si estaba oculta
        if (root != null && !root.activeSelf) root.SetActive(true);

        // 5. Si cambió el target, actualizar textos dinámicamente
        if (target != currentBoss)
        {
            currentBoss = target;
            UpdateTexts();
        }

        UpdateBar();
    }

    void UpdateTexts()
    {
        if (nameText != null && currentBoss != null)
        {
            string prefix = currentBoss.isBoss ? "JEFE: " : (currentBoss.isElite ? "ÉLITE: " : "");
            nameText.text = prefix + currentBoss.gameObject.name;
        }
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

        // 0.6-fix: panel en el espacio libre entre el borde superior y el grid (cuadro rojo)
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.25f, 0.94f);
        prt.anchorMax = new Vector2(0.75f, 0.94f);
        prt.pivot = new Vector2(0.5f, 1f);
        prt.sizeDelta = new Vector2(0, 60); // 60px de alto, cuelga desde y=0.94 hacia abajo
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = SpriteFactory.Square();
        panelImg.color = new Color(0.05f, 0.03f, 0.02f, 0.95f);

        // Borde dorado
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-3, -3);
        brt.offsetMax = new Vector2(3, 3);
        Image brdImg = border.AddComponent<Image>();
        brdImg.sprite = SpriteFactory.Square();
        brdImg.color = new Color(0.8f, 0.7f, 0.2f, 1f);

        // Nombre (fila superior del panel)
        nameText = MakeText(panel.transform, bossName, -1, 22, 16, new Color(1f, 0.9f, 0.4f));

        // Barra de relleno (fila media del panel)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(panel.transform, false);
        RectTransform frt = fillObj.AddComponent<RectTransform>();
        frt.anchorMin = new Vector2(0.02f, 1f);
        frt.anchorMax = new Vector2(0.98f, 1f);
        frt.pivot = new Vector2(0.5f, 1f);
        frt.anchoredPosition = new Vector2(0, -25);
        frt.sizeDelta = new Vector2(0, 18);
        fillBar = fillObj.AddComponent<Image>();
        fillBar.sprite = SpriteFactory.Square();
        fillBar.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        fillBar.type = Image.Type.Filled;
        fillBar.fillMethod = Image.FillMethod.Horizontal;

        // Subtítulo (fila inferior del panel)
        if (!string.IsNullOrEmpty(subtitle))
        {
            subtitleText = MakeText(panel.transform, subtitle, -44, 15, 10, new Color(0.75f, 0.75f, 0.75f));
        }

        UpdateBar();
    }
    
    void UpdateBar()
    {
        if (fillBar == null || currentBoss == null) return;
        float t = Mathf.Clamp01((float)currentBoss.currentHealth / currentBoss.maxHealth);
        fillBar.fillAmount = t;
    }
    
    // topY = distancia desde el borde superior del panel; height = alto de la caja de texto
    Text MakeText(Transform parent, string content, float topY, float height, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, topY);
        rt.sizeDelta = new Vector2(0, height);
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        // 0.6-fix: sombra negra para que el texto se lea sobre cualquier fondo
        Shadow sh = go.AddComponent<Shadow>();
        sh.effectColor = Color.black;
        sh.effectDistance = new Vector2(1, -1);
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