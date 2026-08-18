using UnityEngine;
using UnityEngine.UI;

// 2.1: NPC "Tablón de Misiones".
// Spawn perezoso junto al Mercader (funciona en cualquier arquitectura de escena)
// y tecla Q como acceso directo dentro de la ciudad.
public class QuestNPC : MonoBehaviour
{
    private GameObject promptCanvas;
    private Transform player;
    private bool ready;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Object.FindAnyObjectByType<QuestNPC>() != null) return;
        new GameObject("QuestNPC").AddComponent<QuestNPC>();
    }

    void Update()
    {
        // 1) Spawn perezoso: en cuanto exista el Mercader, estamos en ciudad
        if (!ready)
        {
            MerchantNPC merchant = Object.FindAnyObjectByType<MerchantNPC>();
            if (merchant == null) return; // no estamos en ciudad todavía

            ready = true;
            transform.position = merchant.transform.position + new Vector3(2f, 0, 0);
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ArtProvider.Get("capitan");
            sr.color = Color.cyan;
            sr.sortingOrder = 2;
            BuildPrompt();
            Debug.Log("[QuestNPC] Tablón de Misiones creado junto al mercader.");
        }

        // 2) Detección del jugador con fallbacks (sin depender de un controlador concreto)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) p = GameObject.Find("CityPlayer");
            if (p == null) p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        bool near = player != null && Vector2.Distance(player.position, transform.position) <= 1.6f;
        if (promptCanvas != null) promptCanvas.SetActive(near && !QuestUI.IsOpen);

        if (near && Input.GetKeyDown(KeyCode.E)) QuestUI.Toggle();

        // 3) Acceso directo en ciudad: Q abre/cierra el tablón
        if (Input.GetKeyDown(KeyCode.Q)) QuestUI.Toggle();
    }

    void BuildPrompt()
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
        rt.sizeDelta = new Vector2(700, 30);
        Text t = txtObj.AddComponent<Text>();
        t.text = "Pulsa E (o Q): Tablón de Misiones";
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.yellow;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.font = f;
        t.fontSize = 16;
        promptCanvas.SetActive(false);
    }
}