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

    public StatBlock stats = new StatBlock();
    public float threat = 0f;

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

    public static Unit Create(string name, Vector2Int cell, bool isEnemy, Color color,
        float scale = 0.8f, int maxHealth = 10, int maxAP = 3)
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
        unit.maxHealth = maxHealth;
        unit.currentHealth = maxHealth;
        unit.maxAP = maxAP;
        unit.currentAP = maxAP;

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

    public void ReceiveAttack(Unit attacker, int rawDamage)
    {
        int atk = attacker != null ? attacker.stats.attack : 70;
        int crit = attacker != null ? attacker.stats.critChance : 5;
        int lifesteal = attacker != null ? attacker.stats.lifesteal : 0;
        float threatMult = attacker != null ? attacker.stats.threatMult : 1f;

        int hitChance = Mathf.Clamp(atk - stats.evasion, 5, 95);
        if (Random.Range(0, 100) >= hitChance)
        {
            Debug.Log(gameObject.name + " esquivó el ataque de " + (attacker != null ? attacker.name : "???"));
            CombatFeedback.SpawnText(transform.position, "FALLÓ", Color.gray);
            return;
        }

        bool isCrit = Random.Range(0, 100) < crit;
        int mitigated = Mathf.Max(1, rawDamage - stats.defense);
        int final = isCrit ? mitigated * 2 : mitigated;

        currentHealth -= final;
        Debug.Log(gameObject.name + " recibió " + final + (isCrit ? " (CRÍTICO)" : "") + " de daño. HP: " + currentHealth);
        CombatFeedback.SpawnText(transform.position, (isCrit ? "CRIT -" : "-") + final, isCrit ? Color.yellow : Color.red);
        CombatFeedback.SpawnImpact(transform.position, isEnemy ? Color.yellow : Color.red);
        StartCoroutine(Flash());

        if (attacker != null)
        {
            attacker.threat += final * threatMult;

            if (lifesteal > 0)
            {
                int heal = Mathf.Max(1, Mathf.RoundToInt(final * lifesteal / 100f));
                attacker.Heal(heal);
            }
        }

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " ha sido derrotado.");
            bool wasEnemy = isEnemy;
            Destroy(gameObject);
            if (TurnManager.Instance != null) TurnManager.Instance.NotifyUnitDeath(wasEnemy);
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int real = currentHealth - before;
        if (real > 0)
        {
            Debug.Log(gameObject.name + " se cura " + real + ". HP: " + currentHealth);
            CombatFeedback.SpawnText(transform.position, "+" + real, Color.green);
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