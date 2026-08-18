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
    private Text xpText;
    private float xpBarWidth = 200f;

    private GameObject bossBarRoot;
    private Text bossNameText;
    private RectTransform bossHpFill;
    private Text bossHpText;

    private string currentPortraitKey = "";
    private Text objectiveText;
 // 2.2: seguimiento de misiones aceptadas (tecla J)
    private GameObject questTrackerRoot;
    private Text questTrackerText;
    private float questTrackerTimer;

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


        // Relleno de la barra de EXP (faltante)
        xpFill = UIFactory.CreatePanel(xpBg, "XP_Fill",
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
            Vector2.zero, new Vector2(0, 16), Color.cyan);

        // 1.1-D.7: progreso de EXP visible en texto
        xpText = UIFactory.CreateText(root.transform, "XP_Text", "XP 0/0", 14, TextAnchor.MiddleLeft, Color.cyan,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(390, -25), new Vector2(160, 20));
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
                float wpx = xpBarWidth * t;
                if (cd.xp > 0 && wpx < 2) wpx = 2; // mínimo visible
                xpFill.sizeDelta = new Vector2(wpx, 16);
            }
            if (xpText != null) xpText.text = "XP " + cd.xp + "/" + cd.XpToNextLevel();
            if (levelText != null) levelText.text = "Nv " + cd.level;
            SetPortrait();
        }

        // 5.3: refresco del objetivo actual
        if (objectiveText != null) objectiveText.text = ObjectiveSystem.Current();
             // 2.2: toggle J del seguimiento de misiones aceptadas
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (questTrackerRoot == null) OpenQuestTracker();
            else CloseQuestTracker();
        }
     // DEBUG: F9 fuerza reset diario (misiones + mazmorras)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuestSystem.DebugForceDailyReset();
            DungeonDaily.ResetToday();
            Debug.Log("[HUD] DEBUG F9: reset diario forzado.");
        }
        if (questTrackerRoot != null)
        {
            QuestSystem.Tick();
            questTrackerTimer += Time.deltaTime;
            if (questTrackerTimer >= 1f) { questTrackerTimer = 0f; RefreshQuestTracker(); }
        }

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
 // --- 2.2: tracker de misiones aceptadas (J) ---
 void OpenQuestTracker()
 {
     questTrackerRoot = new GameObject("QuestTrackerCanvas");
     Canvas c = questTrackerRoot.AddComponent<Canvas>();
     c.renderMode = RenderMode.ScreenSpaceOverlay;
     c.sortingOrder = 60;
     questTrackerRoot.AddComponent<GraphicRaycaster>();

     GameObject panel = new GameObject("Panel");
     panel.transform.SetParent(questTrackerRoot.transform, false);
     RectTransform prt = panel.AddComponent<RectTransform>();
     prt.anchorMin = new Vector2(0, 1);
     prt.anchorMax = new Vector2(0, 1);
     prt.pivot = new Vector2(0, 1);
     prt.anchoredPosition = new Vector2(10, -80);
     prt.sizeDelta = new Vector2(420, 320);
     Image img = panel.AddComponent<Image>();
     img.sprite = SpriteFactory.Square();
     img.color = new Color(0.03f, 0.03f, 0.05f, 0.88f);

     GameObject txtObj = new GameObject("Body");
     txtObj.transform.SetParent(panel.transform, false);
     RectTransform brt = txtObj.AddComponent<RectTransform>();
     brt.anchorMin = Vector2.zero;
     brt.anchorMax = Vector2.one;
     brt.offsetMin = new Vector2(12, 12);
     brt.offsetMax = new Vector2(-12, -12);
     questTrackerText = txtObj.AddComponent<Text>();
     questTrackerText.font = GetTrackerFont();
     questTrackerText.fontSize = 13;
     questTrackerText.alignment = TextAnchor.UpperLeft;
     questTrackerText.color = Color.white;
     questTrackerText.horizontalOverflow = HorizontalWrapMode.Wrap;
     questTrackerText.verticalOverflow = VerticalWrapMode.Overflow;
     RefreshQuestTracker();
 }

 void CloseQuestTracker()
 {
     if (questTrackerRoot != null) Destroy(questTrackerRoot);
     questTrackerRoot = null;
     questTrackerText = null;
 }

 void RefreshQuestTracker()
 {
     if (questTrackerText == null) return;
     System.Text.StringBuilder sb = new System.Text.StringBuilder();
     sb.AppendLine("<b>MISIONES ACEPTADAS</b>  (J para cerrar)");
     sb.AppendLine();
     int count = 0;
     foreach (QuestState q in QuestSystem.Actives())
     {
         if (!q.accepted || q.claimed) continue;
         QuestDef d = QuestSystem.GetDef(q.id);
         if (d == null) continue;
         count++;
         sb.Append("• " + d.description + "  [" + q.progress + "/" + d.target + "]");
         if (q.expiry > 0) sb.Append("  <color=#ffcc44>(" + QuestSystem.MinutesLeft(q.id) + " min)</color>");
         sb.AppendLine();
     }
     if (count == 0)
     {
         sb.AppendLine("Sin misiones aceptadas.");
         sb.AppendLine("Visita el Tablón en la ciudad (Q o E junto al NPC cian).");
     }
        questTrackerText.text = sb.ToString();
    }

    Font GetTrackerFont()
    {
     Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
     if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
     return f;
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