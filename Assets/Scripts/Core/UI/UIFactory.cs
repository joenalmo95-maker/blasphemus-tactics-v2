using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class UIFactory
{
    public static GameObject CreateCanvas(string name, int sortOrder = 50)
    {
        // Regla de trabajo: Asegurar EventSystem
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        GameObject root = new GameObject(name);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        root.AddComponent<GraphicRaycaster>();
        return root;
    }

    public static RectTransform CreatePanel(Transform parent, string name, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null, Vector2? pos = null, Vector2? size = null, Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        
        rt.anchorMin = anchorMin ?? new Vector2(0.5f, 0.5f);
        rt.anchorMax = anchorMax ?? new Vector2(0.5f, 0.5f);
        rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos ?? Vector2.zero;
        rt.sizeDelta = size ?? new Vector2(100, 100);

        if (color.HasValue)
        {
            Image img = go.AddComponent<Image>();
            img.sprite = SpriteFactory.Square();
            img.color = color.Value;
        }
        return rt;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize = 16, TextAnchor align = TextAnchor.UpperLeft, Color? color = null, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null, Vector2? pos = null, Vector2? size = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        
        rt.anchorMin = anchorMin ?? new Vector2(0, 1);
        rt.anchorMax = anchorMax ?? new Vector2(0, 1);
        rt.pivot = pivot ?? new Vector2(0, 1);
        rt.anchoredPosition = pos ?? Vector2.zero;
        rt.sizeDelta = size ?? new Vector2(200, 50);

        Text t = go.AddComponent<Text>();
        t.text = content;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color ?? Color.white;
        return t;
    }

    public struct BarUI
    {
        public GameObject root;
        public RectTransform fill;
        public Text text;
    }

    public static BarUI CreateBar(Transform parent, string name, Vector2 pos, Vector2 size, Color bgColor, Color fillColor)
    {
        BarUI bar = new BarUI();
        
        RectTransform bgRt = CreatePanel(parent, name + "_BG", 
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), pos, size, bgColor);
        bar.root = bgRt.gameObject;

        RectTransform fillRt = CreatePanel(bgRt, name + "_Fill", 
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(0, size.y), fillColor);
        bar.fill = fillRt;

        bar.text = CreateText(bgRt, name + "_Text", "", 14, TextAnchor.MiddleCenter, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            
        RectTransform tRt = bar.text.GetComponent<RectTransform>();
        tRt.offsetMin = Vector2.zero;
        tRt.offsetMax = Vector2.zero;

        return bar;
    }
}