using UnityEngine;
using UnityEngine.UI;

public class CityPlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Text promptText;
    private bool isPaused = false;
    private GameObject pauseCanvas;

    void Awake() { BuildPrompt(); }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("CityPromptCanvas", 40);
        promptText = UIFactory.CreateText(canvas.transform, "Prompt", "", 18, TextAnchor.MiddleCenter, Color.yellow,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 120), new Vector2(700, 40));
    }

    void Update()
    {
        // FIX: Sistema de pausa con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseCanvas != null)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
            return;
        }

        if (isPaused) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;

        if (dir != Vector3.zero)
        {
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            Vector2Int targetCell = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            if (targetCell.x >= 0 && targetCell.y >= 0 &&
                targetCell.x < CityBootstrap.CityWidth && targetCell.y < CityBootstrap.CityHeight &&
                TerrainMap.IsWalkable(targetCell))
                transform.position = newPos;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, CityBootstrap.CityWidth - 1),
            Mathf.Clamp(transform.position.y, 0, CityBootstrap.CityHeight - 1),
            0);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        UpdatePrompt();
    }

    void ShowPauseMenu()
    {
        isPaused = true;

        pauseCanvas = new GameObject("CityPauseCanvas");
        Canvas canvas = pauseCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        pauseCanvas.AddComponent<GraphicRaycaster>();

        // Fondo oscuro semitransparente
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(pauseCanvas.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = SpriteFactory.Square();
        bgImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Panel central
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(pauseCanvas.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(400, 320);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = SpriteFactory.Square();
        panelImg.color = new Color(0.12f, 0.10f, 0.08f, 0.95f);

        // Título
        MakeText(panel.transform, "BASTIÓN DE SAN VERITAS", 0, 110, 24, new Color(1f, 0.9f, 0.4f));
        MakeText(panel.transform, "— Menú de Pausa —", 0, 80, 14, new Color(0.7f, 0.7f, 0.7f));

        // Botón CONTINUAR
        MakeButton(panel.transform, "CONTINUAR", 0, 30, 300, 50, Color.green, () =>
        {
            HidePauseMenu();
        });

        // Botón GUARDAR PARTIDA
        MakeButton(panel.transform, "GUARDAR PARTIDA", 0, -30, 300, 50, Color.cyan, () =>
        {
            SaveSystem.Save();
            Debug.Log("[CityPlayer] Partida guardada correctamente.");
        });

        // Botón SALIR AL MUNDO
        MakeButton(panel.transform, "SALIR AL MUNDO", 0, -90, 300, 50, new Color(1f, 0.6f, 0.2f), () =>
        {
            SaveSystem.Save();
            Debug.Log("[CityPlayer] Autoguardado antes de salir de la ciudad.");
            HidePauseMenu();
            GameFlow.ReturnToWorld();
        });

        // Info inferior
        MakeText(panel.transform, "ESC para cerrar este menú", 0, -140, 12, new Color(0.5f, 0.5f, 0.5f));
    }

    void HidePauseMenu()
    {
        isPaused = false;
        if (pauseCanvas != null)
        {
            Destroy(pauseCanvas);
            pauseCanvas = null;
        }
    }

    // FIX: Autoguardar al cerrar Unity desde la ciudad
    void OnApplicationQuit()
    {
        SaveSystem.Save();
        Debug.Log("[CityPlayer] Autoguardado al cerrar el juego.");
    }

    void UpdatePrompt()
    {
        if (isPaused) return;

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (Mathf.Abs(myCell.x - CityBootstrap.ExitPortal.x) <= 1 && Mathf.Abs(myCell.y - CityBootstrap.ExitPortal.y) <= 1)
        {
            promptText.text = "Pulsa E para salir al mundo";
        }
        else
        {
            promptText.text = "BASTIÓN DE SAN VERITAS (WASD mover · I inventario · ESC pausa)";
        }
    }

    void MakeText(Transform parent, string content, float x, float y, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(380, 40);
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
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
    
}