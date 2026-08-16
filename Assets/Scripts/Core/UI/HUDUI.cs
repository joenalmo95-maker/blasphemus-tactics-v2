using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HUDUI : MonoBehaviour
{
    // Player HUD references
    private Image portraitImage;
    private Text levelText;
    private Text hpText;
    private RectTransform hpFill;
    private Text apText;
    private RectTransform apFill;
    private Transform buffContainer;
    private Transform debuffContainer;

    // Global HUD references
    private Text goldText;
    private RectTransform xpFill;
    private float xpBarWidth = 200f;

    // Boss/Elite HUD references
    private GameObject bossBarRoot;
    private Text bossNameText;
    private RectTransform bossHpFill;
    private Text bossHpText;

    private Unit playerUnit;
    private List<GameObject> activeBuffIcons = new List<GameObject>();
    private List<GameObject> activeDebuffIcons = new List<GameObject>();

    void Awake()
    {
        // EventSystem guard (Regla de trabajo)
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        Build();
    }

    void Build()
    {
        GameObject root = new GameObject("HUDCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // --- TOP CENTER: Boss / Elite Bar ---
        bossBarRoot = new GameObject("BossBarRoot");
        bossBarRoot.transform.SetParent(root.transform, false);
        RectTransform bRootRt = bossBarRoot.AddComponent<RectTransform>();
        bRootRt.anchorMin = new Vector2(0.5f, 1f);
        bRootRt.anchorMax = new Vector2(0.5f, 1f);
        bRootRt.pivot = new Vector2(0.5f, 1f);
        bRootRt.anchoredPosition = new Vector2(0, -20);
        bRootRt.sizeDelta = new Vector2(600, 60);
        
        bossNameText = MakeText(bossBarRoot.transform, "", new Vector2(0, 0), new Vector2(600, 30), TextAnchor.UpperCenter, 24, Color.yellow);
        RectTransform bNameRt = bossNameText.GetComponent<RectTransform>();
        bNameRt.anchorMin = new Vector2(0, 0.5f);
        bNameRt.anchorMax = new Vector2(1, 1f);
        bNameRt.offsetMin = Vector2.zero;
        bNameRt.offsetMax = Vector2.zero;
        
        GameObject bossBg = new GameObject("BossHP_BG");
        bossBg.transform.SetParent(bossBarRoot.transform, false);
        RectTransform bgRt = bossBg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0);
        bgRt.anchorMax = new Vector2(1, 0.4f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bossBg.AddComponent<Image>();
        bgImg.sprite = SpriteFactory.Square();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject bossFill = new GameObject("BossHP_Fill");
        bossFill.transform.SetParent(bossBg.transform, false);
        bossHpFill = bossFill.AddComponent<RectTransform>();
        bossHpFill.anchorMin = new Vector2(0, 0);
        bossHpFill.anchorMax = new Vector2(1, 1);
        bossHpFill.offsetMin = Vector2.zero;
        bossHpFill.offsetMax = Vector2.zero;
        Image bFillImg = bossFill.AddComponent<Image>();
        bFillImg.sprite = SpriteFactory.Square();
        bFillImg.color = new Color(0.8f, 0.1f, 0.1f);

        bossHpText = MakeText(bossBg.transform, "", Vector2.zero, new Vector2(-1, -1), TextAnchor.MiddleCenter, 16, Color.white);
        RectTransform bHpRt = bossHpText.GetComponent<RectTransform>();
        bHpRt.anchorMin = Vector2.zero;
        bHpRt.anchorMax = Vector2.one;
        bHpRt.offsetMin = Vector2.zero;
        bHpRt.offsetMax = Vector2.zero;

        bossBarRoot.SetActive(false);

        // --- BOTTOM LEFT: Player HUD ---
        GameObject playerHud = new GameObject("PlayerHUD");
        playerHud.transform.SetParent(root.transform, false);
        RectTransform pHudRt = playerHud.AddComponent<RectTransform>();
        pHudRt.anchorMin = new Vector2(0, 0);
        pHudRt.anchorMax = new Vector2(0, 0);
        pHudRt.pivot = new Vector2(0, 0);
        pHudRt.anchoredPosition = new Vector2(20, 40); 
        pHudRt.sizeDelta = new Vector2(300, 150);

        // Portrait
        GameObject portrait = new GameObject("Portrait");
        portrait.transform.SetParent(playerHud.transform, false);
        RectTransform pRt = portrait.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0, 0.5f);
        pRt.anchorMax = new Vector2(0, 0.5f);
        pRt.pivot = new Vector2(0, 0.5f);
        pRt.anchoredPosition = new Vector2(0, 20);
        pRt.sizeDelta = new Vector2(80, 80);
        portraitImage = portrait.AddComponent<Image>();
        portraitImage.sprite = SpriteFactory.Square(); 
        portraitImage.color = Color.gray;

        // Level Text
        levelText = MakeText(playerHud.transform, "Nv 1", new Vector2(0, -30), new Vector2(80, 30), TextAnchor.UpperCenter, 18, Color.white);
        RectTransform lvlRt = levelText.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0, 0.5f);
        lvlRt.anchorMax = new Vector2(0, 0.5f);
        lvlRt.pivot = new Vector2(0.5f, 1);
        lvlRt.anchoredPosition = new Vector2(40, -25);

        // Bars
        CreateBar(playerHud.transform, "HP", new Vector2(90, 60), new Vector2(200, 25), new Color(0.6f, 0f, 0f), new Color(0.9f, 0.1f, 0.1f), out hpText, out hpFill);
        CreateBar(playerHud.transform, "AP", new Vector2(90, 30), new Vector2(200, 25), new Color(0f, 0.3f, 0.6f), new Color(0.1f, 0.6f, 0.9f), out apText, out apFill);

        // Status Containers
        GameObject statusContainer = new GameObject("StatusIcons");
        statusContainer.transform.SetParent(playerHud.transform, false);
        RectTransform sRt = statusContainer.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 1);
        sRt.anchorMax = new Vector2(1, 1);
        sRt.pivot = new Vector2(0, 1);
        sRt.anchoredPosition = new Vector2(0, 30);
        sRt.sizeDelta = new Vector2(300, 30);
        
        HorizontalLayoutGroup hlg = statusContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        buffContainer = new GameObject("Buffs").transform;
        buffContainer.SetParent(statusContainer.transform, false);
        HorizontalLayoutGroup bhlg = buffContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        bhlg.spacing = 2; bhlg.childForceExpandWidth = false; bhlg.childForceExpandHeight = true;
        buffContainer.gameObject.AddComponent<LayoutElement>().preferredWidth = 140;

        debuffContainer = new GameObject("Debuffs").transform;
        debuffContainer.SetParent(statusContainer.transform, false);
        HorizontalLayoutGroup dhlg = debuffContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        dhlg.spacing = 2; dhlg.childForceExpandWidth = false; dhlg.childForceExpandHeight = true;
        debuffContainer.gameObject.AddComponent<LayoutElement>().preferredWidth = 140;

        // --- TOP LEFT: Gold & XP ---
        goldText = MakeText(root.transform, "Oro: 0", new Vector2(20, -20), new Vector2(150, 30), TextAnchor.UpperLeft, 18, Color.yellow);
        
        GameObject xpBg = new GameObject("XP_BG");
        xpBg.transform.SetParent(root.transform, false);
        RectTransform xBgRt = xpBg.AddComponent<RectTransform>();
        xBgRt.anchorMin = new Vector2(0, 1);
        xBgRt.anchorMax = new Vector2(0, 1);
        xBgRt.pivot = new Vector2(0, 1);
        xBgRt.anchoredPosition = new Vector2(180, -25);
        xBgRt.sizeDelta = new Vector2(xpBarWidth, 16);
        Image xBgImg = xpBg.AddComponent<Image>();
        xBgImg.sprite = SpriteFactory.Square();
        xBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject xpFillObj = new GameObject("XP_Fill");
        xpFillObj.transform.SetParent(xpBg.transform, false);
        xpFill = xpFillObj.AddComponent<RectTransform>();
        xpFill.anchorMin = new Vector2(0, 0);
        xpFill.anchorMax = new Vector2(0, 1);
        xpFill.pivot = new Vector2(0, 0.5f);
        xpFill.anchoredPosition = Vector2.zero;
        xpFill.sizeDelta = new Vector2(0, 16);
        Image xFillImg = xpFillObj.AddComponent<Image>();
        xFillImg.sprite = SpriteFactory.Square();
        xFillImg.color = Color.cyan;

        MakeHint(root.transform, "1/2: habilidades 3: utilidad 4-7: consumibles I: inventario B: tienda E: fin de turno");
    }

    void CreateBar(Transform parent, string label, Vector2 pos, Vector2 size, Color bgColor, Color fillColor, out Text textOut, out RectTransform fillOut)
    {
        GameObject bg = new GameObject(label + "_BG");
        bg.transform.SetParent(parent, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 1);
        bgRt.anchorMax = new Vector2(0, 1);
        bgRt.pivot = new Vector2(0, 1);
        bgRt.anchoredPosition = pos;
        bgRt.sizeDelta = size;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = SpriteFactory.Square();
        bgImg.color = bgColor;

        GameObject fill = new GameObject(label + "_Fill");
        fill.transform.SetParent(bg.transform, false);
        fillOut = fill.AddComponent<RectTransform>();
        fillOut.anchorMin = new Vector2(0, 0);
        fillOut.anchorMax = new Vector2(0, 1);
        fillOut.pivot = new Vector2(0, 0.5f);
        fillOut.anchoredPosition = Vector2.zero;
        fillOut.sizeDelta = new Vector2(0, size.y);
        Image fImg = fill.AddComponent<Image>();
        fImg.sprite = SpriteFactory.Square();
        fImg.color = fillColor;

        textOut = MakeText(bg.transform, "", Vector2.zero, new Vector2(-1, -1), TextAnchor.MiddleCenter, 14, Color.white);
        RectTransform tRt = textOut.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = Vector2.zero;
        tRt.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (playerUnit == null)
        {
            Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units)
            {
                if (!u.isEnemy) { playerUnit = u; break; }
            }
        }

        if (CharacterData.Instance != null)
        {
            CharacterData cd = CharacterData.Instance;
            if (goldText != null) goldText.text = "Oro: " + cd.gold;
            if (xpFill != null)
            {
                float t = Mathf.Clamp01((float)cd.xp / cd.XpToNextLevel());
                xpFill.sizeDelta = new Vector2(xpBarWidth * t, 16);
            }
            if (levelText != null) levelText.text = "Nv " + cd.level;
        }

        if (playerUnit != null)
        {
            float hpRatio = Mathf.Clamp01((float)playerUnit.currentHealth / playerUnit.maxHealth);
            if (hpFill != null) hpFill.sizeDelta = new Vector2(200 * hpRatio, 25);
            if (hpText != null) hpText.text = playerUnit.currentHealth + " / " + playerUnit.maxHealth;

            float apRatio = Mathf.Clamp01((float)playerUnit.currentAP / playerUnit.maxAP);
            if (apFill != null) apFill.sizeDelta = new Vector2(200 * apRatio, 25);
            if (apText != null) apText.text = playerUnit.currentAP + " / " + playerUnit.maxAP;

            UpdateStatusIcons();
        }

        UpdateBossBar();
    }

    void UpdateStatusIcons()
    {
        int buffCount = (playerUnit.buffTurns > 0) ? 1 : 0; 
        int debuffCount = (playerUnit.debuffTurns > 0) ? 1 : 0;

        if (activeBuffIcons.Count != buffCount)
        {
            foreach (var g in activeBuffIcons) Destroy(g);
            activeBuffIcons.Clear();
            if (buffCount > 0)
            {
                GameObject icon = CreateStatusIcon(buffContainer, "BUFF\n+DMG/DEF", Color.blue, playerUnit.buffTurns);
                activeBuffIcons.Add(icon);
            }
        }
        else if (buffCount > 0 && activeBuffIcons.Count > 0)
        {
            UpdateStatusIconText(activeBuffIcons[0], "BUFF\n" + playerUnit.buffTurns + "t", Color.blue);
        }

        if (activeDebuffIcons.Count != debuffCount)
        {
            foreach (var g in activeDebuffIcons) Destroy(g);
            activeDebuffIcons.Clear();
            if (debuffCount > 0)
            {
                GameObject icon = CreateStatusIcon(debuffContainer, "MALDICIÓN\n-PREC", Color.magenta, playerUnit.debuffTurns);
                activeDebuffIcons.Add(icon);
            }
        }
        else if (debuffCount > 0 && activeDebuffIcons.Count > 0)
        {
            UpdateStatusIconText(activeDebuffIcons[0], "MALDICIÓN\n" + playerUnit.debuffTurns + "t", Color.magenta);
        }
    }

    GameObject CreateStatusIcon(Transform parent, string label, Color color, int turns)
    {
        GameObject go = new GameObject("StatusIcon");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 30);
        Image img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = color;

        Text t = go.AddComponent<Text>();
        t.text = label + "\n" + turns + "t";
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = 10;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        return go;
    }

    void UpdateStatusIconText(GameObject icon, string label, Color color)
    {
        Text t = icon.GetComponent<Text>();
        if (t != null) t.text = label;
        Image img = icon.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void UpdateBossBar()
    {
        Unit bossOrElite = null;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u.isEnemy && (u.isBoss || u.isElite || u.maxHealth >= 50)) 
            {
                bossOrElite = u;
                break;
            }
        }

        if (bossOrElite != null)
        {
            if (!bossBarRoot.activeSelf) bossBarRoot.SetActive(true);
            if (bossNameText != null) bossNameText.text = (bossOrElite.isBoss ? "JEFE: " : "ELITE: ") + bossOrElite.gameObject.name;
            float ratio = Mathf.Clamp01((float)bossOrElite.currentHealth / bossOrElite.maxHealth);
            if (bossHpFill != null) bossHpFill.anchorMax = new Vector2(ratio, 1f);
            if (bossHpText != null) bossHpText.text = bossOrElite.currentHealth + " / " + bossOrElite.maxHealth;
        }
        else
        {
            if (bossBarRoot != null && bossBarRoot.activeSelf) bossBarRoot.SetActive(false);
        }
    }

    void MakeHint(Transform parent, string content)
    {
        GameObject go = new GameObject("Hint");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 8);
        rt.sizeDelta = new Vector2(900, 24);

        Text t = go.AddComponent<Text>();
        t.text = content;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = 14;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.75f, 0.75f, 0.75f, 0.8f);
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 size, TextAnchor align, int fontSize, Color color)
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
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        return t;
    }
}