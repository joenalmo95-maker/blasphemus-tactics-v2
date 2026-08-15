using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public int currentAP = 3;
    public int maxAP = 3;
    public int currentHealth = 10;
    public int maxHealth = 10;
    public Vector2Int currentGridPos;
    public bool isEnemy = false;
    public SpriteRenderer spriteRenderer;

    private Color originalColor;
    private bool colorCached = false;

    void CacheColor()
    {
        if (!colorCached && spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            colorCached = true;
        }
    }

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

    public void TakeDamage(int amount)
    {
        CacheColor();
        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. HP: " + currentHealth);

        CombatFeedback.SpawnDamage(transform.position, amount);
        CombatFeedback.SpawnImpact(transform.position, isEnemy ? Color.yellow : Color.red);
        StartCoroutine(Flash());

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " ha sido derrotado.");
            bool wasEnemy = isEnemy;
            Destroy(gameObject);
            if (TurnManager.Instance != null) TurnManager.Instance.NotifyUnitDeath(wasEnemy);
        }
    }

    IEnumerator Flash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
        }
    }
}