using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MerchantUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static MerchantUI Instance { get; private set; }

    private GameObject root;
    private static readonly List<ItemData> equipmentStock = new List<ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    // Renovado al entrar a la ciudad (CityBootstrap.Awake)
    // 1.1b-fix: Renovado cada 1 hora real (persistente con PlayerPrefs)
    public static void RefreshStock()
    {
        // Check de reset por timestamp (1 hora = 3600 segundos)
        long lastRefresh = long.Parse(PlayerPrefs.GetString("CityMerchantLastRefresh", "0"));
        long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        bool needsRefresh = (currentTime - lastRefresh) >= 3600;
        
        // Si ya hay stock y no ha pasado 1 hora, no regenerar
        if (equipmentStock.Count > 0 && !needsRefresh) return;
        
        equipmentStock.Clear();
        ClassData cd = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
        
        // Generar 5 items con probabilidades específicas
        for (int i = 0; i < 5; i++)
        {
            int roll = Random.Range(0, 100);
            ItemData item = null;
            
            if (roll < 1) // 1% espadón épico
            {
                item = ItemGenerator.GenerateEspadon(Rarity.Epic);
                Debug.Log("[MerchantUI] ★ ESPADÓN ÉPICO en stock");
            }
            else if (roll < 2) // 1% armadura épica
            {
                item = ItemGenerator.GenerateWithRarity(cd, Rarity.Epic);
                Debug.Log("[MerchantUI] ★ ARMADURA ÉPICA en stock");
            }
            else // 98% aleatorio (solo Common/Rare, SIN épico)
            {
                Rarity rar = Random.Range(0, 100) < 70 ? Rarity.Common : Rarity.Rare;
                item = ItemGenerator.GenerateWithRarity(cd, rar);
            }
            
            if (item != null) equipmentStock.Add(item);
        }
        
        // Guardar timestamp de refresh
        PlayerPrefs.SetString("CityMerchantLastRefresh", currentTime.ToString());
        PlayerPrefs.Save();
        Debug.Log("[MerchantUI] Stock renovado. Próximo reset en 1 hora.");
    }
    public static void Toggle()
    {
        if (Instance == null) new GameObject("MerchantUI").AddComponent<MerchantUI>();
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

        root = new GameObject("MerchantCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
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

        int gold = CharacterData.Instance != null ? CharacterData.Instance.gold : 0;

        MakeText(root.transform, "MERCADER DE LA CRUZADA  (ESC cerrar)", 0, 260, 20, Color.white);
        MakeText(root.transform, "Oro: " + gold, 0, 228, 16, Color.yellow);

        // --- COMPRA: CONSUMIBLES ---
        MakeText(root.transform, "COMPRAR: CONSUMIBLES", -260, 190, 16, Color.white);
        float cy = 150;
        foreach (ConsumableType t in System.Enum.GetValues(typeof(ConsumableType)))
        {
            ConsumableType ct = t;
            int price = ConsumableCatalog.Price(t);
            GameObject cb = MakeButton(root.transform, ConsumableCatalog.Name(t) + " - " + price + " oro", -260, cy, 320, 34, Color.white,
                () => BuyConsumable(ct));
            ConsumableTooltipTrigger ctt = cb.AddComponent<ConsumableTooltipTrigger>();
            ctt.type = ct;
            cy -= 40;
        }

        // --- COMPRA: EQUIPO (stock de ciudad) ---
        MakeText(root.transform, "COMPRAR: EQUIPO", 80, 190, 16, Color.white);
        float ey = 150;
        for (int i = 0; i < equipmentStock.Count; i++)
        {
            ItemData item = equipmentStock[i];
            int idx = i;
            int price = ItemGenerator.BuyPrice(item.rarity);
            GameObject eb = MakeButton(root.transform, item.itemName + " [" + item.rarity + "] - " + price + " oro", 80, ey, 360, 34,
                ItemGenerator.RarityColor(item.rarity), () => BuyEquipment(idx));
            ItemTooltipTrigger itt = eb.AddComponent<ItemTooltipTrigger>();
            itt.item = item;
            itt.compareWithEquipped = false;
            ey -= 40;
        }

        // --- VENTA: LOOT DE MOCHILA ---
        MakeText(root.transform, "VENDER LOOT", 80, ey - 10, 16, Color.white);
        float sy = ey - 45;
        int count = 0;
        if (InventorySystem.Instance != null)
        {
            for (int i = 0; i < InventorySystem.Instance.items.Count && count < 4; i++)
            {
                ItemData item = InventorySystem.Instance.items[i];
                int idx = i;
                int price = ItemGenerator.SellPrice(item);
            GameObject vb = MakeButton(root.transform, "Vender " + item.itemName + " +" + price + " oro", 80, sy, 360, 34, Color.yellow,
                () => { InventorySystem.Instance.SellItem(idx); Rebuild(); });
            ItemTooltipTrigger vtt = vb.AddComponent<ItemTooltipTrigger>();
            vtt.item = item;
            vtt.compareWithEquipped = false;
                sy -= 40;
                count++;
            }
        }

        MakeButton(root.transform, "CERRAR", 0, -240, 200, 40, Color.red, () => Close());
    }

    void BuyConsumable(ConsumableType t)
    {
        if (CharacterData.Instance == null || InventorySystem.Instance == null) return;
        int price = ConsumableCatalog.Price(t);
        if (CharacterData.Instance.gold < price) { Debug.Log("Oro insuficiente."); return; }
        CharacterData.Instance.gold -= price;
        AddConsumable(t);
        Debug.Log("Comprado: " + ConsumableCatalog.Name(t));
        Rebuild();
    }

    void BuyEquipment(int stockIndex)
    {
        if (CharacterData.Instance == null || InventorySystem.Instance == null) return;
        if (stockIndex < 0 || stockIndex >= equipmentStock.Count) return;
        ItemData item = equipmentStock[stockIndex];
        int price = ItemGenerator.BuyPrice(item.rarity);
        if (CharacterData.Instance.gold < price) { Debug.Log("Oro insuficiente."); return; }
        CharacterData.Instance.gold -= price;
        InventorySystem.Instance.AddItem(item);
        equipmentStock.RemoveAt(stockIndex);
        Debug.Log("Comprado: " + item.itemName);
        Rebuild();
    }

    void AddConsumable(ConsumableType t)
    {
        var inv = InventorySystem.Instance;
        foreach (var c in inv.consumables)
        {
            if (c.type == t) { c.count++; return; }
        }
        inv.consumables.Add(new ConsumableData { type = t, count = 1 });
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
        t.fontSize = 14;
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
        return UIFactory.GetFont();
    }
}

// 1.1-D.4: tooltip de consumibles para el mercader
public class ConsumableTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ConsumableType type;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            int count = InventorySystem.Instance != null ? InventorySystem.Instance.GetConsumableCount(type) : 0;
            TooltipUI.Instance.ShowConsumableTooltip(type, count);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
    }
}