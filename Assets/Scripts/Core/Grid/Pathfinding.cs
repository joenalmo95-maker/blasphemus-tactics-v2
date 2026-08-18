using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    // 1.1-E.5: 8 direcciones (4 ortogonales + 4 diagonales)
    static readonly Vector2Int[] dirs = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, int maxRange)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == target) break;

            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (!GridManager.Instance.InBounds(next)) continue;
                if (!TerrainMap.IsWalkable(next)) continue;
                if (IsOccupied(next) && next != target) continue;

                // 1.1-E.5: anti-corner-cutting para diagonales
                if (d.x != 0 && d.y != 0)
                {
                    Vector2Int ortho1 = current + new Vector2Int(d.x, 0);
                    Vector2Int ortho2 = current + new Vector2Int(0, d.y);
                    if (!TerrainMap.IsWalkable(ortho1) || !TerrainMap.IsWalkable(ortho2)) continue;
                }

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

    // 1.1-E.5: celdas alcanzables desde start con hasta maxRange pasos (para highlights)
    public static HashSet<Vector2Int> GetReachableCells(Vector2Int start, int maxRange)
    {
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        distance[start] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distance[current];

            if (currentDist > 0) reachable.Add(current);
            if (currentDist >= maxRange) continue;

            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (!GridManager.Instance.InBounds(next)) continue;
                if (!TerrainMap.IsWalkable(next)) continue;
                if (IsOccupied(next)) continue;

                // Anti-corner-cutting para diagonales
                if (d.x != 0 && d.y != 0)
                {
                    Vector2Int ortho1 = current + new Vector2Int(d.x, 0);
                    Vector2Int ortho2 = current + new Vector2Int(0, d.y);
                    if (!TerrainMap.IsWalkable(ortho1) || !TerrainMap.IsWalkable(ortho2)) continue;
                }

                if (!distance.ContainsKey(next))
                {
                    distance[next] = currentDist + 1;
                    queue.Enqueue(next);
                }
            }
        }

        return reachable;
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

    // 1.1-E.5: distancia Chebyshev (la diagonal cuenta como 1)
    public static int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
}