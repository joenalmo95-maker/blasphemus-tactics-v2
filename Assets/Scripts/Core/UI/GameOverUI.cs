using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    private GameObject root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Show(bool victory)
    {
        if (root != null) Destroy(root);
        if (victory) SaveSystem.Save();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        root = new GameObject("GameOverCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
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

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(root.transform, false);
        RectTransform trt = titleObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 140);
        trt.sizeDelta = new Vector2(700, 80);
        Text t = titleObj.AddComponent<Text>();
        t.text = victory ? "VICTORIA" : "DERROTA";
        t.font = GetFont();
        t.fontSize = 48;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = victory ? Color.yellow : Color.red;

        float buttonY = -40;

        if (victory)
        {
            GameObject sumObj = new GameObject("Summary");
            sumObj.transform.SetParent(root.transform, false);
            RectTransform srt = sumObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0, 10);
            srt.sizeDelta = new Vector2(800, 180);
            Text st = sumObj.AddComponent<Text>();
            st.text = "BOTÍN DE LA MISIÓN\n" + LootSystem.GetCombatSummary();
            st.font = GetFont();
            st.fontSize = 16;
            st.alignment = TextAnchor.UpperCenter;
            st.color = Color.white;
            buttonY = -160;
        }

        GameObject btnObj = new GameObject("Btn");
        btnObj.transform.SetParent(root.transform, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, buttonY);
        rt.sizeDelta = new Vector2(280, 50);
        Image img = btnObj.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        Button btn = btnObj.AddComponent<Button>();

        GameObject lblObj = new GameObject("Label");
        lblObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = lblObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        Text lt = lblObj.AddComponent<Text>();
        lt.text = "Reiniciar";
        lt.font = GetFont();
        lt.fontSize = 20;
        lt.alignment = TextAnchor.MiddleCenter;
        lt.color = Color.white;

        btn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}