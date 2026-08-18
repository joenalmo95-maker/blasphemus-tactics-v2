using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDUI : MonoBehaviour
{
    private Image portraitImage;
    private Text levelText;
    private UIFactory.BarUI hpBar;
    private UIFactory.BarUI apBar;
    private Transform buffContainer;
    private Transform debuffContainer;

    private Text goldText;
    private RectTransform xpFill;
    private float xpBarWidth = 200f;

    private GameObject bossBarRoot;
    private Text bossNameText;
    private RectTransform bossHpFill;
    private Text bossHpText;

    private string currentPortraitKey = "";
    private Text objectiveText;

    private Unit playerUnit;
    private readonly List<GameObject> activeBuffIcons = new List<GameObject>();
    private readonly List<GameObject> activeDebuffIcons = new List<GameObject>();

    void Awake()
    {
        Build();

        // ActionBar y botón de huida: exclusivos de combate
        gameObject.AddComponent<ActionBarUI>();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            gameObject.AddComponent<FleeUI>();
        }

    }

    void Build()
    {
        GameObject root = UIFactory.CreateCanvas("HUDCanvas", 50);

        // --- BARRA DE JEFE / ÉLITE (superior centro) ---
        RectTransform bossRootRt = UIFactory.CreatePanel(root.transform, "BossBarRoot",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -20), new Vector2(600, 60));
        bossBarRoot = bossRootRt.gameObject;

        bossNameText = UIFactory.CreateText(bossBarRoot.transform, "BossName", "", 24, TextAnchor.UpperCenter, Color.yellow,
            new Vector2(0, 0.5f), new Vector2(1, 1f), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        Stretch(bossNameText.rectTransform);

        UIFactory.BarUI bossBar = UIFactory.CreateBar(bossBarRoot.transform, "BossHP",
            new Vector2(0, 0), new Vector2(600, 25),
            new Color(0.1f, 0.1f, 0.1f, 0.8f), new Color(0.8f, 0.1f, 0.1f));
        RectTransform bossBgRt = bossBar.root.GetComponent<RectTransform>();
        bossBgRt.anchorMin = new Vector2(0, 0);
        bossBgRt.anchorMax = new Vector2(1, 0.4f);
        bossBgRt.offsetMin = Vector2.zero;
        bossBgRt.offsetMax = Vector2.zero;

        bossHpFill = bossBar.fill;
        bossHpFill.anchorMin = new Vector2(0, 0);
        bossHpFill.anchorMax = new Vector2(1, 1);
        bossHpFill.offsetMin = Vector2.zero;
        bossHpFill.offsetMax = Vector2.zero;

        bossHpText = bossBar.text;
        bossBarRoot.SetActive(false);

        // --- HUD DEL JUGADOR (inferior izquierda) ---
        RectTransform playerHud = UIFactory.CreatePanel(root.transform, "PlayerHUD",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(20, 40), new Vector2(320, 105));

        RectTransform portrait = UIFactory.CreatePanel(playerHud, "Portrait",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(70, 70), Color.gray);
        portraitImage = portrait.GetComponent<Image>();
        SetPortrait();

        levelText = UIFactory.CreateText(playerHud, "Level", "Nv 1", 16, TextAnchor.UpperCenter, Color.white,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, -75), new Vector2(70, 25));

        hpBar = UIFactory.CreateBar(playerHud, "HP", new Vector2(80, 0), new Vector2(220, 28),
            new Color(0.4f, 0f, 0f), new Color(0.9f, 0.1f, 0.1f));
        apBar = UIFactory.CreateBar(playerHud, "AP", new Vector2(80, -33), new Vector2(220, 28),
            new Color(0f, 0.2f, 0.4f), new Color(0.1f, 0.5f, 0.9f));

        RectTransform statusContainer = UIFactory.CreatePanel(playerHud, "StatusIcons",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(80, -66), new Vector2(240, 30));
        HorizontalLayoutGroup hlg = statusContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        buffContainer = CreateStatusContainer(statusContainer, "Buffs");
        debuffContainer = CreateStatusContainer(statusContainer, "Debuffs");

        // --- ORO Y EXP (superior izquierda) ---
        goldText = UIFactory.CreateText(root.transform, "Gold", "Oro: 0", 18, TextAnchor.UpperLeft, Color.yellow,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(150, 30));

        RectTransform xpBg = UIFactory.CreatePanel(root.transform, "XP_BG",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(180, -25), new Vector2(xpBarWidth, 16), new Color(0.1f, 0.1f, 0.1f, 0.8f));


        // 5.3: objetivo visible bajo oro/EXP
        objectiveText = UIFactory.CreateText(root.transform, "Objective", "", 14, TextAnchor.UpperLeft,
            new Color(0.9f, 0.8f, 0.4f),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -50), new Vector2(760, 24));

        UIFactory.CreateText(root.transform, "Hint",
            "1-4: habilidades 5: ultimate 6-9: consumibles I: inventario B: tienda E: fin de turno ESC: huir",
            14, TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.75f, 0.8f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 8), new Vector2(900, 24));
    }

    Transform CreateStatusContainer(RectTransform parent, string name)
    {
        RectTransform rt = UIFactory.CreatePanel(parent, name);
        HorizontalLayoutGroup g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        g.spacing = 2;
        g.childForceExpandWidth = false;
        g.childForceExpandHeight = true;
        rt.gameObject.AddComponent<LayoutElement>().preferredWidth = 115;
        return rt;
    }

    void Stretch(RectTransform rt)
    {
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetPortrait()
    {
        if (portraitImage == null) return;
        string art = "dps";
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
        {
            switch (CharacterData.Instance.classData.role)
            {
                case ClassRole.Tank: art = "tank"; break;
                case ClassRole.Healer: art = "healer"; break;
                default: art = "dps"; break;
            }
        }
        if (art != currentPortraitKey)
        {
            currentPortraitKey = art;
            portraitImage.sprite = ArtProvider.Get(art);
            portraitImage.color = Color.white;
        }
    }

    void Update()
    {
        if (playerUnit == null)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units) if (!u.isEnemy) { playerUnit = u; break; }
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
            SetPortrait();
        }

        // 5.3: refresco del objetivo actual
        if (objectiveText != null) objectiveText.text = ObjectiveSystem.Current();

        if (playerUnit != null)
        {
            float hpRatio = Mathf.Clamp01((float)playerUnit.currentHealth / playerUnit.maxHealth);
            if (hpBar.fill != null) hpBar.fill.sizeDelta = new Vector2(220 * hpRatio, 28);
            if (hpBar.text != null) hpBar.text.text = playerUnit.currentHealth + " / " + playerUnit.maxHealth;

            float apRatio = Mathf.Clamp01((float)playerUnit.currentAP / playerUnit.maxAP);
            if (apBar.fill != null) apBar.fill.sizeDelta = new Vector2(220 * apRatio, 28);
            if (apBar.text != null) apBar.text.text = playerUnit.currentAP + " / " + playerUnit.maxAP;

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
            if (buffCount > 0) activeBuffIcons.Add(CreateStatusIcon(buffContainer, "BUFF", Color.blue, playerUnit.buffTurns));
        }
        else if (buffCount > 0)
        {
            UpdateStatusIconText(activeBuffIcons[0], "BUFF\n" + playerUnit.buffTurns + "t", Color.blue);
        }

        if (activeDebuffIcons.Count != debuffCount)
        {
            foreach (var g in activeDebuffIcons) Destroy(g);
            activeDebuffIcons.Clear();
            if (debuffCount > 0) activeDebuffIcons.Add(CreateStatusIcon(debuffContainer, "MALDIC.", Color.magenta, playerUnit.debuffTurns));
        }
        else if (debuffCount > 0)
        {
            UpdateStatusIconText(activeDebuffIcons[0], "MALDIC.\n" + playerUnit.debuffTurns + "t", Color.magenta);
        }
    }

    GameObject CreateStatusIcon(Transform parent, string label, Color color, int turns)
    {
        RectTransform rt = UIFactory.CreatePanel(parent, "StatusIcon",
            null, null, null, null, new Vector2(40, 30), color);

        Text t = UIFactory.CreateText(rt, "Label", label + "\n" + turns + "t", 10,
            TextAnchor.MiddleCenter, Color.white,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        Stretch(t.rectTransform);

        return rt.gameObject;
    }

    void UpdateStatusIconText(GameObject icon, string label, Color color)
    {
        Text t = icon.GetComponentInChildren<Text>();
        if (t != null) t.text = label;
        Image img = icon.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void UpdateBossBar()
    {
        Unit bossOrElite = null;
        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u.isEnemy && (u.isBoss || u.isElite || u.maxHealth >= 50)) { bossOrElite = u; break; }
        }

        if (bossOrElite != null)
        {
            if (!bossBarRoot.activeSelf) bossBarRoot.SetActive(true);
            if (bossNameText != null) bossNameText.text = (bossOrElite.isBoss ? "JEFE: " : "ELITE: ") + bossOrElite.gameObject.name;
            float ratio = Mathf.Clamp01((float)bossOrElite.currentHealth / bossOrElite.maxHealth);
            if (bossHpFill != null) bossHpFill.anchorMax = new Vector2(ratio, 1f);
            if (bossHpText != null) bossHpText.text = bossOrElite.currentHealth + " / " + bossOrElite.maxHealth;
        }
        else if (bossBarRoot != null && bossBarRoot.activeSelf)
        {
            bossBarRoot.SetActive(false);
        }
    }
}