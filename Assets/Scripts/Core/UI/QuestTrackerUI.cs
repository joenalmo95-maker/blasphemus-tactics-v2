using UnityEngine;
using UnityEngine.UI;
using System.Text;

// 2.2: seguimiento en pantalla de misiones ACEPTADAS (tecla J, en cualquier escena)
public class QuestTrackerUI : MonoBehaviour
{
    private GameObject root;
    private Text bodyText;
    private float refreshTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Object.FindAnyObjectByType<QuestTrackerUI>() == null)
            new GameObject("QuestTrackerUI").AddComponent<QuestTrackerUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (root == null) Open(); else Close();
        }

        if (root != null)
        {
            QuestSystem.Tick(); // expira contratos con timer
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 1f) { refreshTimer = 0f; Refresh(); }
        }
    }

    void Open()
    {
        if (root != null) return;
        root = new GameObject("QuestTrackerCanvas");
        Canvas c = root.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 60;
        root.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(1, 1);
        prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(1, 1);
        prt.anchoredPosition = new Vector2(-10, -60);
        prt.sizeDelta = new Vector2(480, 320);
        Image img = panel.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.03f, 0.03f, 0.05f, 0.88f);

        GameObject txtObj = new GameObject("Body");
        txtObj.transform.SetParent(panel.transform, false);
        RectTransform brt = txtObj.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(12, 12);
        brt.offsetMax = new Vector2(-12, -12);
        bodyText = txtObj.AddComponent<Text>();
        bodyText.font = GetFont();
        bodyText.fontSize = 13;
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.color = Color.white;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;

        Refresh();
    }

    void Close()
    {
        if (root != null) Destroy(root);
        root = null;
        bodyText = null;
    }

    void Refresh()
    {
        if (bodyText == null) return;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>MISIONES ACEPTADAS</b>  (J para cerrar)");
        sb.AppendLine();
        int count = 0;
        foreach (QuestState q in QuestSystem.Actives())
        {
            if (!q.accepted || q.claimed) continue;
            QuestDef d = QuestSystem.GetDef(q.id);
            if (d == null) continue;
            count++;
            sb.Append("• " + d.description + "  [" + q.progress + "/" + d.target + "]");
            if (q.expiry > 0)
            {
                sb.Append("  <color=#ffcc44>(" + QuestSystem.MinutesLeft(q.id) + " min)</color>");
            }
            sb.AppendLine();
        }
        if (count == 0)
        {
            sb.AppendLine("Sin misiones aceptadas.");
            sb.AppendLine("Visita el Tablón de Misiones en la ciudad (Q o E junto al NPC cian).");
        }
        bodyText.text = sb.ToString();
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}