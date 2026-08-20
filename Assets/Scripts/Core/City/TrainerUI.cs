using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TrainerUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static TrainerUI Instance { get; private set; }

    private GameObject root;
    private int tab = 0; // 0 = ENTRENAR DAÑO (legacy 3.5), 1 = SKILLS (1.1-C)
    private SkillType filter = SkillType.Activa;

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
        if (Instance == null) new GameObject("TrainerUI").AddComponent<TrainerUI>();
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

        if (TooltipUI.Instance == null)
            new GameObject("TooltipUI").AddComponent<TooltipUI>();

        root = new GameObject("TrainerCanvas");
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

        int gold = CharacterData.Instance != null ? CharacterData.Instance.gold : 0;
        int level = CharacterData.Instance != null ? CharacterData.Instance.level : 0;

        MakeText(root.transform, "ENTRENADOR DE HABILIDADES  (ESC cerrar)", 0, 260, 20, Color.white);
        MakeText(root.transform, "Oro: " + gold + "   Nivel: " + level, 0, 230, 15, Color.yellow);

        MakeButton(root.transform, tab == 0 ? "> ENTRENAR DAÑO" : "ENTRENAR DAÑO", -120, 200, 200, 32,
            tab == 0 ? Color.green : Color.gray, () => { tab = 0; Rebuild(); });
        MakeButton(root.transform, tab == 1 ? "> SKILLS (LOADOUT)" : "SKILLS (LOADOUT)", 120, 200, 200, 32,
            tab == 1 ? Color.green : Color.gray, () => { tab = 1; Rebuild(); });

        if (tab == 0) BuildTrainTab();
        else BuildSkillsTab();

        MakeButton(root.transform, "CERRAR", 0, -250, 200, 40, Color.red, () => Close());
    }

    // --- PESTAÑA 0: entreno legacy del 3.5 ---
    void BuildTrainTab()
    {
        int level = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        ClassRole role = Role();

        float y = 150;
        for (int slot = 1; slot <= 4; slot++)
        {
            SkillData sk = SkillCatalog.Get(role, slot);
            bool learned = SkillTrainer.IsLearned(slot);
            bool levelOk = level >= sk.unlockLevel;
            int s = slot;

            string info = sk.skillName + "  |  Daño " + sk.damage + "  |  Entreno " + SkillTrainer.TrainLevel(slot) + "/" + SkillTrainer.MaxTrain;
            MakeText(root.transform, info, -120, y, 15, Color.white);

            if (!levelOk)
            {
                MakeText(root.transform, "Requiere nivel " + sk.unlockLevel, 260, y, 14, Color.red);
            }
            else if (!learned)
            {
                MakeButton(root.transform, "Aprender " + SkillTrainer.LearnCost(slot) + " oro", 260, y, 180, 34, Color.cyan,
                    () => { SkillTrainer.TryLearn(s); Rebuild(); });
            }
            else if (SkillTrainer.TrainLevel(slot) < SkillTrainer.MaxTrain)
            {
                MakeButton(root.transform, "Entrenar +1 daño " + SkillTrainer.TrainCost(slot) + " oro", 260, y, 220, 34, Color.green,
                    () => { SkillTrainer.TryTrain(s); Rebuild(); });
            }
            else
            {
                MakeText(root.transform, "ENTRENAMIENTO MÁXIMO", 260, y, 14, Color.green);
            }

            y -= 55;
        }
    }

    // --- PESTAÑA 1: SKILLS (1.1-C + tooltips 1.1-D.1) ---
    void BuildSkillsTab()
    {
        MakeText(root.transform, "LOADOUT ACTUAL", -280, 168, 16, Color.white);

        float ly = 138;
        for (int i = 0; i < 4; i++)
        {
            int s = i;
            string id = LoadoutSystem.ActiveId(i);
            string name = id == "" ? "(vacío)" : SkillPool.Get(id).skillName;
            MakeLoadoutRow("Activa " + (i + 1) + ": " + name, Color.white, id, -280, ly,
                () => { LoadoutSystem.AssignActive(s, ""); Rebuild(); });
            ly -= 32;
        }

        string ultId = LoadoutSystem.UltimateId();
        string ultName = ultId == "" ? "(vacío)" : SkillPool.Get(ultId).skillName;
        MakeLoadoutRow("Ultimate: " + ultName, Color.magenta, ultId, -280, ly,
            () => { LoadoutSystem.AssignUltimate(""); Rebuild(); });
        ly -= 32;

        for (int i = 0; i < 3; i++)
        {
            int s = i;
            string id = LoadoutSystem.PassiveId(i);
            string name = id == "" ? "(vacío)" : SkillPool.Get(id).skillName;
            MakeLoadoutRow("Pasiva " + (i + 1) + ": " + name, Color.cyan, id, -280, ly,
                () => { LoadoutSystem.AssignPassive(s, ""); Rebuild(); });
            ly -= 32;
        }

        MakeText(root.transform, "POOL DE SKILLS", 140, 168, 16, Color.white);
        MakeButton(root.transform, "Activas", 30, 140, 90, 28, filter == SkillType.Activa ? Color.green : Color.gray,
            () => { filter = SkillType.Activa; Rebuild(); });
        MakeButton(root.transform, "Ultimates", 130, 140, 90, 28, filter == SkillType.Ultimate ? Color.green : Color.gray,
            () => { filter = SkillType.Ultimate; Rebuild(); });
        MakeButton(root.transform, "Pasivas", 230, 140, 90, 28, filter == SkillType.Pasiva ? Color.green : Color.gray,
            () => { filter = SkillType.Pasiva; Rebuild(); });

        BuildPoolScroll();
    }

    void MakeLoadoutRow(string label, Color color, string id, float x, float y, UnityEngine.Events.UnityAction onVaciar)
    {
        GameObject row = new GameObject("LoadoutRow");
        row.transform.SetParent(root.transform, false);
        RectTransform rrt = row.AddComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.5f, 0.5f);
        rrt.anchorMax = new Vector2(0.5f, 0.5f);
        rrt.anchoredPosition = new Vector2(x, y);
        rrt.sizeDelta = new Vector2(300, 26);

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(row.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = GetFont();
        t.fontSize = 14;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = color;

        GameObject btn = new GameObject("Vaciar");
        btn.transform.SetParent(row.transform, false);
        RectTransform brt = btn.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = new Vector2(70, 24);
        Image img = btn.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        Button b = btn.AddComponent<Button>();
        b.onClick.AddListener(() => { onVaciar(); });

        GameObject bl = new GameObject("L");
        bl.transform.SetParent(btn.transform, false);
        RectTransform blrt = bl.AddComponent<RectTransform>();
        blrt.anchorMin = Vector2.zero;
        blrt.anchorMax = Vector2.one;
        blrt.offsetMin = Vector2.zero;
        blrt.offsetMax = Vector2.zero;
        Text bt = bl.AddComponent<Text>();
        bt.text = "Vaciar";
        bt.font = GetFont();
        bt.fontSize = 12;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.color = Color.gray;

        if (id != "") AddSkillTooltipTrigger(row, id);
    }

    void BuildPoolScroll()
    {
        GameObject scrollObj = new GameObject("PoolScroll");
        scrollObj.transform.SetParent(root.transform, false);
        RectTransform srt = scrollObj.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0.5f);
        srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(140, -45);
        srt.sizeDelta = new Vector2(470, 330);
        Image simg = scrollObj.AddComponent<Image>();
        simg.color = new Color(0.08f, 0.08f, 0.10f, 0.6f);
        scrollObj.AddComponent<Mask>().showMaskGraphic = true;
        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        RectTransform crt = content.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = crt;

        foreach (string id in SkillPool.AllIds())
        {
            SkillMeta meta = SkillPool.Meta(id);
            if (meta.type != filter) continue;
            MakePoolRow(crt.transform, id, meta);
        }
    }

    void MakePoolRow(Transform parent, string id, SkillMeta meta)
    {
        SkillData sk = SkillPool.Get(id);

        GameObject row = new GameObject("Row_" + id);
        row.transform.SetParent(parent, false);
        RectTransform rrt = row.AddComponent<RectTransform>();
        rrt.sizeDelta = new Vector2(0, 34);
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 34;

        GameObject txtObj = new GameObject("Name");
        txtObj.transform.SetParent(row.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0.5f);
        trt.anchorMax = new Vector2(0, 0.5f);
        trt.pivot = new Vector2(0, 0.5f);
        trt.anchoredPosition = new Vector2(6, 0);
        trt.sizeDelta = new Vector2(230, 30);
        Text t = txtObj.AddComponent<Text>();
        t.text = sk.skillName + (LoadoutSystem.IsLearned(id) ? "" : " · " + meta.cost + " oro");
        t.font = GetFont();
        t.fontSize = 13;
        t.alignment = TextAnchor.MiddleLeft;
        Rarity rar = Rarity.Common;
        if (meta.rarity == "Rare") rar = Rarity.Rare;
        else if (meta.rarity == "Epic") rar = Rarity.Epic;
        else if (meta.rarity == "Legendary") rar = Rarity.Legendary;
        t.color = ItemGenerator.RarityColor(rar);

        float bx = 250;
        if (!LoadoutSystem.IsLearned(id))
        {
            RowButton(row.transform, "APRENDER", bx, 90, Color.cyan, () =>
            {
                if (CharacterData.Instance == null) return;
                if (CharacterData.Instance.gold < meta.cost) { Debug.Log("Oro insuficiente."); return; }
                CharacterData.Instance.gold -= meta.cost;
                LoadoutSystem.Learn(id);
                Debug.Log("Skill aprendida: " + sk.skillName);
                Rebuild();
            });
        }
        else if (meta.type == SkillType.Activa)
        {
            for (int i = 0; i < 4; i++)
            {
                int s = i;
                RowButton(row.transform, "" + (i + 1), bx, 26, LoadoutSystem.ActiveId(i) == id ? Color.green : Color.white,
                    () => { LoadoutSystem.AssignActive(s, id); Rebuild(); });
                bx += 30;
            }
        }
        else if (meta.type == SkillType.Ultimate)
        {
            RowButton(row.transform, "U", bx, 26, LoadoutSystem.UltimateId() == id ? Color.green : Color.white,
                () => { LoadoutSystem.AssignUltimate(id); Rebuild(); });
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                int s = i;
                RowButton(row.transform, "P" + (i + 1), bx, 30, LoadoutSystem.PassiveId(i) == id ? Color.green : Color.white,
                    () =>
                    {
                        if (!LoadoutSystem.AssignPassive(s, id))
                            Debug.Log("No se puede asignar esa pasiva (duplicada o no aprendida).");
                        Rebuild();
                    });
                bx += 34;
            }
        }

        // 1.1-D.1: tooltip rico al pasar el ratón
        AddSkillTooltipTrigger(row, id);
    }

    void AddSkillTooltipTrigger(GameObject target, string id)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((d) => { if (TooltipUI.Instance != null) TooltipUI.Instance.ShowPoolSkillTooltip(id); });
        trigger.triggers.Add(enter);
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((d) => { if (TooltipUI.Instance != null) TooltipUI.Instance.Hide(); });
        trigger.triggers.Add(exit);
    }

    void RowButton(Transform parent, string label, float x, float w, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0);
        rt.sizeDelta = new Vector2(w, 26);
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
        t.fontSize = 12;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        btn.onClick.AddListener(onClick);
    }

    ClassRole Role()
    {
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
            return CharacterData.Instance.classData.role;
        return ClassRole.DPS;
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