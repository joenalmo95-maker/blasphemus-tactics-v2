using UnityEngine;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    private Text levelText;
    private Text goldText;
    private RectTransform xpFill;
    private float xpBarWidth = 200f;

    void Awake()
    {
        Build();
    }

    void Build()
    {
        GameObject root = new GameObject("HUDCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        levelText = MakeText(root.transform, "Nv 0", new Vector2(20, -10), new Vector2(90, 30));
        goldText = MakeText(root.transform, "Oro: 0", new Vector2(340, -10), new Vector2(160, 30));

        GameObject bgObj = new GameObject("XP_BG");
        bgObj.transform.SetParent(root.transform, false);
        RectTransform brt = bgObj.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 1);
        brt.anchorMax = new Vector2(0, 1);
        brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = new Vector2(110, -18);
        brt.sizeDelta = new Vector2(xpBarWidth, 12);
        Image bimg = bgObj.AddComponent<Image>();
        bimg.sprite = SpriteFactory.Square();
        bimg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject fillObj = new GameObject("XP_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        xpFill = fillObj.AddComponent<RectTransform>();
        xpFill.anchorMin = new Vector2(0, 0);
        xpFill.anchorMax = new Vector2(0, 1);
        xpFill.pivot = new Vector2(0, 0.5f);
        xpFill.anchoredPosition = Vector2.zero;
        xpFill.sizeDelta = new Vector2(0, 12);
        Image fimg = fillObj.AddComponent<Image>();
        fimg.sprite = SpriteFactory.Square();
        fimg.color = Color.cyan;
    }

    void Update()
    {
        if (CharacterData.Instance == null) return;
        CharacterData cd = CharacterData.Instance;

        if (levelText != null) levelText.text = "Nv " + cd.level;
        if (goldText != null) goldText.text = "Oro: " + cd.gold;
        if (xpFill != null)
        {
            float t = Mathf.Clamp01((float)cd.xp / cd.XpToNextLevel());
            xpFill.sizeDelta = new Vector2(xpBarWidth * t, 12);
        }
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text t = go.AddComponent<Text>();
        t.text = content;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = 16;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = Color.white;
        return t;
    }
}