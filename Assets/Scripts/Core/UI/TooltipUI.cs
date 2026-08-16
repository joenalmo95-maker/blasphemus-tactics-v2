using UnityEngine;
using UnityEngine.UI;
using System.Text;

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

        Vector2 half = canvasRt.rect.size * 0.5f;
        Vector2 pos = localPoint + half;

        float x = pos.x + OFFSET;
        float y = pos.y + OFFSET;

        float w = PANEL_WIDTH;
        float h = Mathf.Max(panelRt.rect.height, 60f);
        float cw = canvasRt.rect.width;
        float ch = canvasRt.rect.height;

        if (x + w > cw - 5f) x = pos.x - w - OFFSET;
        if (y + h > ch - 5f) y = pos.y - h - OFFSET;

        panelRt.anchoredPosition = new Vector2(x, y);
    }

    public void ShowSkillTooltip(SkillData skill, bool isUnlocked, int playerLevel)
    {
        if (skill == null) return;
        titleText.color = Color.yellow;
        titleText.text = skill.skillName;

        string desc = skill.description;
        if (!isUnlocked)
        {
            desc = "<color=#ff6666>BLOQUEADA</color> - Requiere nivel " + skill.unlockLevel + "\n" + desc;
        }
        descriptionText.text = desc;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Costo: " + skill.actionPointCost + " AP");
        sb.AppendLine("Rango: " + skill.range + " casillas");
        sb.Append("Daño: " + skill.damage);
        if (skill.bonusCrit > 0) sb.Append(" (+" + skill.bonusCrit + "% crítico)");
        sb.Append("\nAmenaza: x" + skill.threatMult.ToString("F1"));

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    public void ShowUnitTooltip(Unit unit)
    {
        if (unit == null) return;
        titleText.color = Color.yellow;
        titleText.text = unit.gameObject.name;
        descriptionText.text = unit.isEnemy ? "Enemigo" : "Jugador";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("HP: " + unit.currentHealth + " / " + unit.maxHealth);
        if (!unit.isEnemy)
        {
            sb.AppendLine("AP: " + unit.currentAP + " / " + unit.maxAP);
            sb.AppendLine("Daño: " + (unit.stats.damage + unit.buffDamage));
            sb.AppendLine("Defensa: " + (unit.stats.defense + unit.buffDefense));
            sb.AppendLine("Crítico: " + (unit.stats.critChance + unit.buffCrit) + "%");
            sb.Append("Precisión: " + (unit.stats.attack - unit.debuffAttack) + "%");
        }

        if (unit.buffTurns > 0 || unit.debuffTurns > 0)
        {
            sb.AppendLine("\n<color=#66ccff>Estados activos:</color>");
            if (unit.buffTurns > 0) sb.AppendLine("  BUFF (" + unit.buffTurns + " turnos)");
            if (unit.debuffTurns > 0) sb.AppendLine("  MALDICIÓN (" + unit.debuffTurns + " turnos)");
        }

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    public void ShowConsumableTooltip(ConsumableType type, int count)
    {
        titleText.color = Color.yellow;
        titleText.text = ConsumableCatalog.Name(type);
        descriptionText.text = ConsumableCatalog.Description(type);
        statsText.text = "Cantidad: " + count;
        panelRt.gameObject.SetActive(true);
    }

    // BLOQUE 1.5: Tarjeta de item con comparativa contra el slot equipado.
    public void ShowItemTooltip(ItemData item, ItemData equipped)
    {
        if (item == null) return;

        titleText.color = ItemGenerator.RarityColor(item.rarity);
        titleText.text = item.itemName;

        string desc = RarityLabel(item.rarity) + " · " + SlotLabel(item.slot);
        if (!string.IsNullOrEmpty(item.requiredClass)) desc += "\nClase: " + item.requiredClass;
        if (equipped != null) desc += "\n<color=#aaaaaa>Reemplaza: " + equipped.itemName + "</color>";
        descriptionText.text = desc;

        bool cmp = equipped != null;
        StringBuilder sb = new StringBuilder();
        AppendStat(sb, "HP máx", item.stats.maxHP, cmp ? equipped.stats.maxHP : 0, "", cmp);
        AppendStat(sb, "Defensa", item.stats.defense, cmp ? equipped.stats.defense : 0, "", cmp);
        AppendStat(sb, "Daño", item.stats.damage, cmp ? equipped.stats.damage : 0, "", cmp);
        AppendStat(sb, "Precisión", item.stats.attack, cmp ? equipped.stats.attack : 0, "", cmp);
        AppendStat(sb, "Crítico", item.stats.critChance, cmp ? equipped.stats.critChance : 0, "%", cmp);
        AppendStat(sb, "Evasión", item.stats.evasion, cmp ? equipped.stats.evasion : 0, "%", cmp);
        AppendStat(sb, "AP", item.stats.apMove, cmp ? equipped.stats.apMove : 0, "", cmp);
        AppendStat(sb, "Curación", item.stats.healingPower, cmp ? equipped.stats.healingPower : 0, "%", cmp);
        AppendStat(sb, "Robo de vida", item.stats.lifesteal, cmp ? equipped.stats.lifesteal : 0, "%", cmp);
        sb.AppendLine("Venta: " + ItemGenerator.SellPrice(item) + " oro");

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    void AppendStat(StringBuilder sb, string label, int val, int eqVal, string suffix, bool compare)
    {
        if (val == 0) return;
        string line = "+" + val + suffix + " " + label;
        if (compare)
        {
            int delta = val - eqVal;
            if (delta != 0)
            {
                string col = delta > 0 ? "#00ff00" : "#ff5555";
                line += " <color=" + col + ">(" + (delta > 0 ? "+" + delta : delta.ToString()) + ")</color>";
            }
        }
        sb.AppendLine(line);
    }

    string RarityLabel(Rarity r)
    {
        switch (r)
        {
            case Rarity.Rare: return "Rara";
            case Rarity.Epic: return "Épica";
            case Rarity.Legendary: return "Legendaria";
            default: return "Común";
        }
    }

    string SlotLabel(ItemSlot s)
    {
        switch (s)
        {
            case ItemSlot.Weapon: return "Arma";
            case ItemSlot.Chest: return "Peto";
            case ItemSlot.Legs: return "Pantalón";
            case ItemSlot.Helm: return "Casco";
            default: return "Guantes";
        }
    }

    public void Hide()
    {
        if (panelRt != null) panelRt.gameObject.SetActive(false);
    }
}