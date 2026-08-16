using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public int gridWidth = 10;
    public int gridHeight = 10;

    private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

    private static readonly Color TileDark = new Color(0.13f, 0.12f, 0.12f);
    private static readonly Color TileLight = new Color(0.22f, 0.20f, 0.18f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GameObject tileObj = new GameObject("Tile_" + x + "_" + y);
                tileObj.transform.position = new Vector3(x, y, 0);

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = ArtProvider.Get((x + y) % 2 == 0 ? "tileA" : "tileB");
                sr.sortingOrder = 0;

                Tile tile = tileObj.AddComponent<Tile>();
                tile.Init(pos, (x + y) % 2 == 0 ? TileDark : TileLight);

                tiles[pos] = tile;
            }
        }
    }

    public Tile GetTile(Vector2Int cell)
    {
        return tiles.TryGetValue(cell, out Tile t) ? t : null;
    }

    public bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
    }

    public Vector3 GetWorldPosition(Vector2Int cell)
    {
        return new Vector3(cell.x, cell.y, 0);
    }
}