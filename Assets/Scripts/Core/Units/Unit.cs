using UnityEngine;

public class Unit : MonoBehaviour
{
    public int currentAP = 3;
    public int maxAP = 3;
    public int currentHealth = 10;
    public int maxHealth = 10;
    public Vector2Int currentGridPos;
    public bool isEnemy = false;
    public SpriteRenderer spriteRenderer;

    public static Unit Create(string name, Vector2Int cell, bool isEnemy, Color color, float scale = 0.8f)
    {
        GameObject go = new GameObject(name);
        go.transform.position = new Vector3(cell.x, cell.y, 0);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle();
        sr.color = color;
        sr.sortingOrder = 2;

        Unit unit = go.AddComponent<Unit>();
        unit.isEnemy = isEnemy;
        unit.currentGridPos = cell;
        unit.spriteRenderer = sr;

        if (!isEnemy)
        {
            go.AddComponent<SelectionIndicator>();
        }

        return unit;
    }

    public void ResetAP()
    {
        currentAP = maxAP;
    }
}