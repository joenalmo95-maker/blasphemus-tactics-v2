using UnityEngine;
using UnityEngine.UI;

// 2.1: NPC "Tablón de Misiones" en ciudad (auto-spawn, no toca CityBootstrap)
public class QuestNPC : MonoBehaviour
{
    private GameObject promptCanvas;
    private Transform player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "CityScene") return;
        if (Object.FindAnyObjectByType<QuestNPC>() != null) return;
        GameObject go = new GameObject("QuestNPC");
        QuestNPC npc = go.AddComponent<QuestNPC>();
        go.transform.position = new Vector3(4, 2, 0);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get("capitan");
        sr.color = Color.cyan;
        sr.sortingOrder = 2;
    }

    void Start()
    {
        promptCanvas = new GameObject("QuestPromptCanvas");
        Canvas c = promptCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 40;
        GameObject txtObj = new GameObject("Prompt");
        txtObj.transform.SetParent(promptCanvas.transform, false);
        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 120);
        rt.sizeDelta = new Vector2(600, 30);
        Text t = txtObj.AddComponent<Text>();
        t.text = "Pulsa E: Tablón de Misiones";
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.yellow;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = 16;
        promptCanvas.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            CityPlayerController c = Object.FindAnyObjectByType<CityPlayerController>();
            if (c != null) player = c.transform;
        }

        bool near = player != null && Vector2.Distance(player.position, transform.position) <= 1.6f;
        if (promptCanvas != null) promptCanvas.SetActive(near && !QuestUI.IsOpen);

        if (near && Input.GetKeyDown(KeyCode.E))
        {
            QuestUI.Toggle();
        }
    }
}