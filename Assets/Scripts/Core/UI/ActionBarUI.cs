using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionBarUI : MonoBehaviour
{
    private GameObject actionBarRoot;
    private readonly List<ActionButton> actionButtons = new List<ActionButton>();
    private readonly List<RectTransform> slotRects = new List<RectTransform>();
    private Unit playerUnit;
    private int lastHovered = -1;
    private bool eventSystemChecked;

    struct ActionButton
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
        Debug.Log("[ActionBarUI] v1.1-D.3 hover manual activo.");
        if (TooltipUI.Instance == null)
        {
            new GameObject("TooltipUI").AddComponent<TooltipUI>();
        }
        Build();
    }

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void Build()
    {
        EnsureEventSystem();

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

        RefreshButtons();
    }

    void Update()
    {
        if (!eventSystemChecked)
        {
            eventSystemChecked = true;
            EnsureEventSystem();
        }

        if (playerUnit == null)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units)
            {
                if (!u.isEnemy) { playerUnit = u; break; }
            }
        }

        RefreshButtons();
        UpdateHover();
    }

    // 1.1-D.3: hover manual por geometría de pantalla (no depende de EventSystem)
    void UpdateHover()
    {
        // Si hay una UI modal abierta, la barra no roba tooltips
        if (InventoryUI.IsOpen || ShopUI.IsOpen || TrainerUI.IsOpen || DungeonCardUI.IsOpen)
        {
            if (lastHovered != -1)
            {
                lastHovered = -1;
                if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
            }
            return;
        }

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
            if (hovered >= 0) OnPointerEnterButton(hovered);
            else if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
        }
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
                int cd = (cc != null) ? cc.UltimateCooldown : 0;

                if (ult != null)
                {
                    btn.label.text = "ULT: " + ult.skillName;
                    btn.costText.text = (cd > 0) ? "CD: " + cd : "LISTO";
                    btn.actionType = "ultimate";
                    btn.actionIndex = 4;
                    if (cd > 0) btn.background.color = new Color(0.4f, 0.2f, 0.2f, 0.9f);
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

    public void OnPointerEnterButton(int index)
    {
        if (TooltipUI.Instance == null) return;
        ActionButton btn = actionButtons[index];

        switch (btn.actionType)
        {
            case "skill":
                {
                    string id = LoadoutSystem.ActiveId(btn.actionIndex);
                    if (id != "") TooltipUI.Instance.ShowPoolSkillTooltip(id);
                }
                break;

            case "ultimate":
                {
                    SkillData ult = LoadoutSystem.GetUltimate();
                    if (ult != null)
                    {
                        CombatController cc = Object.FindAnyObjectByType<CombatController>();
                        int cd = (cc != null) ? cc.UltimateCooldown : 0;
                        TooltipUI.Instance.ShowUltimateTooltip(ult, cd);
                    }
                }
                break;

            case "consumable":
                {
                    ConsumableType ctype = (ConsumableType)btn.actionIndex;
                    int count = InventorySystem.Instance != null ? InventorySystem.Instance.GetConsumableCount(ctype) : 0;
                    TooltipUI.Instance.ShowConsumableTooltip(ctype, count);
                }
                break;
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
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}