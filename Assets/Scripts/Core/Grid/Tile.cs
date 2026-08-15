using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    private SpriteRenderer sr;
    private Color originalColor;

    public void Init(Vector2Int pos, Color color)
    {
        gridPosition = pos;
        sr = GetComponent<SpriteRenderer>();
        sr.color = color;
        originalColor = color;
    }

    public void Highlight(bool active)
    {
        if (sr == null) return;
        sr.color = active ? Color.yellow : originalColor;
    }
}