using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActionBarUI : MonoBehaviour
{
    private GameObject actionBarRoot;
    private readonly List<ActionButton> actionButtons = new List<ActionButton>();
    private Unit playerUnit;
    private SkillData armedSkill;

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
        Build();
    }

    void Build()
    {
        // FIX: La UI solo se renderiza bajo un Canvas. La barra crea su propio canvas.
        GameObject canvas = UIFactory.CreateCanvas("ActionBarCanvas", 60);

        RectTransform barRt = UIFactory.CreatePanel(canvas.transform, "ActionBar",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 36), new Vector2(560, 84), new Color(0.08f, 0.08f, 0.1f, 0.9f));
        actionBarRoot = barRt.gameObject;

        HorizontalLayoutGroup hlg = actionBarRoot.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(10, 10, 5, 5);

        CreateActionButton("Skill 1", "1", "skill", 1);
        CreateActionButton("Skill 2", "2", "skill", 2);
        CreateActionButton("Utilidad", "3", "utility", 0);
        CreateActionButton("Poc HP", "4", "consumable", (int)ConsumableType.PocionHP);
        CreateActionButton("Poc AP", "5", "consumable", (int)ConsumableType.PocionAP);
        CreateActionButton("Comida", "6", "consumable", (int)ConsumableType.ComidaDano);
    }

    void CreateActionButton(string label, string key, string type, int index)
    {
        ActionButton btn = new ActionButton();
        btn.actionType = type;
        btn.actionIndex = index;

        RectTransform rt = UIFactory.CreatePanel(actionBarRoot.transform, "Slot_" + key,
            null, null, null, null, new Vector2(80, 70), new Color(0.2f, 0.2f, 0.2f, 1f));
        btn.button = rt.gameObject;
        btn.background = rt.GetComponent<Image>();

        Button button = btn.button.AddComponent<Button>();
        button.targetGraphic = btn.background;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
        colors.pressedColor = new Color(0.4f, 0.4f, 0.6f);
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        button.colors = colors;

        UIFactory.CreateText(btn.button.transform, "Key", key, 18, TextAnchor.UpperLeft, Color.yellow,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(5, -5), new Vector2(30, 25));

        btn.label = UIFactory.CreateText(btn.button.transform, "Label", label, 12, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0, 0.3f), new Vector2(1, 0.7f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 30));

        btn.costText = UIFactory.CreateText(btn.button.transform, "Cost", "", 10, TextAnchor.LowerCenter, Color.cyan,
            new Vector2(0, 0), new Vector2(1, 0.3f), new Vector2(0.5f, 0), new Vector2(0, 5), new Vector2(76, 20));

        int buttonIndex = actionButtons.Count;
        button.onClick.AddListener(() => OnActionButtonClicked(buttonIndex));

        actionButtons.Add(btn);
    }

    void OnActionButtonClicked(int index)
    {
        if (playerUnit == null || TurnManager.Instance == null || !TurnManager.Instance.IsPlayerTurn()) return;

        ActionButton btn = actionButtons[index];
        CombatController cc = Object.FindAnyObjectByType<CombatController>();
        if (cc == null) return;

        switch (btn.actionType)
        {
            case "skill":
                SkillData skill = SkillCatalog.Get(GetRole(), btn.actionIndex);
                if (skill != null && playerUnit.currentAP >= skill.actionPointCost) cc.ToggleSkill(skill);
                break;

            case "utility":
                if (playerUnit.currentAP >= 1) cc.TryUtility();
                break;

            case "consumable":
                if (InventorySystem.Instance != null) InventorySystem.Instance.UseConsumable((ConsumableType)btn.actionIndex);
                break;
        }
    }

    ClassRole GetRole()
    {
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
            return CharacterData.Instance.classData.role;
        return ClassRole.DPS;
    }

    void Update()
    {
        if (playerUnit == null)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units) if (!u.isEnemy) { playerUnit = u; break; }
        }

        CombatController cc = Object.FindAnyObjectByType<CombatController>();
        if (cc != null) armedSkill = cc.GetArmedSkill();

        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        if (playerUnit == null || CharacterData.Instance == null) return;

        bool isPlayerTurn = TurnManager.Instance != null && TurnManager.Instance.IsPlayerTurn();

        for (int i = 0; i < actionButtons.Count; i++)
        {
            ActionButton btn = actionButtons[i];
            Button button = btn.button.GetComponent<Button>();
            bool canUse = isPlayerTurn;
            string labelText = "";
            string costText = "";

            switch (btn.actionType)
            {
                case "skill":
                    SkillData skill = SkillCatalog.Get(GetRole(), btn.actionIndex);
                    if (skill != null)
                    {
                        labelText = skill.skillName;
                        costText = skill.actionPointCost + " AP";
                        canUse = canUse && playerUnit.currentAP >= skill.actionPointCost;
                        btn.background.color = (armedSkill == skill)
                            ? new Color(0.3f, 0.55f, 0.3f)
                            : (canUse ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.5f));
                    }
                    break;

                case "utility":
                    switch (GetRole())
                    {
                        case ClassRole.Tank: labelText = "Grito"; break;
                        case ClassRole.Healer: labelText = "Curar"; break;
                        default: labelText = "Ojos"; break;
                    }
                    costText = "1 AP";
                    canUse = canUse && playerUnit.currentAP >= 1;
                    btn.background.color = canUse ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.5f);
                    break;

                case "consumable":
                    ConsumableType ctype = (ConsumableType)btn.actionIndex;
                    int count = GetConsumableCount(ctype);
                    labelText = ConsumableCatalog.Name(ctype);
                    costText = count + "x";
                    canUse = canUse && count > 0;
                    btn.background.color = canUse ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.5f);
                    break;
            }

            button.interactable = canUse;
            if (btn.label != null) btn.label.text = labelText;
            if (btn.costText != null) btn.costText.text = costText;
        }
    }

    int GetConsumableCount(ConsumableType type)
    {
        if (InventorySystem.Instance == null) return 0;
        foreach (var c in InventorySystem.Instance.consumables)
        {
            if (c.type == type) return c.count;
        }
        return 0;
    }
}