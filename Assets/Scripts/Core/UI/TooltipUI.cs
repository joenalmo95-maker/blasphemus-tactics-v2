using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    private GameObject canvas;
    private GameObject tooltipPanel;
    private Text titleText;
    private Text descriptionText;
    private Text statsText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Build();
    }

    void Build()
    {
        canvas = UIFactory.CreateCanvas("TooltipCanvas", 100);

        RectTransform panelRt = UIFactory.CreatePanel(canvas.transform, "TooltipPanel",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            Vector2.zero, new Vector2(300, 150), new Color(0.05f, 0.05f, 0.08f, 0.95f));
        tooltipPanel = panelRt.gameObject;

        VerticalLayoutGroup vlg = tooltipPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        titleText = UIFactory.CreateText(tooltipPanel.transform, "Title", "", 16, TextAnchor.UpperLeft, Color.yellow,
            null, null, null, Vector2.zero, new Vector2(280, 30));

        descriptionText = UIFactory.CreateText(tooltipPanel.transform, "Description", "", 12, TextAnchor.UpperLeft, Color.white,
            null, null, null, Vector2.zero, new Vector2(280, 40));

        statsText = UIFactory.CreateText(tooltipPanel.transform, "Stats", "", 11, TextAnchor.UpperLeft, Color.cyan,
            null, null, null, Vector2.zero, new Vector2(280, 60));

        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            Vector3 mousePos = Input.mousePosition;
            RectTransform panelRt = tooltipPanel.GetComponent<RectTransform>();
            
            float x = mousePos.x + 15;
            float y = mousePos.y - 15;

            if (x + 300 > Screen.width) x = mousePos.x - 315;
            if (y - 150 < 0) y = mousePos.y + 15;

            panelRt.anchoredPosition = new Vector2(x, y);
        }
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
        tooltipPanel.SetActive(true);
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
        tooltipPanel.SetActive(true);
    }

    public void ShowConsumableTooltip(ConsumableType type, int count)
    {
        titleText.text = ConsumableCatalog.Name(type);
        descriptionText.text = ConsumableCatalog.Description(type);
        statsText.text = "Cantidad: " + count;
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}