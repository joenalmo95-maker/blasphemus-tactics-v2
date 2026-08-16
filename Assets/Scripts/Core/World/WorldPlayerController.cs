using UnityEngine;
using UnityEngine.UI;

public class WorldPlayerController : MonoBehaviour
{
    public float speed = 5f;

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

        // Movimiento WASD con colisión de terreno (Bloque 2.1)
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
            promptText.text = "Pulsa E para entrar: " + nearZone.name;
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameFlow.EnterCombat(nearZone.tier, nearZone.dungeon);
            }
        }
        else
        {
            promptText.text = "";
        }
    }
}