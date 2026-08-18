using UnityEngine;
using UnityEngine.UI;

public class WorldPlayerController : MonoBehaviour
{
    public float speed = 3.0f;

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
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;

        if (dir != Vector3.zero)
        {
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            Vector2Int targetCell = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            if (TerrainMap.IsWalkable(targetCell))
                transform.position = newPos;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        CheckZones();
    }

    // 2.2: detecta encuentros cercanos (emboscadas, tesoros, santuarios, mercaderes, cazadores)
    void CheckEncounters()
    {
        // El WorldEncounterManager maneja su propio prompt y detección
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