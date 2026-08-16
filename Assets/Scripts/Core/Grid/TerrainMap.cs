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
        return Get(cell) == TerrainType.Caminable;
    }

    // Carga el mapa dibujado por el usuario (Tools/World Map Editor).
    // Formato: fila 0 del archivo = borde superior (y máximo). '.' caminable, R roca, W agua, U ruinas.
    public static bool TryLoadWorldMap(int width, int height)
    {
        TextAsset asset = Resources.Load<TextAsset>("WorldMapData");
        if (asset == null) return false;

        Clear();
        string[] lines = asset.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            int y = height - 1 - i;
            if (y < 0) break;
            string line = lines[i].TrimEnd('\r');
            for (int x = 0; x < width && x < line.Length; x++)
            {
                char c = line[x];
                if (c == 'R') Set(new Vector2Int(x, y), TerrainType.Roca);
                else if (c == 'W') Set(new Vector2Int(x, y), TerrainType.Agua);
                else if (c == 'U') Set(new Vector2Int(x, y), TerrainType.Ruinas);
            }
        }
        return true;
    }

    // Generación procedural de respaldo (combate)
    public static void GenerateCombatObstacles(int gridWidth, int gridHeight, Vector2Int playerCell, Vector2Int enemyCell)
    {
        Clear();
        int obstacleCount = Random.Range(5, 9);
        int attempts = 0;

        while (obstacleCount > 0 && attempts < 100)
        {
            attempts++;
            Vector2Int cell = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            if (cell == playerCell || cell == enemyCell) continue;
            if (!IsWalkable(cell)) continue;

            TerrainType type = Random.Range(0, 3) == 0 ? TerrainType.Ruinas : TerrainType.Roca;
            Set(cell, type);
            obstacleCount--;
        }
    }

    // Generación procedural de respaldo (mundo, si no hay mapa dibujado)
    public static void GenerateWorldObstacles(int worldWidth, int worldHeight)
    {
        Clear();

        Set(new Vector2Int(5, 3), TerrainType.Roca);
        Set(new Vector2Int(12, 8), TerrainType.Roca);
        Set(new Vector2Int(18, 15), TerrainType.Roca);
        Set(new Vector2Int(25, 10), TerrainType.Roca);
        Set(new Vector2Int(8, 18), TerrainType.Roca);

        Set(new Vector2Int(10, 5), TerrainType.Agua);
        Set(new Vector2Int(11, 5), TerrainType.Agua);
        Set(new Vector2Int(10, 6), TerrainType.Agua);
        Set(new Vector2Int(22, 12), TerrainType.Agua);
        Set(new Vector2Int(23, 12), TerrainType.Agua);

        Set(new Vector2Int(20, 10), TerrainType.Ruinas);
        Set(new Vector2Int(7, 14), TerrainType.Ruinas);
        Set(new Vector2Int(15, 2), TerrainType.Ruinas);
    }
}