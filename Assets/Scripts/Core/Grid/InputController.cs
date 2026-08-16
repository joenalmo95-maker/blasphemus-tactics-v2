using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputController : MonoBehaviour
{
    private Tile currentHovered;
    private Tile selected;
    private Unit playerUnit;
    private bool isMoving = false;

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
        if (isMoving) return;
        if (InventoryUI.IsOpen || ShopUI.IsOpen) return;
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

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (hit != null && Input.GetMouseButtonDown(0) && !overUI)
        {
            if (selected != null && selected != hit) selected.Highlight(false);
            selected = hit;
            selected.Highlight(true);

            if (playerUnit != null && playerUnit.currentAP > 0 && !Pathfinding.IsOccupied(cell))
            {
                List<Vector2Int> path = Pathfinding.FindPath(
                    playerUnit.currentGridPos,
                    cell,
                    playerUnit.currentAP);

                if (path != null)
                {
                    StartCoroutine(MoveAlongPath(path));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndPlayerTurn();
            }
        }
    }

    System.Collections.IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isMoving = true;
        foreach (Vector2Int step in path)
        {
            Vector3 targetPos = GridManager.Instance.GetWorldPosition(step);
            while (Vector3.Distance(playerUnit.transform.position, targetPos) > 0.05f)
            {
                playerUnit.transform.position = Vector3.MoveTowards(
                    playerUnit.transform.position,
                    targetPos,
                    5f * Time.deltaTime);
                yield return null;
            }
            playerUnit.transform.position = targetPos;
        }

        playerUnit.currentGridPos = selected.gridPosition;
        playerUnit.currentAP -= path.Count;
        Debug.Log("Movimiento completado. AP restantes: " + playerUnit.currentAP);
        isMoving = false;
    }
}