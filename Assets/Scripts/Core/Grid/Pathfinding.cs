using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, int maxRange)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == target) break;

            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (!GridManager.Instance.InBounds(next)) continue;
                if (!TerrainMap.IsWalkable(next)) continue; // NUEVO: respetar terreno
                if (IsOccupied(next) && next != target) continue;
                if (!cameFrom.ContainsKey(next))
                {
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        if (!cameFrom.ContainsKey(target)) return null;

        Vector2Int curr = target;
        while (curr != start)
        {
            path.Add(curr);
            curr = cameFrom[curr];
        }
        path.Reverse();

        if (path.Count > maxRange) return null;
        return path;
    }

    public static bool IsOccupied(Vector2Int cell)
    {
        return UnitAt(cell) != null;
    }

    public static Unit UnitAt(Vector2Int cell)
    {
        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u.currentGridPos == cell) return u;
        }
        return null;
    }

        // 1.1-E: celda libre para reposicionamiento (knockback/pull/lunge)
        public static bool IsFreeCell(Vector2Int cell)
        {
            return GridManager.Instance.InBounds(cell) && TerrainMap.IsWalkable(cell) && !IsOccupied(cell);
        }
}