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

    private Unit selfUnit;
    private Unit targetUnit;
    private int chargeCooldown = 0;

    void Awake()
    {
        selfUnit = GetComponent<Unit>();
        if (selfUnit != null)
        {
            selfUnit.stats.attack = 70;
            selfUnit.stats.evasion = 5;
            selfUnit.stats.defense = 1;
            selfUnit.stats.critChance = 5;
            selfUnit.stats.lifesteal = 0;
            selfUnit.stats.threatMult = 1f;
        }
    }

    void Start()
    {
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

        int distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);

        if (distance <= attackRange)
        {
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

                    Debug.Log(gameObject.name + " ¡CARGA contra el Renacido!");
                    chargeCooldown = 3;
                    yield return new WaitForSeconds(0.2f);
                    Attack(1);
                    yield return new WaitForSeconds(0.5f);
                    yield break;
                }
            }
        }

        List<Vector2Int> walkPath = Pathfinding.FindPath(
            selfUnit.currentGridPos, targetUnit.currentGridPos, 99);

        if (walkPath != null && walkPath.Count > 0)
        {
            int stepsToTake = Mathf.Min(walkPath.Count, moveRange);

            if (walkPath[walkPath.Count - 1] == targetUnit.currentGridPos)
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
            }
        }

        distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);
        if (distance <= attackRange)
        {
            yield return new WaitForSeconds(0.25f);
            Attack(0);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Attack(int bonus)
    {
        Debug.Log(gameObject.name + " ataca al jugador. Daño base: " + (attackDamage + bonus));
        bool hit = targetUnit.ReceiveAttack(selfUnit, attackDamage + bonus);
        if (hit && applyCurse)
        {
            targetUnit.ApplyDebuff(10, 2);
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
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}