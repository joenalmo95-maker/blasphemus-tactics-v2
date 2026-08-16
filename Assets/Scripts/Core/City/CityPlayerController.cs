using UnityEngine;
using UnityEngine.UI;

public class CityPlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Text promptText;

    void Awake() { BuildPrompt(); }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("CityPromptCanvas", 40);
        promptText = UIFactory.CreateText(canvas.transform, "Prompt", "", 18, TextAnchor.MiddleCenter, Color.yellow,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 120), new Vector2(700, 40));
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;

        if (dir != Vector3.zero)
        {
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            Vector2Int targetCell = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            if (targetCell.x >= 0 && targetCell.y >= 0 &&
                targetCell.x < CityBootstrap.CityWidth && targetCell.y < CityBootstrap.CityHeight &&
                TerrainMap.IsWalkable(targetCell))
                transform.position = newPos;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, CityBootstrap.CityWidth - 1),
            Mathf.Clamp(transform.position.y, 0, CityBootstrap.CityHeight - 1),
            0);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (Mathf.Abs(myCell.x - CityBootstrap.ExitPortal.x) <= 1 && Mathf.Abs(myCell.y - CityBootstrap.ExitPortal.y) <= 1)
        {
            promptText.text = "Pulsa E para salir al mundo";
        }
        else
        {
            promptText.text = "CIUDAD (próximamente: mercader, almacén, teleport, entrenador)";
        }
    }
}