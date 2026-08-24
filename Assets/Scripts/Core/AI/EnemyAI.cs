using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public int attackRange = 1;
    public int attackDamage = 2;
    public int moveRange = 2;
    public EnemyTier tier = EnemyTier.Basico;
    public bool applyCurse = false;
    public bool canCharge = false;

    // === NUEVOS CAMPOS (Fase A) ===
    public EnemyBehavior behavior = EnemyBehavior.Normal;
    public int baseDefense = 0;
    public string unitName = "";
    public bool applyBleed = false; // 0.8-fix: Cruzado (sangrado)

    private Unit selfUnit;
    private Unit targetUnit;
    private int chargeCooldown = 0;

    void Awake()
    {
        selfUnit = GetComponent<Unit>();
    }

    void OnDestroy()
    {
        if (selfUnit != null && behavior == EnemyBehavior.ExplodeOnDeath)
        {
            selfUnit.onDeath -= OnExplodeDeath;
        }
    }

    // 0.8-fix: Start corre DESPUÉS de que EnemyFactory asigna behavior/baseDefense.
    // Por eso los stats y el hook de muerte van aquí (en Awake no funcionaban).
    void Start()
    {
        if (selfUnit != null)
        {
            selfUnit.stats.accuracy = 70;
            selfUnit.stats.evasion = 5;
            selfUnit.stats.defense = 1 + baseDefense; // ← Autómata ahora sí recibe +5 DEF
            selfUnit.stats.critChance = 5;
            selfUnit.stats.lifesteal = 0;
            selfUnit.stats.threatMult = 1f;

            // Hook de muerte para Penitente de la Ceniza (ahora sí se registra)
            if (behavior == EnemyBehavior.ExplodeOnDeath)
            {
                selfUnit.onDeath += OnExplodeDeath;
            }
        }
        targetUnit = FindTarget();
    }

    Unit FindTarget()
    {
        Unit best = null;
        float bestThreat = float.MinValue;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u.isEnemy) continue;
            if (u.threat > bestThreat)
            {
                bestThreat = u.threat;
                best = u;
            }
        }
        if (best == null)
        {
            foreach (Unit u in units)
            {
                if (!u.isEnemy) return u;
            }
        }
        return best;
    }

    public IEnumerator ExecuteTurn()
    {
        if (chargeCooldown > 0) chargeCooldown--;
        if (targetUnit == null)
        {
            targetUnit = FindTarget();
            if (targetUnit == null) yield break;
        }

        // === HEALER: cura aliados antes de actuar ===
        if (behavior == EnemyBehavior.Healer)
        {
            yield return TryHealAlly();
        }

        int distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);

        // === 0.8-fix RANGED: kiting, el Censor retrocede si te acercas ===
        if (behavior == EnemyBehavior.Ranged && distance <= 2)
        {
            Vector2Int away = selfUnit.currentGridPos + new Vector2Int(
                Mathf.Clamp(selfUnit.currentGridPos.x - targetUnit.currentGridPos.x, -1, 1),
                Mathf.Clamp(selfUnit.currentGridPos.y - targetUnit.currentGridPos.y, -1, 1));
            if (Pathfinding.IsFreeCell(away))
            {
                yield return MoveToPosition(GridManager.Instance.GetWorldPosition(away), away);
                distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);
                Debug.Log(unitName + " mantiene la distancia (kiting).");
            }
        }

        // === 0.8-fix BACKSTABBER: el Heraldo rodea activamente para buscar tu espalda ===
        if (behavior == EnemyBehavior.Backstabber && distance > attackRange)
        {
            Vector2Int behind = targetUnit.currentGridPos - new Vector2Int(
                Mathf.RoundToInt(targetUnit.facing.x), Mathf.RoundToInt(targetUnit.facing.y));
            yield return MoveTowardsCell(behind, moveRange);
            distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);
            if (distance <= attackRange)
            {
                FaceTarget();
                yield return new WaitForSeconds(0.25f);
                Attack(0);
                yield return new WaitForSeconds(0.5f);
            }
            yield break;
        }

        // === RANGED: ataca a distancia sin moverse ===
        if (behavior == EnemyBehavior.Ranged && distance <= attackRange)
        {
            FaceTarget();
            yield return new WaitForSeconds(0.25f);
            Attack(0);
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        if (distance <= attackRange && behavior != EnemyBehavior.Ranged)
        {
            FaceTarget();
            yield return new WaitForSeconds(0.25f);
            Attack(0);
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        if (canCharge && chargeCooldown == 0 && distance >= 2 && distance <= 4)
        {
            List<Vector2Int> path = Pathfinding.FindPath(
                selfUnit.currentGridPos, targetUnit.currentGridPos, 99);
            if (path != null && path.Count > 0)
            {
                int steps = Mathf.Min(path.Count - 1, 4);
                while (steps > 0 && Pathfinding.UnitAt(path[steps - 1]) != null) steps--;
                if (steps > 0)
                {
                    Vector2Int dest = path[steps - 1];
                    Vector3 wp = GridManager.Instance.GetWorldPosition(dest);
                    while (Vector3.Distance(transform.position, wp) > 0.05f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, wp, 8f * Time.deltaTime);
                        yield return null;
                    }
                    transform.position = wp;
                    selfUnit.currentGridPos = dest;
                    FaceTarget();
                    Debug.Log(unitName + " ¡CARGA contra el Renacido!");
                    chargeCooldown = 3;
                    yield return new WaitForSeconds(0.2f);
                    Attack(1);
                    yield return new WaitForSeconds(0.5f);
                    yield break;
                }
            }
        }

        // Ranged no se mueve para perseguir, solo reposiciona si está muy lejos
        if (behavior == EnemyBehavior.Ranged && distance > attackRange + 2)
        {
            yield return MoveTowardsTarget(2);
        }
        else if (behavior != EnemyBehavior.Ranged)
        {
            yield return MoveTowardsTarget(moveRange);
        }

        distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);
        if (distance <= attackRange)
        {
            FaceTarget();
            yield return new WaitForSeconds(0.25f);
            Attack(0);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MoveTowardsTarget(int maxSteps)
    {
        yield return MoveTowardsCell(targetUnit.currentGridPos, maxSteps);
    }

    // 0.8-fix: movimiento hacia celda objetivo (kiting y backstab)
    IEnumerator MoveTowardsCell(Vector2Int goal, int maxSteps)
    {
        List<Vector2Int> walkPath = Pathfinding.FindPath(
            selfUnit.currentGridPos, goal, 99);
        if (walkPath != null && walkPath.Count > 0)
        {
            int stepsToTake = Mathf.Min(walkPath.Count, maxSteps);
            if (walkPath[walkPath.Count - 1] == goal)
            {
                stepsToTake = Mathf.Min(stepsToTake, walkPath.Count - 1);
            }
            Vector2Int destination = selfUnit.currentGridPos;
            while (stepsToTake > 0)
            {
                Vector2Int candidate = walkPath[stepsToTake - 1];
                if (!IsOccupiedByOther(candidate))
                {
                    destination = candidate;
                    break;
                }
                stepsToTake--;
            }
            if (destination != selfUnit.currentGridPos)
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(destination);
                yield return MoveToPosition(worldPos, destination);
                FaceTarget();
            }
        }
    }

    void FaceTarget()
    {
        if (selfUnit != null && targetUnit != null)
        {
            selfUnit.UpdateFacing(new Vector2(targetUnit.currentGridPos.x - selfUnit.currentGridPos.x,
                                              targetUnit.currentGridPos.y - selfUnit.currentGridPos.y).normalized);
        }
    }

    void Attack(int bonus)
    {
        int finalDamage = attackDamage + bonus;

        // === SELF DAMAGE: Flagelante se auto-hiere ===
        if (behavior == EnemyBehavior.SelfDamage)
        {
            finalDamage += 15;
            Debug.Log(unitName + " se flagela (-5 HP) y ataca con +15 daño!");
            selfUnit.TakeRawDamage(5, "autoflagelación");
            if (selfUnit == null || selfUnit.currentHealth <= 0)
            {
                Debug.Log(unitName + " muere por su propio castigo.");
                return;
            }
        }

        // === BACKSTABBER: Heraldo Ciego crítico desde atrás ===
        if (behavior == EnemyBehavior.Backstabber && IsAttackingFromBehind())
        {
            finalDamage *= 2;
            Debug.Log(unitName + " ¡ATAQUE POR LA ESPALDA! Daño duplicado: " + finalDamage);
        }

        Debug.Log(unitName + " ataca al jugador. Daño: " + finalDamage);
        bool hit = targetUnit.ReceiveAttack(selfUnit, finalDamage);
        if (hit && applyCurse)
        {
            targetUnit.ApplyDebuff(10, 2);
        }

        // 0.8-fix: Cruzado deja sangrado pequeño (3 HP/turno, 2 turnos)
        if (hit && applyBleed)
        {
            targetUnit.ApplyDot(3, 2, "SANGRADO");
        }

        // 0.8-fix: Censor reduce tu defensa al impactar (-5 DEF, 2 turnos)
        if (hit && behavior == EnemyBehavior.Ranged)
        {
            targetUnit.ApplyDefenseDebuff(5, 2);
        }

        // 0.8-fix: Flagelante bloquea 1 habilidad de Valerius por 1 turno
        if (hit && behavior == EnemyBehavior.SelfDamage && CharacterData.Instance != null)
        {
            int slot = Random.Range(0, 4);
            CharacterData.Instance.BlockSkillSlot(slot, 1);
            CombatFeedback.SpawnText(targetUnit.transform.position, "HABILIDAD " + (slot + 1) + " BLOQUEADA", Color.magenta);
            Debug.Log(unitName + " bloquea la habilidad " + (slot + 1) + " de Valerius este turno.");
        }
    }

    bool IsAttackingFromBehind()
    {
        if (targetUnit == null || selfUnit == null) return false;
        Vector2 facing = targetUnit.facing;
        if (facing.sqrMagnitude < 0.01f) return false;
        Vector2 attackDir = (selfUnit.currentGridPos - targetUnit.currentGridPos);
        attackDir = attackDir.normalized;
        // Está detrás si el producto punto con el facing del jugador es negativo y grande
        float dot = Vector2.Dot(facing, attackDir);
        return dot < -0.5f;
    }

    IEnumerator TryHealAlly()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        Unit wounded = null;
        int maxWound = 0;
        foreach (Unit u in units)
        {
            if (!u.isEnemy || u == selfUnit) continue;
            int d = Dist(selfUnit.currentGridPos, u.currentGridPos);
            if (d > 2) continue;
            int wound = u.maxHealth - u.currentHealth;
            if (wound > maxWound)
            {
                maxWound = wound;
                wounded = u;
            }
        }
        // 0.8-fix: cura 20 HP a cualquier aliado herido (antes exigía herida >= 10)
        if (wounded != null && maxWound > 0)
        {
            int healAmount = Mathf.Min(20, maxWound);
            wounded.currentHealth = Mathf.Min(wounded.maxHealth, wounded.currentHealth + healAmount);
            CombatFeedback.SpawnText(wounded.transform.position, "+" + healAmount, Color.green);
            Debug.Log(unitName + " cura " + healAmount + " HP a " + wounded.name + ".");
            yield return new WaitForSeconds(0.4f);
        }
    }

    void OnExplodeDeath()
    {
        Debug.Log(unitName + " ¡EXPLOTA EN CENIZAS!");
        Vector2Int center = selfUnit.currentGridPos;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u == selfUnit) continue;
            int d = Dist(center, u.currentGridPos);
            if (d <= 1)
            {
                int dmg = u.isEnemy ? 10 : 20; // menos daño a aliados
                u.TakeRawDamage(dmg, "explosión de ceniza");
                // 0.8-fix: deja QUEMADURA 5 HP/turno por 2 turnos
                if (!u.isEnemy) u.ApplyDot(5, 2, "QUEMADURA");
            }
        }
    }

    bool IsOccupiedByOther(Vector2Int pos)
    {
        Unit u = Pathfinding.UnitAt(pos);
        return u != null && u != selfUnit;
    }

    IEnumerator MoveToPosition(Vector3 targetPos, Vector2Int gridPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        selfUnit.currentGridPos = gridPos;
    }

    int Dist(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
}