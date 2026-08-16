using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    private RectTransform canvasRt;
    private RectTransform panelRt;
    private Text titleText;
    private Text descriptionText;
    private Text statsText;

    private const float PANEL_WIDTH = 300f;
    private const float OFFSET = 12f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Build();
    }

    void Build()
    {
        GameObject canvas = UIFactory.CreateCanvas("TooltipCanvas", 100);
        canvasRt = canvas.GetComponent<RectTransform>();

        panelRt = UIFactory.CreatePanel(canvasRt, "TooltipPanel",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            Vector2.zero, new Vector2(PANEL_WIDTH, 10), new Color(0.05f, 0.05f, 0.08f, 0.95f));

        VerticalLayoutGroup vlg = panelRt.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter fitter = panelRt.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        titleText = UIFactory.CreateText(panelRt, "Title", "", 16, TextAnchor.UpperLeft, Color.yellow,
            null, null, null, Vector2.zero, new Vector2(280, 25));
        descriptionText = UIFactory.CreateText(panelRt, "Description", "", 12, TextAnchor.UpperLeft, Color.white,
            null, null, null, Vector2.zero, new Vector2(280, 40));
        statsText = UIFactory.CreateText(panelRt, "Stats", "", 11, TextAnchor.UpperLeft, Color.cyan,
            null, null, null, Vector2.zero, new Vector2(280, 60));

        descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statsText.verticalOverflow = VerticalWrapMode.Overflow;

        panelRt.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (panelRt == null || !panelRt.gameObject.activeSelf) return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, Input.mousePosition, null, out localPoint))
            return;

        // FIX: localPoint tiene origen en el CENTRO del canvas (pivote 0.5).
        // El panel está anclado abajo-izquierda: convertimos sumando medio rect.
        Vector2 half = canvasRt.rect.size * 0.5f;
        Vector2 pos = localPoint + half;

        float x = pos.x + OFFSET;
        float y = pos.y + OFFSET;

        float w = PANEL_WIDTH;
        float h = Mathf.Max(panelRt.rect.height, 60f);
        float cw = canvasRt.rect.width;
        float ch = canvasRt.rect.height;

        // Volteo automático en bordes.
        if (x + w > cw - 5f) x = pos.x - w - OFFSET;
        if (y + h > ch - 5f) y = pos.y - h - OFFSET;

        panelRt.anchoredPosition = new Vector2(x, y);
    }

    public void ShowSkillTooltip(SkillData skill, bool isUnlocked, int playerLevel)
    {
        if (skill == null) return;

        titleText.text = skill.skillName;

        string desc = skill.description;
        if (!isUnlocked)
        {
            desc = "<color=#ff6666>BLOQUEADA</color> - Requiere nivel " + skill.unlockLevel + "\n" + desc;
        }
        descriptionText.text = desc;

        string stats = "Costo: " + skill.actionPointCost + " AP\n";
        stats += "Rango: " + skill.range + " casillas\n";
        stats += "Daño: " + skill.damage;
        if (skill.bonusCrit > 0) stats += " (+" + skill.bonusCrit + "% crítico)";
        stats += "\nAmenaza: x" + skill.threatMult.ToString("F1");

        statsText.text = stats;
        panelRt.gameObject.SetActive(true);
    }

    public void ShowUnitTooltip(Unit unit)
    {
        if (unit == null) return;

        titleText.text = unit.gameObject.name;
        descriptionText.text = unit.isEnemy ? "Enemigo" : "Jugador";

        string stats = "HP: " + unit.currentHealth + " / " + unit.maxHealth + "\n";
        if (!unit.isEnemy)
        {
            stats += "AP: " + unit.currentAP + " / " + unit.maxAP + "\n";
            stats += "Daño: " + (unit.stats.damage + unit.buffDamage) + "\n";
            stats += "Defensa: " + (unit.stats.defense + unit.buffDefense) + "\n";
            stats += "Crítico: " + (unit.stats.critChance + unit.buffCrit) + "%\n";
            stats += "Precisión: " + (unit.stats.attack - unit.debuffAttack) + "%";
        }

        if (unit.buffTurns > 0 || unit.debuffTurns > 0)
        {
            stats += "\n<color=#66ccff>Estados activos:</color>";
            if (unit.buffTurns > 0) stats += "\n  BUFF (" + unit.buffTurns + " turnos)";
            if (unit.debuffTurns > 0) stats += "\n  MALDICIÓN (" + unit.debuffTurns + " turnos)";
        }

        statsText.text = stats;
        panelRt.gameObject.SetActive(true);
    }

    public void ShowConsumableTooltip(ConsumableType type, int count)
    {
        titleText.text = ConsumableCatalog.Name(type);
        descriptionText.text = ConsumableCatalog.Description(type);
        statsText.text = "Cantidad: " + count;
        panelRt.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (panelRt != null) panelRt.gameObject.SetActive(false);
    }
}