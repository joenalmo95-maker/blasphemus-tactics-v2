using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterCreationUI : MonoBehaviour
{
    public List<ClassData> availableClasses = new List<ClassData>();
    public bool showContinue = false;
    public System.Action onFinished;

    private GameObject root;
    private int selectedClassIndex = 0;

    void Awake()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void Update()
    {
        if (root != null && root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Build()
    {
        if (root != null) Destroy(root);

        root = new GameObject("CharacterCreationCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
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
        bimg.color = new Color(0.02f, 0.02f, 0.03f, 0.95f);

        MakeText(root.transform, "LA LITURGIA DEL CIELO", 0, 200, 32, Color.white);
        MakeText(root.transform, "VALERIUS — Inquisidor del Bastión de San Veritas", 0, 150, 20, Color.yellow);
        MakeText(root.transform, "Un único protagonista. Tu espada. Tu fe. Tu verdad.", 0, 100, 16, new Color(0.8f, 0.8f, 0.8f));

        MakeButton(root.transform, "COMENZAR CRUZADA", 0, -50, 400, 60, Color.green, () =>
        {
            if (CharacterData.Instance != null)
            {
                if (CharacterData.Instance.classData == null)
                {
                    CharacterData.Instance.classData = ScriptableObject.CreateInstance<ClassData>();
                    CharacterData.Instance.classData.className = "Inquisidor";
                    CharacterData.Instance.classData.role = ClassRole.DPS;
                }
            }
            Close();
            if (onFinished != null) onFinished();
        });

        if (showContinue)
        {
            MakeButton(root.transform, "CONTINUAR PARTIDA", 0, -130, 400, 60, Color.cyan, () =>
            {
                SaveSystem.Load();
                Close();
                if (onFinished != null) onFinished();
            });
        }

        MakeText(root.transform, "v0.3 — Valle de la Luz Eterna (Región I)", 0, -250, 12, new Color(0.5f, 0.5f, 0.5f));
    }

    void Close()
    {
        if (root != null) Destroy(root);
        root = null;
    }

    void MakeText(Transform parent, string content, float x, float y, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(900, 40);
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
        t.fontSize = 18;
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