using UnityEngine;
using UnityEngine.UI;

public class WorldPlayerController : MonoBehaviour
{
    public float moveCooldown = 0.15f;
    private float lastMoveTime = -1f;
    private Text promptText;
    private WorldBootstrap.ZoneDef nearZone;

    void Awake()
    {
        BuildPrompt();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldPromptCanvas", 40);
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

                // Anti-corner-cutting para diagonales en mundo
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int ortho1 = currentCell + new Vector2Int(dir.x, 0);
                    Vector2Int ortho2 = currentCell + new Vector2Int(0, dir.y);
                    if (!TerrainMap.IsWalkable(ortho1) && !TerrainMap.IsWalkable(ortho2))
                    {
                        // No cortar esquina: intentar solo horizontal o vertical
                        if (TerrainMap.IsWalkable(ortho1)) targetCell = ortho1;
                        else if (TerrainMap.IsWalkable(ortho2)) targetCell = ortho2;
                        else targetCell = currentCell;
                    }
                }

                if (targetCell != currentCell && GridManager.Instance.InBounds(targetCell) &&
                    TerrainMap.IsWalkable(targetCell))
                {
                    transform.position = new Vector3(targetCell.x, targetCell.y, 0);
                    lastMoveTime = Time.time;
                }
            }
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        CheckZones();
    }

    void CheckZones()
    {
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        nearZone = null;
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (Mathf.Abs(z.center.x - myCell.x) <= 1 && Mathf.Abs(z.center.y - myCell.y) <= 1)
            {
                nearZone = z;
                break;
            }
        }
        if (nearZone != null)
        {
            // 5.2: tarjeta previa + límite diario de mazmorras
            promptText.text = "Pulsa E para ver la tarjeta: " + nearZone.name
                              + "  (Mazmorras hoy: " + DungeonDaily.Count + "/" + DungeonDaily.MaxPerDay + ")";
            if (Input.GetKeyDown(KeyCode.E) && !DungeonCardUI.IsOpen)
            {
                if (!DungeonDaily.CanEnter())
                {
                    Debug.Log("Límite diario de mazmorras alcanzado (5/5). Vuelve mañana.");
                }
                else
                {
                    WorldBootstrap.ZoneDef z = nearZone;
                    DungeonCardUI.Show(z, () =>
                    {
                        DungeonDaily.Consume();
                        GameFlow.EnterCombat(z.tier, z.dungeon);
                    });
                }
            }
        }
        else
        {
            promptText.text = "";
        }
    }
}