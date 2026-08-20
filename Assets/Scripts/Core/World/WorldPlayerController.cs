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
        // Sistema de pausa con ESC (no usa Time.timeScale para que la UI funcione)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                ShowPauseMenu();
            }
            else
            {
                HidePauseMenu();
            }
        }

        // No procesar movimiento si está pausado
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

        // Límites del mundo expandido 120x80
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        // Actualizar posición conocida
        WorldBootstrap.LastKnownPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y));

        // Cámara sigue al jugador
        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        // Portal a la ciudad
        if (Mathf.Abs(myCell.x - WorldBootstrap.CityPortal.x) <= 1 &&
            Mathf.Abs(myCell.y - WorldBootstrap.CityPortal.y) <= 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Autoguardado antes de entrar a la ciudad
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
        // Crear canvas de pausa con sorting order alto
        pauseCanvas = new GameObject("PauseCanvas");
        Canvas canvas = pauseCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        pauseCanvas.AddComponent<GraphicRaycaster>();

        // Fondo oscuro semi-transparente
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(pauseCanvas.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        Image bimg = bg.AddComponent<Image>();
        bimg.sprite = SpriteFactory.Square();
        bimg.color = new Color(0f, 0f, 0f, 0.85f);

        // Panel central con borde
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(pauseCanvas.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(400, 350);
        prt.anchoredPosition = Vector2.zero;
        Image pimg = panel.AddComponent<Image>();
        pimg.sprite = SpriteFactory.Square();
        pimg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // Borde del panel
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform brdrt = border.AddComponent<RectTransform>();
        brdrt.anchorMin = Vector2.zero;
        brdrt.anchorMax = Vector2.one;
        brdrt.offsetMin = new Vector2(-4, -4);
        brdrt.offsetMax = new Vector2(4, 4);
        Image brdimg = border.AddComponent<Image>();
        brdimg.sprite = SpriteFactory.Square();
        brdimg.color = new Color(0.6f, 0.5f, 0.2f, 1f);

        // Título
        CreateText(panel.transform, "⚔ PAUSA ⚔", 0, 130, 28, new Color(0.9f, 0.8f, 0.3f));
        CreateText(panel.transform, "Valle de la Luz Eterna", 0, 95, 16, new Color(0.7f, 0.7f, 0.7f));

        // Separador
        GameObject sep = new GameObject("Separator");
        sep.transform.SetParent(panel.transform, false);
        RectTransform seprt = sep.AddComponent<RectTransform>();
        seprt.anchorMin = new Vector2(0.5f, 0.5f);
        seprt.anchorMax = new Vector2(0.5f, 0.5f);
        seprt.sizeDelta = new Vector2(350, 2);
        seprt.anchoredPosition = new Vector2(0, 70);
        Image sepimg = sep.AddComponent<Image>();
        sepimg.sprite = SpriteFactory.Square();
        sepimg.color = new Color(0.6f, 0.5f, 0.2f, 0.8f);

        // Botón Guardar Partida
        CreateButton(panel.transform, "💾 GUARDAR PARTIDA", 0, 30, 300, 50, new Color(0.2f, 0.8f, 0.3f), () =>
        {
            SaveSystem.Save();
            Debug.Log("[Pause] Partida guardada correctamente.");
            // Feedback visual
            CreateText(panel.transform, "✓ Guardado", 0, -10, 14, Color.green);
        });

        // Botón Continuar
        CreateButton(panel.transform, "▶ CONTINUAR", 0, -40, 300, 50, new Color(0.3f, 0.7f, 0.9f), () =>
        {
            isPaused = false;
            HidePauseMenu();
        });

        // Botón Salir al Menú
        CreateButton(panel.transform, "✕ SALIR AL MENÚ", 0, -110, 300, 50, new Color(0.9f, 0.3f, 0.3f), () =>
        {
            SaveSystem.Save();
            Application.Quit();
        });

        // Texto de ayuda
        CreateText(panel.transform, "ESC para cerrar", 0, -160, 12, new Color(0.5f, 0.5f, 0.5f));
    }

    void HidePauseMenu()
    {
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
        rt.sizeDelta = new Vector2(400, 40);
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
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        // Borde del botón
        GameObject border = new GameObject("Border");
        border.transform.SetParent(go.transform, false);
        RectTransform brdrt = border.AddComponent<RectTransform>();
        brdrt.anchorMin = Vector2.zero;
        brdrt.anchorMax = Vector2.one;
        brdrt.offsetMin = new Vector2(-2, -2);
        brdrt.offsetMax = new Vector2(2, 2);
        Image brdimg = border.AddComponent<Image>();
        brdimg.sprite = SpriteFactory.Square();
        brdimg.color = new Color(textColor.r, textColor.g, textColor.b, 0.6f);

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
        t.fontSize = 18;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = textColor;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    // Autoguardado al cerrar la aplicación
    void OnApplicationQuit()
    {
        SaveSystem.Save();
        Debug.Log("[WorldPlayer] Autoguardado al salir.");
    }
}