using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static QuestUI Instance { get; private set; }

    private GameObject root;
    private int tab = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuestSystem.DebugForceDailyReset();
            DungeonDaily.ResetToday();
            Debug.Log("[QuestUI] DEBUG: reset diario forzado (misiones + mazmorras).");
            Rebuild();
        }
    }

    public static void Toggle()
    {
        if (Instance == null) new GameObject("QuestUI").AddComponent<QuestUI>();
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

        root = new GameObject("QuestCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 97;
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

        CharacterData cd = CharacterData.Instance;
        MakeText(root.transform, "TABLÓN DE MISIONES  (ESC cerrar)", 0, 260, 20, Color.white);
        MakeText(root.transform, "Oro: " + (cd != null ? cd.gold : 0) + "   Nivel: " + (cd != null ? cd.level : 0), 0, 232, 15, Color.yellow);

        MakeButton(root.transform, tab == 0 ? "> DIARIAS" : "DIARIAS", -240, 200, 140, 30, tab == 0 ? Color.green : Color.gray, () => { tab = 0; Rebuild(); });
        MakeButton(root.transform, tab == 1 ? "> SEMANALES" : "SEMANALES", -80, 200, 150, 30, tab == 1 ? Color.green : Color.gray, () => { tab = 1; Rebuild(); });
        MakeButton(root.transform, tab == 2 ? "> TEMPORADA" : "TEMPORADA", 85, 200, 150, 30, tab == 2 ? Color.green : Color.gray, () => { tab = 2; Rebuild(); });
        MakeButton(root.transform, tab == 3 ? "> EVENTOS" : "EVENTOS", 245, 200, 140, 30, tab == 3 ? Color.green : Color.gray, () => { tab = 3; Rebuild(); });

        float y = 150;
        int shown = 0;
        foreach (QuestState q in QuestSystem.Actives())
        {
            QuestDef d = QuestSystem.GetDef(q.id);
            if (d == null) continue;
            if ((int)d.type != tab) continue;
            shown++;

            string extra = "";
            if (d.type == QuestType.Diaria) extra = " · nuevas en " + QuestSystem.HoursLeftDaily() + "h";
            if (d.type == QuestType.Semanal) extra = " · nuevas en " + QuestSystem.HoursLeftWeekly() + "h";
            if (d.type == QuestType.Evento) extra = " · termina en " + QuestSystem.HoursLeftEvent(q.id) + "h";
            if (d.type == QuestType.Temporada) extra = " · fase " + (QuestSystem.SeasonPhase() + 1);

            Color col = q.claimed ? Color.gray : (q.progress >= d.target ? Color.green : Color.white);
            MakeText(root.transform, d.description + "  [" + q.progress + "/" + d.target + "]" + extra +
                "  |  Rec: " + d.gold + " oro + " + d.xp + " XP", -60, y, 14, col);

            string capturedId = q.id;
            if (q.claimed)
            {
                MakeButton(root.transform, "RECLAMADA", 330, y, 110, 28, Color.gray, () => { });
            }
            else if (!q.accepted)
            {
                MakeButton(root.transform, "ACEPTAR", 330, y, 110, 28, Color.cyan, () => { QuestSystem.Accept(capturedId); Rebuild(); });
            }
            else if (q.progress >= d.target)
            {
                MakeButton(root.transform, "RECLAMAR", 330, y, 110, 28, Color.green, () => { QuestSystem.Claim(capturedId); Rebuild(); });
            }
            else
            {
                MakeButton(root.transform, "EN CURSO", 330, y, 110, 28, Color.white, () => { });
            }

            y -= 45;
        }

        if (shown == 0)
        {
            MakeText(root.transform, tab == 3 ? "No hay eventos activos ahora mismo." : "No hay misiones en esta categoría.", 0, 100, 14, Color.gray);
        }

        MakeText(root.transform, "J: seguimiento de misiones aceptadas · DEBUG: F9 fuerza reset diario", 0, -230, 11, new Color(0.5f, 0.5f, 0.5f, 0.6f));
        MakeButton(root.transform, "CERRAR", 0, -260, 200, 40, Color.red, () => Close());
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
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}