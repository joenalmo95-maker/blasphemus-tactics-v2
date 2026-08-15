using UnityEngine;

public class InputController : MonoBehaviour
{
    private Tile currentHovered;
    private Tile selected;
    private Unit playerUnit;

    void Start()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) { playerUnit = u; break; }
        }
    }

    void Update()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        Tile hit = GridManager.Instance != null ? GridManager.Instance.GetTile(cell) : null;

        if (hit != currentHovered)
        {
            if (currentHovered != null && currentHovered != selected) currentHovered.Highlight(false);
            currentHovered = hit;
            if (currentHovered != null && currentHovered != selected) currentHovered.Highlight(true);
        }

        if (hit != null && Input.GetMouseButtonDown(0))
        {
            if (selected != null && selected != hit) selected.Highlight(false);
            selected = hit;
            selected.Highlight(true);
            Debug.Log("Selected: " + cell);
        }
    }
}