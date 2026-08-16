using UnityEngine;
using System.Collections.Generic;

public enum TerrainType { Caminable, Roca, Agua, Ruinas }

public static class TerrainMap
{
    private static Dictionary<Vector2Int, TerrainType> map = new Dictionary<Vector2Int, TerrainType>();

    public static void Clear()
    {
        map.Clear();
    }

    public static void Set(Vector2Int cell, TerrainType type)
    {
        map[cell] = type;
    }

    public static TerrainType Get(Vector2Int cell)
    {
        return map.TryGetValue(cell, out TerrainType t) ? t : TerrainType.Caminable;
    }

    public static bool IsWalkable(Vector2Int cell)
    {
        TerrainType t = Get(cell);
        return t == TerrainType.Caminable;
    }

    // Generación determinista de obstáculos para combate (5-8 por escena)
    public static void GenerateCombatObstacles(int gridWidth, int gridHeight, Vector2Int playerCell, Vector2Int enemyCell)
    {
        Clear();
        int obstacleCount = Random.Range(5, 9);
        int attempts = 0;
        
        while (obstacleCount > 0 && attempts < 100)
        {
            attempts++;
            Vector2Int cell = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
            
            // No bloquear celda del jugador ni del enemigo
            if (cell == playerCell || cell == enemyCell) continue;
            if (!IsWalkable(cell)) continue;
            
            TerrainType type = Random.Range(0, 3) == 0 ? TerrainType.Ruinas : TerrainType.Roca;
            Set(cell, type);
            obstacleCount--;
        }
    }

    // Generación determinista de obstáculos para mundo (patrón fijo)
    public static void GenerateWorldObstacles(int worldWidth, int worldHeight)
    {
        Clear();
        
        // Rocas dispersas
        Set(new Vector2Int(5, 3), TerrainType.Roca);
        Set(new Vector2Int(12, 8), TerrainType.Roca);
        Set(new Vector2Int(18, 15), TerrainType.Roca);
        Set(new Vector2Int(25, 10), TerrainType.Roca);
        Set(new Vector2Int(8, 18), TerrainType.Roca);
        
        // Agua (charcos)
        Set(new Vector2Int(10, 5), TerrainType.Agua);
        Set(new Vector2Int(11, 5), TerrainType.Agua);
        Set(new Vector2Int(10, 6), TerrainType.Agua);
        Set(new Vector2Int(22, 12), TerrainType.Agua);
        Set(new Vector2Int(23, 12), TerrainType.Agua);
        
        // Ruinas
        Set(new Vector2Int(20, 10), TerrainType.Ruinas);
        Set(new Vector2Int(7, 14), TerrainType.Ruinas);
        Set(new Vector2Int(15, 2), TerrainType.Ruinas);
    }
}