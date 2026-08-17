using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TrainerUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static TrainerUI Instance { get; private set; }

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
        ClassRole role = Role();

        MakeText(root.transform, "ENTRENADOR DE HABILIDADES  (ESC cerrar)", 0, 260, 20, Color.white);
        MakeText(root.transform, "Oro: " + gold + "   Nivel: " + level, 0, 228, 16, Color.yellow);

        float y = 170;
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

        MakeButton(root.transform, "CERRAR", 0, -240, 200, 40, Color.red, () => Close());
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