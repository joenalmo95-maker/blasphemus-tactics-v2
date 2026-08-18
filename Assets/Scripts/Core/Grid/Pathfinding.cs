using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    // 1.1-E.5: pathfinding 8-direccional con anti-corner-cutting
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, int maxRange)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        // 8 direcciones: 4 ortogonales + 4 diagonales
        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),  // horizontal
            new Vector2Int(0, 1), new Vector2Int(0, -1),  // vertical
            new Vector2Int(1, 1), new Vector2Int(1, -1),  // diagonales
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

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
                    if (!TerrainMap.IsWalkable(ortho1) && !TerrainMap.IsWalkable(ortho2))
                        continue; // ambas ortogonales bloqueadas = no cortar esquina
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

    // 1.1-E.5: devuelve todas las celdas alcanzables desde start con maxRange pasos (para highlight)
    public static List<Vector2Int> GetReachableCells(Vector2Int start, int maxRange)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        dist[start] = 0;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int d = dist[current];
            if (d > 0) reachable.Add(current);
            if (d >= maxRange) continue;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;
                if (!GridManager.Instance.InBounds(next)) continue;
                if (!TerrainMap.IsWalkable(next)) continue;
                if (IsOccupied(next)) continue;

                // Anti-corner-cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int ortho1 = current + new Vector2Int(dir.x, 0);
                    Vector2Int ortho2 = current + new Vector2Int(0, dir.y);
                    if (!TerrainMap.IsWalkable(ortho1) && !TerrainMap.IsWalkable(ortho2))
                        continue;
                }

                if (!dist.ContainsKey(next))
                {
                    dist[next] = d + 1;
                    queue.Enqueue(next);
                }
            }
        }

        return reachable;
    }
}