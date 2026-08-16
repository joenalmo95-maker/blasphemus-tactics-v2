using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public int attackRange = 1;
    public int attackDamage = 2;
    public int moveRange = 2;

    private Unit selfUnit;
    private Unit targetUnit;

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
        if (targetUnit == null)
        {
            targetUnit = FindTarget();
            if (targetUnit == null) yield break;
        }

        int distance = CalculateDistance(selfUnit.currentGridPos, targetUnit.currentGridPos);

        if (distance <= attackRange)
        {
            yield return new WaitForSeconds(0.25f);
            Debug.Log(gameObject.name + " ataca al jugador. Daño base: " + attackDamage);
            targetUnit.ReceiveAttack(selfUnit, attackDamage);
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        List<Vector2Int> path = Pathfinding.FindPath(
            selfUnit.currentGridPos,
            targetUnit.currentGridPos,
            99);

        if (path != null && path.Count > 0)
        {
            int stepsToTake = Mathf.Min(path.Count, moveRange);

            if (path[path.Count - 1] == targetUnit.currentGridPos)
            {
                stepsToTake = Mathf.Min(stepsToTake, path.Count - 1);
            }

            Vector2Int destination = selfUnit.currentGridPos;
            while (stepsToTake > 0)
            {
                Vector2Int candidate = path[stepsToTake - 1];
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

        distance = CalculateDistance(selfUnit.currentGridPos, targetUnit.currentGridPos);
        if (distance <= attackRange)
        {
            yield return new WaitForSeconds(0.25f);
            Debug.Log(gameObject.name + " ataca al jugador. Daño base: " + attackDamage);
            targetUnit.ReceiveAttack(selfUnit, attackDamage);
            yield return new WaitForSeconds(0.5f);
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

    int CalculateDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}