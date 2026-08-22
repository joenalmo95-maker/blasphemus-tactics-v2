using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

public class ActionBarUI : MonoBehaviour
{
    private GameObject actionBarRoot;
    private readonly List<ActionButton> actionButtons = new List<ActionButton>();
    private readonly List<RectTransform> slotRects = new List<RectTransform>();
    private readonly List<float> slotX = new List<float>();
    private Unit playerUnit;
    private int lastHovered = -1;

    private GameObject tipRoot;
    private RectTransform tipRt;
    private Text tipTitle;
    private Text tipDesc;
    private Text tipStats;

    class ActionButton
    {
        public GameObject button;
        public Image background;
        public Text label;
        public Text costText;
        public string actionType;
        public int actionIndex;
    }

    void Awake()
    {
        Debug.Log("[ActionBarUI] v1.1-FINAL barra autocontenida.");
        if (TooltipUI.Instance == null) new GameObject("TooltipUI").AddComponent<TooltipUI>();
        Build();
    }

    void Build()
    {
        actionBarRoot = new GameObject("ActionBarCanvas");
        Canvas canvas = actionBarRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 55;
        actionBarRoot.AddComponent<GraphicRaycaster>();

        RectTransform panel = UIFactory.CreatePanel(actionBarRoot.transform, "ActionBarPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 20), new Vector2(830, 70), new Color(0.05f, 0.05f, 0.08f, 0.9f));

        for (int i = 0; i < 9; i++)
        {
            float x = -360 + i * 90;
            int captured = i;
            slotX.Add(x);

            GameObject btnObj = new GameObject("Slot_" + i);
            btnObj.transform.SetParent(panel, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0);
            rt.sizeDelta = new Vector2(80, 60);
            slotRects.Add(rt);

            Image bg = btnObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.Square();
            bg.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnActionButtonClicked(captured));

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            RectTransform lrt = labelObj.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0.5f);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(4, 0);
            lrt.offsetMax = new Vector2(-4, -4);
            Text label = labelObj.AddComponent<Text>();
            label.font = GetFont();
            label.fontSize = 11;
            label.alignment = TextAnchor.UpperCenter;
            label.color = Color.white;

            GameObject costObj = new GameObject("Cost");
            costObj.transform.SetParent(btnObj.transform, false);
            RectTransform crt = costObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(1, 0.5f);
            crt.offsetMin = new Vector2(4, 4);
            crt.offsetMax = new Vector2(-4, 0);
            Text cost = costObj.AddComponent<Text>();
            cost.font = GetFont();
            cost.fontSize = 10;
            cost.alignment = TextAnchor.LowerCenter;
            cost.color = Color.cyan;

            actionButtons.Add(new ActionButton
            {
                button = btnObj,
                background = bg,
                label = label,
                costText = cost,
                actionType = "",
                actionIndex = -1
            });
        }

        BuildTip(panel);
        RefreshButtons();
    }

    void BuildTip(RectTransform panel)
    {
        tipRoot = new GameObject("BarTooltip");
        tipRoot.transform.SetParent(panel, false);
        tipRt = tipRoot.AddComponent<RectTransform>();
        tipRt.anchorMin = new Vector2(0.5f, 0f);
        tipRt.anchorMax = new Vector2(0.5f, 0f);
        tipRt.pivot = new Vector2(0.5f, 0f);
        tipRt.sizeDelta = new Vector2(300, 0);

        Image img = tipRoot.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.05f, 0.05f, 0.08f, 0.97f);

        VerticalLayoutGroup vlg = tipRoot.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(8, 8, 8, 8);

        ContentSizeFitter csf = tipRoot.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        tipTitle = MakeTipText("TipTitle", 14, Color.yellow);
        tipDesc = MakeTipText("TipDesc", 11, Color.white);
        tipStats = MakeTipText("TipStats", 11, Color.cyan);

        tipRoot.SetActive(false);
    }

    Text MakeTipText(string name, int size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(tipRoot.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(284, 0);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.UpperLeft;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 284;
        return t;
    }

    void Update()
    {
        if (playerUnit == null)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units)
            {
                if (!u.isEnemy) { playerUnit = u; break; }
            }
        }

        RefreshButtons();

        int hovered = -1;
        for (int i = 0; i < slotRects.Count; i++)
        {
            if (slotRects[i] != null &&
                RectTransformUtility.RectangleContainsScreenPoint(slotRects[i], Input.mousePosition, null))
            {
                hovered = i;
                break;
            }
        }

        if (hovered != lastHovered)
        {
            lastHovered = hovered;
            Debug.Log("[ActionBarUI] hover -> " + hovered);
            if (hovered >= 0) ShowTip(hovered);
            else HideTip();
        }

        for (int i = 0; i < 9; i++)
        {
            if (i == hovered) actionButtons[i].background.color = new Color(0.5f, 0.5f, 0.1f, 0.9f);
        }
    }

    void ShowTip(int index)
    {
        if (tipRoot == null) return;
        ActionButton btn = actionButtons[index];
        Debug.Log("[ActionBarUI] ShowTip ENTER slot " + index + " type=" + btn.actionType);

        string title = "";
        string desc = "";
        StringBuilder sb = new StringBuilder();
        Color titleColor = Color.yellow;

        if (btn.actionType == "skill")
        {
            string id = LoadoutSystem.ActiveId(btn.actionIndex);
            SkillData sk = id != "" ? SkillPool.Get(id) : null;
            SkillMeta meta = id != "" ? SkillPool.Meta(id) : null;
            if (sk == null || meta == null) { HideTip(); return; }
            title = sk.skillName + "  [" + meta.rarity + "]";
            desc = meta.type + " · " + meta.affinity + (string.IsNullOrEmpty(meta.tag) ? "" : " · " + meta.tag) + "\n" + sk.description;
            sb.AppendLine("Coste: " + sk.actionPointCost + " AP · Rango: " + sk.range);
            if (sk.damage > 0) sb.AppendLine("Daño: " + sk.damage + (sk.bonusCrit > 0 ? " (+" + sk.bonusCrit + "% crit)" : ""));
            if (meta.heal > 0) sb.AppendLine("Curación: " + meta.heal);
            sb.Append(LoadoutSystem.IsLearned(id) ? "Aprendida (" + meta.origin + ")" : "Aprender: " + meta.cost + " oro");
        }
        else if (btn.actionType == "ultimate")
        {
            SkillData ult = LoadoutSystem.GetUltimate();
            if (ult == null) { HideTip(); return; }
            CombatController cc = Object.FindAnyObjectByType<CombatController>();
            int cd = cc != null ? cc.UltimateCooldown : 0;
            string uid = LoadoutSystem.UltimateId();
            SkillMeta meta = uid != "" ? SkillPool.Meta(uid) : null;

            titleColor = Color.magenta;
            title = "ULTIMATE: " + ult.skillName;
            desc = ult.description;
            sb.AppendLine("Cooldown: " + (meta != null ? meta.cooldown : 3) + " turnos");
            sb.AppendLine(cd > 0 ? "Recarga: " + cd + " turnos restantes" : "LISTO PARA USAR");
            sb.AppendLine("Daño: " + ult.damage + " · Rango: " + ult.range);
        }
        else if (btn.actionType == "consumable")
        {
            ConsumableType t = (ConsumableType)btn.actionIndex;
            int count = InventorySystem.Instance != null ? InventorySystem.Instance.GetConsumableCount(t) : 0;
            title = ConsumableCatalog.Name(t);
            desc = ConsumableCatalog.Description(t);
            sb.Append("Cantidad: " + count);
        }
        else
        {
            HideTip();
            return;
        }

        tipTitle.text = title;
        tipTitle.color = titleColor;
        tipDesc.text = desc;
        tipStats.text = sb.ToString();

        float x = Mathf.Clamp(slotX[index], -265f, 265f);
        tipRt.anchoredPosition = new Vector2(x, 38);
        tipRoot.SetActive(true);
        tipRoot.transform.SetAsLastSibling();
    }

    void HideTip()
    {
        if (tipRoot != null && tipRoot.activeSelf)
            tipRoot.SetActive(false);
    }

    static ConsumableType ConsumableForSlot(int i)
    {
        switch (i)
        {
            case 5: return ConsumableType.PocionHP;
            case 6: return ConsumableType.PocionAP;
            case 7: return ConsumableType.ComidaDano;
            default: return ConsumableType.ComidaDefensa;
        }
    }

    void RefreshButtons()
    {
        CombatController ccArmed = Object.FindAnyObjectByType<CombatController>();
        SkillData armed = ccArmed != null ? ccArmed.GetArmedSkill() : null;

        for (int i = 0; i < 9; i++)
        {
            ActionButton btn = actionButtons[i];
            btn.background.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

            if (i < 4)
            {
                SkillData sk = LoadoutSystem.GetActive(i);
                if (sk != null)
                {
                    btn.label.text = sk.skillName;
                    btn.costText.text = sk.actionPointCost + " AP";
                    btn.actionType = "skill";
                    btn.actionIndex = i;
                    if (armed != null && sk == armed) btn.background.color = new Color(0.1f, 0.5f, 0.1f, 0.9f);
                }
                else
                {
                    btn.label.text = "(vacío)";
                    btn.costText.text = "";
                    btn.actionType = "";
                    btn.actionIndex = -1;
                }
            }
            else if (i == 4)
            {
                SkillData ult = LoadoutSystem.GetUltimate();
                CombatController cc = Object.FindAnyObjectByType<CombatController>();
                int cd = cc != null ? cc.UltimateCooldown : 0;

                if (ult != null)
                {
                    btn.label.text = "ULT: " + ult.skillName;
                    btn.costText.text = (cd > 0) ? "CD: " + cd : "LISTO";
                    btn.actionType = "ultimate";
                    btn.actionIndex = 4;
                    if (cd > 0) btn.background.color = new Color(0.4f, 0.2f, 0.2f, 0.9f);
                    if (armed != null && ult == armed) btn.background.color = new Color(0.1f, 0.5f, 0.1f, 0.9f);
                }
                else
                {
                    btn.label.text = "ULT: (vacío)";
                    btn.costText.text = "";
                    btn.actionType = "";
                    btn.actionIndex = -1;
                }
            }
            else
            {
                ConsumableType t = ConsumableForSlot(i);
                int count = InventorySystem.Instance != null ? InventorySystem.Instance.GetConsumableCount(t) : 0;
                btn.label.text = ConsumableCatalog.Name(t);
                btn.costText.text = "x" + count;
                btn.actionType = "consumable";
                btn.actionIndex = (int)t;
            }
        }
    }

    void OnActionButtonClicked(int index)
    {
        if (TurnManager.Instance == null) return;
        if (playerUnit == null || !TurnManager.Instance.IsPlayerTurn()) return;

        ActionButton btn = actionButtons[index];
        CombatController cc = Object.FindAnyObjectByType<CombatController>();
        if (cc == null) return;

        switch (btn.actionType)
        {
            case "skill":
                {
                    SkillData sk = LoadoutSystem.GetActive(btn.actionIndex);
                    if (sk != null && playerUnit.currentAP >= sk.actionPointCost)
                        cc.ToggleSkill(sk);
                }
                break;

            case "ultimate":
                {
                    if (cc.UltimateCooldown == 0)
                        cc.TryUseUltimate();
                    else
                        Debug.Log("Ultimate en cooldown: " + cc.UltimateCooldown + " turnos.");
                }
                break;

            case "consumable":
                if (InventorySystem.Instance != null)
                    InventorySystem.Instance.UseConsumable((ConsumableType)btn.actionIndex);
                break;
        }
    }

    Font GetFont()
    {
        return UIFactory.GetFont();
    }
}