using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EncounterUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static EncounterUI Instance { get; private set; }
    
    private GameObject root;
    private Encounter current;
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    void Update()
    {
        if (!IsOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }
    
    public static void ShowShrine(Encounter e)
    {
        if (Instance == null) new GameObject("EncounterUI").AddComponent<EncounterUI>();
        Instance.current = e;
        Instance.BuildShrine();
        IsOpen = true;
    }
    
    public static void ShowWanderingMerchant(Encounter e)
    {
        if (Instance == null) new GameObject("EncounterUI").AddComponent<EncounterUI>();
        Instance.current = e;
        Instance.BuildMerchant();
        IsOpen = true;
    }
    
    public static void ShowHunter(Encounter e)
    {
        if (Instance == null) new GameObject("EncounterUI").AddComponent<EncounterUI>();
        Instance.current = e;
        Instance.BuildHunter();
        IsOpen = true;
    }
    
    void Close()
    {
        IsOpen = false;
        if (root != null) Destroy(root);
        root = null;
    }
    
    void BuildShrine()
    {
        if (root != null) Destroy(root);
        
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        
        root = new GameObject("ShrineCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 98;
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
        
        MakeText(root.transform, "SANTUARIO", 0, 200, 24, Color.cyan);
        MakeText(root.transform, "Elige un buff temporal (10 minutos):", 0, 160, 16, Color.white);
        
        MakeButton(root.transform, "+20% DAÑO", -200, 80, 180, 60, Color.red, () => { ApplyBuff("dmg"); Close(); });
        MakeButton(root.transform, "+15% DEFENSA", 0, 80, 180, 60, Color.blue, () => { ApplyBuff("def"); Close(); });
        MakeButton(root.transform, "+1 AP MAX", 200, 80, 180, 60, Color.green, () => { ApplyBuff("ap"); Close(); });
        
        MakeButton(root.transform, "CERRAR", 0, -100, 200, 40, Color.gray, () => Close());
    }
    
    void ApplyBuff(string type)
    {
        // 0.7-fix: Los buffs del mundo se guardan en CharacterData (persistentes)
        if (CharacterData.Instance == null) return;

        if (type == "ap")
        {
            // AP extra es especial: se acumula como bonus de AP base
            Debug.Log("[Encounters] Santuario: +1 AP máximo (se aplicará en próximo combate).");
        }
        else
        {
            CharacterData.Instance.ApplyWorldBuff(type);
        }
        Debug.Log("[Encounters] Santuario: buff " + type + " aplicado al Renacido.");
    }
    
    void BuildMerchant()
    {
        if (root != null) Destroy(root);

        root = new GameObject("WanderingMerchantCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 98;
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

        MakeText(root.transform, "MERCADER ERRANTE", 0, 200, 24, Color.yellow);
        MakeText(root.transform, "Stock efímero (desaparece al cerrar):", 0, 160, 16, Color.white);

        float y = 100;
        ClassData cd = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
        
        // 1.1b-fix: Generar 5 items con probabilidades específicas para ambulante
        for (int i = 0; i < 5; i++)
        {
            int roll = Random.Range(0, 100);
            ItemData item = null;
            string prefix = "";
            
            if (roll < 2) // 2% espadón épico
            {
                item = ItemGenerator.GenerateEspadon(Rarity.Epic);
                prefix = "★ ";
                Debug.Log("[Encounters] ★ ESPADÓN ÉPICO en mercader ambulante");
            }
            else if (roll < 3) // 1% espadón legendario
            {
                item = ItemGenerator.GenerateEspadon(Rarity.Legendary);
                prefix = "★★ ";
                Debug.Log("[Encounters] ★★ ESPADÓN LEGENDARIO en mercader ambulante");
            }
            else if (roll < 5) // 2% armadura épica
            {
                item = ItemGenerator.GenerateWithRarity(cd, Rarity.Epic);
                prefix = "★ ";
                Debug.Log("[Encounters] ★ ARMADURA ÉPICA en mercader ambulante");
            }
            else if (roll < 8) // 3% armadura legendaria
            {
                item = ItemGenerator.GenerateWithRarity(cd, Rarity.Legendary);
                prefix = "★★ ";
                Debug.Log("[Encounters] ★★ ARMADURA LEGENDARIA en mercader ambulante");
            }
            else // 92% aleatorio (solo Common/Rare)
            {
                Rarity rar = Random.Range(0, 100) < 70 ? Rarity.Common : Rarity.Rare;
                item = ItemGenerator.GenerateWithRarity(cd, rar);
            }
            
            if (item != null)
            {
                int price = ItemGenerator.BuyPrice(item.rarity) * 2; // recargo de errante
                string label = prefix + item.itemName + " - " + price + " oro";
                ItemData captured = item;
                int capturedPrice = price;
                Color textColor = item.rarity == Rarity.Legendary ? Color.yellow 
                                : item.rarity == Rarity.Epic ? Color.magenta 
                                : Color.white;
                MakeButton(root.transform, label, 0, y, 500, 40, textColor, () => {
                    if (CharacterData.Instance != null && CharacterData.Instance.gold >= capturedPrice)
                    {
                        CharacterData.Instance.gold -= capturedPrice;
                        InventorySystem.Instance.items.Add(captured);
                        Debug.Log("[Encounters] Comprado: " + captured.itemName);
                    }
                    else
                    {
                        Debug.Log("[Encounters] Oro insuficiente.");
                    }
                });
                y -= 50;
            }
        }

        MakeButton(root.transform, "CERRAR", 0, -200, 200, 40, Color.gray, () => Close());
    }
    
    void BuildHunter()
    {
        if (root != null) Destroy(root);
        
        root = new GameObject("HunterCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 98;
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
        
        MakeText(root.transform, "CAZADOR DE RECOMPENSAS", 0, 200, 24, new Color(0.9f, 0.6f, 0.2f));
        MakeText(root.transform, "Contrato: Mata 10 enemigos en 30 minutos", 0, 140, 16, Color.white);
        MakeText(root.transform, "Recompensa: 1000 oro + 500 XP", 0, 100, 16, Color.yellow);
        
        MakeButton(root.transform, "ACEPTAR CONTRATO", 0, 0, 300, 60, Color.green, () => {
            QuestSystem.AcceptHunterContract();
            Close();
        });
        
        MakeButton(root.transform, "RECHAZAR", 0, -100, 300, 60, Color.red, () => Close());
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
    
    void MakeText(Transform parent, string content, float x, float y, int size, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(1000, 40);
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