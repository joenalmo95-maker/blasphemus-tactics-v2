using UnityEngine;
using UnityEngine.UI;

public class CityPlayerController : MonoBehaviour
{
    public float moveCooldown = 0.15f;
    private float lastMoveTime = -1f;
    private Text promptText;

    void Awake()
    {
        BuildPrompt();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("CityPromptCanvas", 40);
        promptText = UIFactory.CreateText(canvas.transform, "Prompt", "", 18, TextAnchor.MiddleCenter, Color.yellow,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 120), new Vector2(700, 40));
    }

    void Update()
    {
        // 1.1-E.5: movimiento discreto 8-direccional (WASD combinado)
        if (Time.time - lastMoveTime >= moveCooldown)
        {
            Vector2Int dir = Vector2Int.zero;
            if (Input.GetKey(KeyCode.W)) dir.y += 1;
            if (Input.GetKey(KeyCode.S)) dir.y -= 1;
            if (Input.GetKey(KeyCode.A)) dir.x -= 1;
            if (Input.GetKey(KeyCode.D)) dir.x += 1;

            if (dir != Vector2Int.zero)
            {
                Vector2Int currentCell = new Vector2Int(Mathf.RoundToInt(transform.position.x),
                                                         Mathf.RoundToInt(transform.position.y));
                Vector2Int targetCell = currentCell + dir;

                // Anti-corner-cutting para diagonales en ciudad
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int ortho1 = currentCell + new Vector2Int(dir.x, 0);
                    Vector2Int ortho2 = currentCell + new Vector2Int(0, dir.y);
                    if (!TerrainMap.IsWalkable(ortho1) && !TerrainMap.IsWalkable(ortho2))
                    {
                        if (TerrainMap.IsWalkable(ortho1)) targetCell = ortho1;
                        else if (TerrainMap.IsWalkable(ortho2)) targetCell = ortho2;
                        else targetCell = currentCell;
                    }
                }

                if (targetCell != currentCell &&
                    targetCell.x >= 0 && targetCell.y >= 0 &&
                    targetCell.x < CityBootstrap.CityWidth && targetCell.y < CityBootstrap.CityHeight &&
                    TerrainMap.IsWalkable(targetCell))
                {
                    transform.position = new Vector3(targetCell.x, targetCell.y, 0);
                    lastMoveTime = Time.time;
                }
            }
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