using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoStart()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        if (Object.FindAnyObjectByType<MainMenuBootstrap>() != null) return;
        new GameObject("MainMenuBootstrap").AddComponent<MainMenuBootstrap>();
    }

    void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.03f, 0.02f);
        }

        BuildMenu();
    }

    void BuildMenu()
    {
        GameObject canvas = UIFactory.CreateCanvas("MainMenuCanvas", 100);

        // Fondo oscuro con gradiente simulado
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = SpriteFactory.Square();
        bgImg.color = new Color(0.08f, 0.06f, 0.04f);

        // Panel central
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(500, 500);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = SpriteFactory.Square();
        panelImg.color = new Color(0.12f, 0.10f, 0.08f, 0.95f);

        // Borde del panel
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform brdrt = border.AddComponent<RectTransform>();
        brdrt.anchorMin = Vector2.zero;
        brdrt.anchorMax = Vector2.one;
        brdrt.offsetMin = new Vector2(-6, -6);
        brdrt.offsetMax = new Vector2(6, 6);
        Image brdimg = border.AddComponent<Image>();
        brdimg.sprite = SpriteFactory.Square();
        brdimg.color = new Color(0.6f, 0.5f, 0.2f, 0.8f);

        // Título principal
        CreateText(panel.transform, "LA LITURGIA DEL CIELO", 0, 180, 36, new Color(1f, 0.9f, 0.4f));
        CreateText(panel.transform, "Blasphemus Tactics", 0, 140, 20, new Color(0.7f, 0.7f, 0.7f));

        // Separador
        GameObject sep = new GameObject("Separator");
        sep.transform.SetParent(panel.transform, false);
        RectTransform seprt = sep.AddComponent<RectTransform>();
        seprt.anchorMin = new Vector2(0.5f, 0.5f);
        seprt.anchorMax = new Vector2(0.5f, 0.5f);
        seprt.sizeDelta = new Vector2(400, 2);
        seprt.anchoredPosition = new Vector2(0, 110);
        Image sepimg = sep.AddComponent<Image>();
        sepimg.sprite = SpriteFactory.Square();
        sepimg.color = new Color(0.6f, 0.5f, 0.2f, 0.8f);

        // Verificar si hay save
        bool hasSave = SaveSystem.HasSave();

        // Botón CONTINUAR (solo si hay save)
        Color continueColor = hasSave ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.4f, 0.4f, 0.4f);
        CreateButton(panel.transform, "CONTINUAR", 0, 60, 350, 60, continueColor, () =>
        {
            if (SaveSystem.HasSave())
            {
                Debug.Log("[MainMenu] Cargando partida guardada...");
                SceneManager.LoadScene("WorldMap");
            }
        }, hasSave);

        // Botón NUEVA PARTIDA
        CreateButton(panel.transform, "NUEVA PARTIDA", 0, -10, 350, 60, new Color(0.3f, 0.7f, 0.9f), () =>
        {
            // Reset completo de TODOS los managers en memoria
            GameReset.ResetAll();
            
            if (SaveSystem.HasSave())
            {
                Debug.Log("[MainMenu] Eliminando save anterior para nueva partida...");
                SaveSystem.DeleteSave();
            }
            SceneManager.LoadScene("WorldMap");
        });

        // Botón SALIR
        CreateButton(panel.transform, "SALIR", 0, -80, 350, 60, new Color(0.9f, 0.3f, 0.3f), () =>
        {
            Application.Quit();
        });

        // Info inferior
        CreateText(panel.transform, "Valerius — El Inquisidor", 0, -150, 14, new Color(0.6f, 0.6f, 0.6f));
        CreateText(panel.transform, "v0.3 — Región I: Valle de la Luz Eterna", 0, -170, 12, new Color(0.4f, 0.4f, 0.4f));
    }

    void CreateText(Transform parent, string content, float x, float y, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(480, 50);
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
        UnityEngine.Events.UnityAction onClick, bool interactable = true)
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
        btn.interactable = interactable;

        // Borde del botón
        GameObject border = new GameObject("Border");
        border.transform.SetParent(go.transform, false);
        RectTransform brdrt = border.AddComponent<RectTransform>();
        brdrt.anchorMin = Vector2.zero;
        brdrt.anchorMax = Vector2.one;
        brdrt.offsetMin = new Vector2(-3, -3);
        brdrt.offsetMax = new Vector2(3, 3);
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
        t.fontSize = 20;
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