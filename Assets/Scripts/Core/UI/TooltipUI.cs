using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    private bool pinned;
    private RectTransform pinnedTarget;
    private int noPointerFrames;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnsureEventSystem();
        Build();
    }

    // 1.1-FINAL: garantiza EventSystem desde el arranque de cada escena
    // (corrige tooltips que no salen en la PRIMERA UI abierta)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureAtBoot()
    {
        EnsureEventSystem();
        if (Instance == null)
        {
            new GameObject("TooltipUI").AddComponent<TooltipUI>();
            Debug.Log("[TooltipUI] instancia creada al arranque.");
        }
    }

    static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[TooltipUI] EventSystem garantizado.");
        }
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

        // Auto-ocultado: si el puntero ya no está sobre UI, el tooltip muere (evita huérfanos tras Rebuild)
        if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
        {
            noPointerFrames++;
            if (noPointerFrames > 10) { Hide(); noPointerFrames = 0; }
        }
        else noPointerFrames = 0;

        // Modo pinned: anclado encima del slot
        if (pinned && pinnedTarget != null)
        {
            PositionPinned();
            return;
        }

        // Modo ratón (trainer/inventario/mercader)
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
            sb.Append("Precisión: " + (unit.stats.accuracy - unit.debuffAccuracy) + "%");
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

    public void ShowItemTooltip(ItemData item, ItemData equipped)
    {
        if (item == null) return;
        // 0.3 FIX: item.rarity YA es enum Rarity, usarlo directamente
        titleText.color = ItemGenerator.RarityColor(item.rarity);
        titleText.text = item.itemName;

        string desc = RarityLabel(item.rarity) + " · " + SlotLabel(item.slot);
        if (item.armorType != ArmorType.Ninguna)
        {
            desc += " · " + item.armorType;
            ClassData cdTip = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
            desc += ItemGenerator.CanEquipClass(item, cdTip)
                ? " <color=#00ff00>(usable)</color>"
                : " <color=#ff5555>(no usable)</color>";
        }
        if (!string.IsNullOrEmpty(item.requiredClass)) desc += "\nClase: " + item.requiredClass;
        if (equipped != null) desc += "\n<color=#aaaaaa>Reemplaza: " + equipped.itemName + "</color>";
        descriptionText.text = desc;

        bool cmp = equipped != null;
        StringBuilder sb = new StringBuilder();
        AppendStat(sb, "HP máx", item.stats.maxHP, cmp ? equipped.stats.maxHP : 0, "", cmp);
        AppendStat(sb, "Defensa", item.stats.defense, cmp ? equipped.stats.defense : 0, "", cmp);
        AppendStat(sb, "Daño", item.stats.damage, cmp ? equipped.stats.damage : 0, "", cmp);
        AppendStat(sb, "Precisión", item.stats.accuracy, cmp ? equipped.stats.accuracy : 0, "", cmp);
        AppendStat(sb, "Crítico", item.stats.critChance + 0, cmp ? equipped.stats.critChance : 0, "%", cmp);
        AppendStat(sb, "Evasión", item.stats.evasion, cmp ? equipped.stats.evasion : 0, "%", cmp);
        AppendStat(sb, "AP", item.stats.apMove, cmp ? equipped.stats.apMove : 0, "", cmp);
        AppendStat(sb, "Curación", item.stats.healingPower, cmp ? equipped.stats.healingPower : 0, "%", cmp);
        AppendStat(sb, "Robo de vida", item.stats.lifesteal, cmp ? equipped.stats.lifesteal : 0, "%", cmp);
        sb.AppendLine("Venta: " + ItemGenerator.SellPrice(item) + " oro");

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    // --- Tarjeta rica de skill del pool ---
    public void ShowPoolSkillTooltip(string id)
    {
        SkillData sk = SkillPool.Get(id);
        SkillMeta meta = SkillPool.Meta(id);
        if (sk == null || meta == null) return;

        // 0.3 FIX: meta.rarity es string, convertir a enum Rarity
        Rarity metaRar = Rarity.Common;
        if (meta.rarity == "Rare") metaRar = Rarity.Rare;
        else if (meta.rarity == "Epic") metaRar = Rarity.Epic;
        else if (meta.rarity == "Legendary") metaRar = Rarity.Legendary;
        titleText.color = ItemGenerator.RarityColor(metaRar);
        titleText.text = sk.skillName + "  [" + meta.rarity + "]";

        string desc = meta.type + " · " + meta.affinity;
        if (!string.IsNullOrEmpty(meta.tag)) desc += " · " + meta.tag;
        desc += "\n" + sk.description;
        descriptionText.text = desc;

        StringBuilder sb = new StringBuilder();
        if (meta.type == SkillType.Pasiva)
        {
            sb.AppendLine("Pasiva permanente (slot de pasiva).");
        }
        else
        {
            sb.AppendLine("Coste: " + sk.actionPointCost + " AP");
            if (meta.type == SkillType.Ultimate) sb.AppendLine("Cooldown: " + meta.cooldown + " turnos");
            sb.AppendLine("Rango: " + sk.range + " casillas");
            if (sk.damage > 0) sb.AppendLine("Daño: " + sk.damage + (sk.bonusCrit > 0 ? " (+" + sk.bonusCrit + "% crit)" : ""));
            if (meta.heal > 0) sb.AppendLine("Curación: " + meta.heal);
        }
        if (!LoadoutSystem.IsLearned(id))
            sb.AppendLine("Aprender: " + meta.cost + " oro (" + meta.origin + ")");
        else
            sb.AppendLine("Aprendida (" + meta.origin + ")");

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    public void ShowUltimateTooltip(SkillData ult, int cooldown)
    {
        if (ult == null) return;
        titleText.color = Color.magenta;
        titleText.text = "ULTIMATE: " + ult.skillName;
        descriptionText.text = ult.description;

        StringBuilder sb = new StringBuilder();
        string ultId = LoadoutSystem.UltimateId();
        SkillMeta meta = (ultId != "") ? SkillPool.Meta(ultId) : null;
        int cdTurns = (meta != null) ? meta.cooldown : 3;
        sb.AppendLine("Cooldown: " + cdTurns + " turnos");
        if (cooldown > 0)
            sb.AppendLine("<color=#ff6666>Recarga: " + cooldown + " turnos restantes</color>");
        else
            sb.AppendLine("<color=#00ff00>LISTO PARA USAR</color>");
        sb.AppendLine("Daño: " + ult.damage);
        sb.AppendLine("Rango: " + ult.range + " casillas");

        statsText.text = sb.ToString();
        panelRt.gameObject.SetActive(true);
    }

    // --- Variantes pinned ---
    public void ShowPoolSkillTooltipPinned(string id, RectTransform target)
    {
        ShowPoolSkillTooltip(id);
        SetPinned(target);
    }

    public void ShowUltimateTooltipPinned(SkillData ult, int cooldown, RectTransform target)
    {
        ShowUltimateTooltip(ult, cooldown);
        SetPinned(target);
    }

    public void ShowConsumableTooltipPinned(ConsumableType type, int count, RectTransform target)
    {
        ShowConsumableTooltip(type, count);
        SetPinned(target);
    }

    void SetPinned(RectTransform target)
    {
        pinned = true;
        pinnedTarget = target;
        PositionPinned();
    }

    void PositionPinned()
    {
        if (pinnedTarget == null || panelRt == null) return;

        Vector3[] corners = new Vector3[4];
        pinnedTarget.GetWorldCorners(corners);
        Vector2 screenTopCenter = RectTransformUtility.WorldToScreenPoint(null, (corners[1] + corners[2]) * 0.5f);

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenTopCenter, null, out localPoint))
            localPoint = Vector2.zero;

        Vector2 half = canvasRt.rect.size * 0.5f;
        Vector2 pos = localPoint + half;

        float w = PANEL_WIDTH;
        float h = Mathf.Max(panelRt.rect.height, 60f);
        float cw = canvasRt.rect.width;
        float ch = canvasRt.rect.height;

        float x = pos.x - w * 0.5f;
        float y = pos.y + OFFSET;

        x = Mathf.Clamp(x, 5f, cw - w - 5f);
        y = Mathf.Clamp(y, 5f, ch - h - 5f);

        panelRt.anchoredPosition = new Vector2(x, y);
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
        pinned = false;
        pinnedTarget = null;
        if (panelRt != null) panelRt.gameObject.SetActive(false);
    }
}