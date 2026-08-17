using UnityEngine;
using UnityEngine.UI;

public class TeleportNPC : MonoBehaviour
{
    private Text promptText;

    void Awake()
    {
        GameObject canvas = UIFactory.CreateCanvas("TeleportPromptCanvas", 44);
        promptText = UIFactory.CreateText(canvas.transform, "TeleportPrompt", "", 16, TextAnchor.MiddleCenter,
            new Color(0.8f, 0.4f, 1f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 140), new Vector2(500, 30));
    }

    void Update()
    {
        CityPlayerController pc = Object.FindAnyObjectByType<CityPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));
        Vector2Int npcCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        if (Mathf.Abs(myCell.x - npcCell.x) <= 1 && Mathf.Abs(myCell.y - npcCell.y) <= 1)
        {
            promptText.text = "Pulsa E para usar el Teletransporte";
            if (Input.GetKeyDown(KeyCode.E) && !TeleportUI.IsOpen) TeleportUI.Toggle();
        }
        else
        {
            promptText.text = "";
        }
    }
}