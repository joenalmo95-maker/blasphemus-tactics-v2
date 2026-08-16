using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    private GameObject root;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSystem.Save();
        }
                
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.C)) Toggle();

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (InventorySystem.Instance != null)
            {
                ClassData cd = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
                InventorySystem.Instance.AddItem(ItemGenerator.Generate(cd));
                if (IsOpen) Rebuild();
            }
        }
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;
        if (IsOpen) Rebuild(); else Close();
    }

    void Close()
    {
        if (root != null) Destroy(root);
        root = null;
    }

    public void Rebuild()
    {
        Close();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        root = new GameObject("InventoryCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        root.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        Image bimg = bg.AddComponent<Image>();
        bimg.sprite = SpriteFactory.Square();
        bimg.color = new Color(0.02f, 0.02f, 0.03f, 0.95f);

        MakeText(root.transform, "INVENTARIO Y EQUIPO  (I/C cerrar, L simular drop)", 0, 260, 20);

        MakeText(root.transform, "EQUIPADO (clic para desequipar)", -260, 190, 16);
        float ey = 150;
        foreach (ItemSlot slot in System.Enum.GetValues(typeof(ItemSlot)))
        {
                        ItemData eq = InventorySystem.Instance != null ? InventorySystem.Instance.GetEquipped(slot) : null;

            string label = SlotLabel(slot) + ": " + (eq != null ? eq.itemName : "---");
            ItemData captured = eq;

            MakeButton(root.transform, label, -260, ey, 320, 34, Color.white, () =>
            {
                if (captured != null && InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.Unequip(captured.slot);
                    Rebuild();
                }
            });
            ey -= 40;
        }

        MakeText(root.transform, "MOCHILA (clic para equipar)", 80, 190, 16);
        float iy = 150;
        int count = 0;
        if (InventorySystem.Instance != null)
        {
            for (int i = 0; i < InventorySystem.Instance.items.Count && count < 8; i++)
            {
                ItemData item = InventorySystem.Instance.items[i];
                int idx = i;

                MakeButton(root.transform, item.itemName + " [" + item.rarity + "]", 80, iy, 360, 34,
                    ItemGenerator.RarityColor(item.rarity), () =>
                {
                    if (InventorySystem.Instance != null)
                    {
                        InventorySystem.Instance.Equip(idx);
                        Rebuild();
                    }
                });
                iy -= 40;
                count++;
            }
        }

        StatBlock stats = CharacterData.Instance != null ? CharacterData.Instance.GetTotalStats() : new StatBlock();

        Unit player = null;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) { player = u; break; }
        }

        string hpText = player != null ? player.currentHealth + "/" + player.maxHealth : stats.maxHP + "/" + stats.maxHP;
        string apText = player != null ? player.currentAP + "/" + player.maxAP : stats.apMove + "/" + stats.apMove;

        string statsText = "HP: " + hpText + "  DEF: " + stats.defense + "  DAÑO: " + stats.damage +
                           "  ATQ: " + stats.attack + "  CRIT: " + stats.critChance + "%  EVA: " + stats.evasion +
                           "%  AP: " + apText + "  CUR: " + stats.healingPower + "%  ROBO: " + stats.lifesteal + "%";
        MakeText(root.transform, statsText, 0, -220, 18);
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

    void MakeButton(Transform parent, string label, float x, float y, float w, float h, Color textColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        Image img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

        Button btn = go.AddComponent<Button>();

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(go.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = GetFont();
        t.fontSize = 14;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = textColor;

        btn.onClick.AddListener(onClick);
    }

    void MakeText(Transform parent, string content, float x, float y, int size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(900, 40);

        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}