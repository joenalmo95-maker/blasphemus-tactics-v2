using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WarehouseUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static WarehouseUI Instance { get; private set; }

    private GameObject root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public static void Toggle()
    {
        if (WarehouseSystem.Instance == null)
            new GameObject("WarehouseSystem").AddComponent<WarehouseSystem>();

        if (Instance == null)
            new GameObject("WarehouseUI").AddComponent<WarehouseUI>();

        IsOpen = !IsOpen;
        if (IsOpen) Instance.Rebuild(); else Instance.Close();
    }

    void Close()
    {
        IsOpen = false;
        if (root != null) Destroy(root);
        root = null;
    }

    void Rebuild()
    {
        if (root != null) Destroy(root);

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        root = new GameObject("WarehouseCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 96;
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

        MakeText(root.transform, "ALMACÉN DE LA CRUZADA  (ESC cerrar)", 0, 260, 20, Color.white);

        // --- MOCHILA (depositar) ---
        MakeText(root.transform, "MOCHILA (depositar)", -260, 190, 16, Color.white);
        float iy = 150;
        int count = 0;
        if (InventorySystem.Instance != null)
        {
            for (int i = 0; i < InventorySystem.Instance.items.Count && count < 10; i++)
            {
                ItemData item = InventorySystem.Instance.items[i];
                int idx = i;
                GameObject mb = MakeButton(root.transform, item.itemName + " [" + item.rarity + "]", -380, iy, 260, 34,
                    ItemGenerator.RarityColor(item.rarity), () => Deposit(idx));
                ItemTooltipTrigger mit = mb.AddComponent<ItemTooltipTrigger>();
                mit.item = item;
                mit.compareWithEquipped = false;
                MakeButton(root.transform, "Depositar", -105, iy, 90, 34, Color.cyan, () => Deposit(idx));
                iy -= 40;
                count++;
            }
        }

        // --- ALMACÉN (retirar) ---
        MakeText(root.transform, "ALMACÉN (retirar)", 200, 190, 16, Color.white);
        float sy = 150;
        count = 0;
        if (WarehouseSystem.Instance != null)
        {
            for (int i = 0; i < WarehouseSystem.Instance.stored.Count && count < 10; i++)
            {
                ItemData item = WarehouseSystem.Instance.stored[i];
                int idx = i;
                GameObject sb = MakeButton(root.transform, item.itemName + " [" + item.rarity + "]", 80, sy, 260, 34,
                    ItemGenerator.RarityColor(item.rarity), () => Withdraw(idx));
                ItemTooltipTrigger sit = sb.AddComponent<ItemTooltipTrigger>();
                sit.item = item;
                sit.compareWithEquipped = false;
                MakeButton(root.transform, "Retirar", 355, sy, 90, 34, Color.yellow, () => Withdraw(idx));
                sy -= 40;
                count++;
            }
        }

        MakeButton(root.transform, "CERRAR", 0, -240, 200, 40, Color.red, () => Close());
    }

    void Deposit(int idx)
    {
        var inv = InventorySystem.Instance;
        var wh = WarehouseSystem.Instance;
        if (inv == null || wh == null) return;
        if (idx < 0 || idx >= inv.items.Count) return;

        ItemData item = inv.items[idx];
        inv.items.RemoveAt(idx);
        wh.stored.Add(item);
        wh.Save();
        Debug.Log("Depositado: " + item.itemName);
        Rebuild();
    }

    void Withdraw(int idx)
    {
        var inv = InventorySystem.Instance;
        var wh = WarehouseSystem.Instance;
        if (inv == null || wh == null) return;
        if (idx < 0 || idx >= wh.stored.Count) return;

        ItemData item = wh.stored[idx];
        wh.stored.RemoveAt(idx);
        inv.AddItem(item);
        wh.Save();
        Debug.Log("Retirado: " + item.itemName);
        Rebuild();
    }

    GameObject MakeButton(Transform parent, string label, float x, float y, float w, float h, Color textColor,
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
        t.fontSize = 13;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = textColor;
        btn.onClick.AddListener(onClick);
        return go;
    }

    void MakeText(Transform parent, string content, float x, float y, int size, Color color)
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
        t.color = color;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}