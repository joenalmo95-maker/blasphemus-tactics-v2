using UnityEngine;
using UnityEngine.UI;

// 2.1: NPC "Tablón de Misiones" (mismo patrón que MerchantNPC)
public class QuestNPC : MonoBehaviour
{
    private Text promptText;

    void Awake()
    {
        GameObject canvas = UIFactory.CreateCanvas("QuestPromptCanvas", 44);
        promptText = UIFactory.CreateText(canvas.transform, "QuestPrompt", "", 16, TextAnchor.MiddleCenter,
            new Color(1f, 0.9f, 0.4f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 140), new Vector2(700, 30));
        Debug.Log("[QuestNPC] Tablón listo.");
    }

    void Update()
    {
        CityPlayerController pc = Object.FindAnyObjectByType<CityPlayerController>();
        if (pc == null)
        {
            if (promptText != null) promptText.text = "";
            return;
        }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));
        Vector2Int npcCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        bool near = Mathf.Abs(myCell.x - npcCell.x) <= 1 && Mathf.Abs(myCell.y - npcCell.y) <= 1;

        if (promptText != null)
            promptText.text = (near && !QuestUI.IsOpen) ? "Pulsa E (o Q): Tablón de Misiones" : "";

        if (near && Input.GetKeyDown(KeyCode.E)) QuestUI.Toggle();
        if (Input.GetKeyDown(KeyCode.Q)) QuestUI.Toggle();
    }
}