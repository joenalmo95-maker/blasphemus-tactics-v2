using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    private GameObject root;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) Toggle();
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

        root = new GameObject("ShopCanvas");
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

        MakeText(root.transform, "TIENDA DEL RENEGADO  (B para cerrar)", 0, 220, 22);

        int gold = CharacterData.Instance != null ? CharacterData.Instance.gold : 0;
        MakeText(root.transform, "Oro disponible: " + gold, 0, 180, 16);

        float y = 120;
        foreach (ConsumableType t in System.Enum.GetValues(typeof(ConsumableType)))
        {
            ConsumableType captured = t;
            int count = InventorySystem.Instance != null ? InventorySystem.Instance.GetConsumableCount(t) : 0;
            string label = ConsumableCatalog.Name(t) + " [" + count + "]  " +
                           ConsumableCatalog.Price(t) + " oro — " + ConsumableCatalog.Description(t);
            MakeButton(root.transform, label, 0, y, 680, 40, Color.white, () => Buy(captured));
            y -= 50;
        }
    }

    void Buy(ConsumableType t)
    {
        if (CharacterData.Instance == null) return;
        int price = ConsumableCatalog.Price(t);

        if (CharacterData.Instance.gold < price)
        {
            Debug.Log("Oro insuficiente para " + ConsumableCatalog.Name(t) + ".");
            return;
        }

        CharacterData.Instance.gold -= price;
        if (InventorySystem.Instance != null) InventorySystem.Instance.AddConsumable(t);
        Debug.Log("Comprado: " + ConsumableCatalog.Name(t) + " (-" + price + " oro)");
        Rebuild();
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