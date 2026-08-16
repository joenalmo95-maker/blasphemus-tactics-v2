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

    private Unit playerUnit;
    private readonly List<GameObject> activeBuffIcons = new List<GameObject>();
    private readonly List<GameObject> activeDebuffIcons = new List<GameObject>();

    void Awake()
    {
        Build();
        gameObject.AddComponent<ActionBarUI>();
        if (TooltipUI.Instance == null)
        {
            new GameObject("TooltipUI").AddComponent<TooltipUI>();
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
        // Cuadrícula: retrato+nivel a la izquierda; HP/AP y estados a la derecha.
        RectTransform playerHud = UIFactory.CreatePanel(root.transform, "PlayerHUD",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(20, 40), new Vector2(320, 105));

        // Retrato alineado al tope con las barras
        RectTransform portrait = UIFactory.CreatePanel(playerHud, "Portrait",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(70, 70), Color.gray);
        portraitImage = portrait.GetComponent<Image>();
        if (portraitImage != null) portraitImage.sprite = SpriteFactory.Square();

        // Nivel debajo del retrato
        levelText = UIFactory.CreateText(playerHud, "Level", "Nv 1", 16, TextAnchor.UpperCenter, Color.white,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, -75), new Vector2(70, 25));

        // Barras HP/AP a la derecha del retrato, alineadas al tope
        hpBar = UIFactory.CreateBar(playerHud, "HP", new Vector2(80, 0), new Vector2(220, 28),
            new Color(0.4f, 0f, 0f), new Color(0.9f, 0.1f, 0.1f));
        apBar = UIFactory.CreateBar(playerHud, "AP", new Vector2(80, -33), new Vector2(220, 28),
            new Color(0f, 0.2f, 0.4f), new Color(0.1f, 0.5f, 0.9f));

        // Buffs/Debuffs en fila propia DEBAJO de la barra de AP
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

        xpFill = UIFactory.CreatePanel(xpBg, "XP_Fill",
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
            Vector2.zero, new Vector2(0, 16), Color.cyan);

        UIFactory.CreateText(root.transform, "Hint",
            "1/2: habilidades 3: utilidad 4-7: consumibles I: inventario B: tienda E: fin de turno",
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

    // Un GameObject UI solo admite UNA Graphic: Image en el padre, Text como hijo estirado.
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