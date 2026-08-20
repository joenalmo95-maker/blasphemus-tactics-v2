using UnityEngine;
using System.Collections;

// 1.1-E: sector de ataque relativo al facing del objetivo
public enum FlankType { Frontal, Lateral, Espalda }

public class Unit : MonoBehaviour
{
    public int currentAP = 3;
    public int maxAP = 3;
    public int currentHealth = 10;
    public int maxHealth = 10;
    public Vector2Int currentGridPos;
    public bool isEnemy = false;
    [HideInInspector] public bool isElite = false;
    [HideInInspector] public bool isBoss = false;
    public SpriteRenderer spriteRenderer;

    public StatBlock stats = new StatBlock();
    public float threat = 0f;

    public int buffDamage = 0;
    public int buffDefense = 0;
    public int buffCrit = 0;
    public int buffTurns = 0;

    public int debuffAttack = 0;
    public int debuffTurns = 0;

    public int pendingApPenalty = 0;
    // 1.1-E: dirección de mirada e intención telegrafiada

    public Vector2 facing = new Vector2(0, -1);
    public IntentType intent = IntentType.Ninguna;

    // Fase A: hook para comportamientos al morir (Penitente de la Ceniza)
    public System.Action onDeath;

    public static Unit Create(string name, Vector2Int cell, bool isEnemy, Color color,
        float scale = 0.8f, int maxHealth = 10, int maxAP = 3, string artKey = "circle")
    {
        GameObject go = new GameObject(name);
        go.transform.position = new Vector3(cell.x, cell.y, 0);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ArtProvider.Get(artKey);
        sr.color = artKey == "circle" ? color : Color.white;
        sr.sortingOrder = 2;

        Unit unit = go.AddComponent<Unit>();
        unit.isEnemy = isEnemy;
        unit.currentGridPos = cell;
        unit.spriteRenderer = sr;
        unit.maxHealth = maxHealth;
        unit.currentHealth = maxHealth;
        unit.maxAP = maxAP;
        unit.currentAP = maxAP;

     go.AddComponent<HealthBar2D>();
     if (isEnemy)
     {
         go.AddComponent<FacingIndicator>();
     }
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

    public void AddBuff(int dmg, int def, int turns)
    {
        AddBuff(dmg, def, 0, turns);
    }

    public void AddBuff(int dmg, int def, int crit, int turns)
    {
        buffDamage += dmg;
        buffDefense += def;
        buffCrit += crit;
        buffTurns = Mathf.Max(buffTurns, turns);
        Debug.Log(gameObject.name + " recibe buff: +" + dmg + " daño, +" + def + " defensa, +" + crit + " crítico por " + turns + " turnos.");
        CombatFeedback.SpawnText(transform.position, "BUFF", Color.blue);
    }

    public void TickBuffs()
    {
        if (buffTurns > 0)
        {
            buffTurns--;
            if (buffTurns == 0)
            {
                buffDamage = 0;
                buffDefense = 0;
                buffCrit = 0;
                Debug.Log("Los buffs han expirado.");
            }
        }
    }

    public void ApplyDebuff(int attackReduction, int turns)
    {
        debuffAttack += attackReduction;
        debuffTurns = Mathf.Max(debuffTurns, turns);
        Debug.Log(gameObject.name + " sufre MALDICIÓN: -" + attackReduction + " precisión por " + turns + " turnos.");
        CombatFeedback.SpawnText(transform.position, "MALDICIÓN", Color.magenta);
    }

    public void TickDebuffs()
    {
        if (debuffTurns > 0)
        {
            debuffTurns--;
            if (debuffTurns == 0)
            {
                debuffAttack = 0;
                Debug.Log("La maldición se disipa.");
            }
        }
    }

    public bool ReceiveAttack(Unit attacker, int rawDamage, int bonusCrit = 0, float skillThreat = 1f)
    {
        int atk = attacker != null ? attacker.stats.attack - attacker.debuffAttack : 70;
        int crit = (attacker != null ? attacker.stats.critChance + attacker.buffCrit : 5) + bonusCrit;
        int lifesteal = attacker != null ? attacker.stats.lifesteal : 0;
        float threatMult = attacker != null ? attacker.stats.threatMult : 1f;

        int hitChance = Mathf.Clamp(atk - stats.evasion, 5, 95);
        if (Random.Range(0, 100) >= hitChance)
        {
            Debug.Log(gameObject.name + " esquivó el ataque de " + (attacker != null ? attacker.name : "???"));
            CombatFeedback.SpawnText(transform.position, "FALLÓ", Color.gray);
            return false;
        }

        bool isCrit = Random.Range(0, 100) < crit;
        int mitigated = Mathf.Max(1, rawDamage - (stats.defense + buffDefense));
        int final = isCrit ? mitigated * 2 : mitigated;

        currentHealth -= final;
        Debug.Log(gameObject.name + " recibió " + final + (isCrit ? " (CRÍTICO)" : "") + " de daño. HP: " + currentHealth);
        CombatFeedback.SpawnText(transform.position, (isCrit ? "CRIT -" : "-") + final, isCrit ? Color.yellow : Color.red);
        CombatFeedback.SpawnImpact(transform.position, isEnemy ? Color.yellow : Color.red);
        StartCoroutine(Flash());

        if (attacker != null)
        {
            attacker.threat += final * threatMult * skillThreat;

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
            
            // Fase A: disparar hook de muerte ANTES de destruir (para Penitente de la Ceniza)
            if (onDeath != null) onDeath.Invoke();
            
            if (wasEnemy)
            {
                EnemyAI ai = GetComponent<EnemyAI>();
                LootSystem.DropFrom(this, ai != null ? ai.tier : EnemyTier.Basico);
                // 2.1: progreso de misiones
                QuestSystem.NotifyEnemyKilled(isBoss, isElite);
            }
            Destroy(gameObject);
            if (TurnManager.Instance != null) TurnManager.Instance.NotifyUnitDeath(wasEnemy);
        }

        return true;
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
        else
        {
            Debug.Log(gameObject.name + " está al máximo de HP.");
            CombatFeedback.SpawnText(transform.position, "MAX", Color.gray);
        }
    }

    // 1.1-E: actualización de mirada y cálculo de flanking
    public void UpdateFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.001f) facing = dir.normalized;
    }

    public FlankType GetFlankFrom(Unit attacker)
    {
        if (attacker == null) return FlankType.Frontal;
        return GetFlankFromPos(attacker.currentGridPos);
    }

    public FlankType GetFlankFromPos(Vector2Int attackerCell)
    {
        Vector2 toAtt = new Vector2(attackerCell.x - currentGridPos.x, attackerCell.y - currentGridPos.y);
        if (toAtt.sqrMagnitude < 0.001f) return FlankType.Frontal;
        float ang = Mathf.Abs(Vector2.SignedAngle(facing, toAtt));
        if (ang <= 60f) return FlankType.Frontal;
        if (ang <= 120f) return FlankType.Lateral;
        return FlankType.Espalda;
    }

    IEnumerator Flash()
    {
        if (spriteRenderer != null)
        {
            Color current = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = current;
        }
    }

    // 0.3: Metodo estatico para verificar si hay una unidad en una celda
    public static Unit At(Vector2Int cell)
    {
        Unit[] all = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in all)
        {
            if (u.currentGridPos == cell) return u;
        }
        return null;
    }
}

