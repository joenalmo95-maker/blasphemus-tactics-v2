using UnityEngine;
using UnityEngine.UI;

public class WorldPlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Text promptText;
    private bool isPaused = false;
    private GameObject pauseCanvas;

    void Awake()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldPromptCanvas", 44);
        promptText = UIFactory.CreateText(canvas.transform, "WorldPrompt", "", 16, TextAnchor.MiddleCenter,
            new Color(1f, 0.9f, 0.4f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 8), new Vector2(900, 24));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
                ShowPauseMenu();
            else
                HidePauseMenu();
        }

        if (isPaused) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;

        if (dir != Vector3.zero)
        {
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            Vector2Int targetCell = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            if (TerrainMap.IsWalkable(targetCell))
                transform.position = newPos;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        WorldBootstrap.LastKnownPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y));

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        if (Mathf.Abs(myCell.x - WorldBootstrap.CityPortal.x) <= 1 &&
            Mathf.Abs(myCell.y - WorldBootstrap.CityPortal.y) <= 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SaveSystem.Save();
                PlayerPrefs.SetInt("LastWorldX", myCell.x);
                PlayerPrefs.SetInt("LastWorldY", myCell.y);
                GameFlow.EnterCity();
            }
            promptText.text = "Pulsa E para entrar al Bastión de San Veritas";
        }
        else
        {
            promptText.text = "VALLE DE LA LUZ ETERNA (WASD mover · M mapa · I inventario · ESC pausa)";
        }
    }

    void ShowPauseMenu()
    {
        pauseCanvas = new GameObject("PauseCanvas");
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

        // Panel central (estilo ciudad: marrón cálido, sin borde)
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
        CreateText(panel.transform, "VALLE DE LA LUZ ETERNA", 0, 110, 24, new Color(1f, 0.9f, 0.4f));
        CreateText(panel.transform, "— Menú de Pausa —", 0, 80, 14, new Color(0.7f, 0.7f, 0.7f));

        // Botón CONTINUAR
        CreateButton(panel.transform, "CONTINUAR", 0, 30, 300, 50, Color.green, () =>
        {
            isPaused = false;
            HidePauseMenu();
        });

        // Botón GUARDAR PARTIDA
        CreateButton(panel.transform, "GUARDAR PARTIDA", 0, -30, 300, 50, Color.cyan, () =>
        {
            SaveSystem.Save();
            Debug.Log("[WorldPause] Partida guardada correctamente.");
        });

        // Botón SALIR AL MENÚ PRINCIPAL
        CreateButton(panel.transform, "SALIR AL MENÚ", 0, -90, 300, 50, new Color(0.9f, 0.3f, 0.3f), () =>
        {
            Debug.Log("[WorldPause] Guardado y regresando al menú principal.");
            HidePauseMenu();
            GameFlow.ReturnToMainMenu();
        });

        // Info inferior
        CreateText(panel.transform, "ESC para cerrar este menú", 0, -140, 12, new Color(0.5f, 0.5f, 0.5f));
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

    void CreateText(Transform parent, string content, float x, float y, int size, Color color)
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

    void CreateButton(Transform parent, string label, float x, float y, float w, float h, Color textColor,
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

    Font GetFont()
    {
        return UIFactory.GetFont();
    }

    void OnApplicationQuit()
    {
        SaveSystem.Save();
        Debug.Log("[WorldPlayer] Autoguardado al salir.");
    }
}