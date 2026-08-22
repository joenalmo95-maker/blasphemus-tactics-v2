using UnityEngine;
using UnityEngine.EventSystems;

// 1.1-E.7: InputController SIN movimiento (el movimiento vive SOLO en CombatController,
// evitando el doble consumo de AP). Conserva: resaltado de tiles y tecla E (fin de turno).
public class InputController : MonoBehaviour
{
    private Tile currentHovered;
    private Tile selected;
    private Unit playerUnit;

    void Start()
    {
        playerUnit = GetPlayer();
    }

    Unit GetPlayer()
    {
        if (playerUnit != null) return playerUnit;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) return u;
        }
        return null;
    }

    void Update()
    {
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn()) return;
        if (playerUnit == null) playerUnit = GetPlayer();

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        Tile hit = GridManager.Instance != null ? GridManager.Instance.GetTile(cell) : null;

        if (hit != currentHovered)
        {
            if (currentHovered != null && currentHovered != selected) currentHovered.Highlight(false);
            currentHovered = hit;
            if (currentHovered != null && currentHovered != selected) currentHovered.Highlight(true);
        }

        // Solo selección visual: el movimiento lo ejecuta CombatController
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (hit != null && Input.GetMouseButtonDown(0) && !overUI)
        {
            if (selected != null && selected != hit) selected.Highlight(false);
            selected = hit;
            selected.Highlight(true);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (TurnManager.Instance != null)
            {
                Debug.Log("[InputController] E presionada. Estado: " + TurnManager.Instance.currentState);
                TurnManager.Instance.EndPlayerTurn();
            }
        }
    }
}