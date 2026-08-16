using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    public int attackDamage = 4;
    public EnemyTier tier = EnemyTier.Jefe;

    private Unit selfUnit;
    private Unit targetUnit;
    private int pattern = 0;
    private List<Vector2Int> telegraphed = new List<Vector2Int>();
    private List<GameObject> overlays = new List<GameObject>();

    void Awake()
    {
        selfUnit = GetComponent<Unit>();
        selfUnit.stats.attack = 80;
        selfUnit.stats.evasion = 10;
        selfUnit.stats.defense = 2;
        selfUnit.stats.critChance = 10;
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

        if (telegraphed.Count > 0)
        {
            yield return ResolveJudgment();
        }

        int distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);

        if (pattern == 0)
        {
            if (distance > 1)
            {
                yield return Approach();
                distance = Dist(selfUnit.currentGridPos, targetUnit.currentGridPos);
            }
            if (distance <= 1)
            {
                Debug.Log("Ángel de la Vigilia golpea al Renacido.");
                targetUnit.ReceiveAttack(selfUnit, attackDamage);
            }
            pattern = 1;
        }
        else if (pattern == 1)
        {
            Debug.Log("¡El Ángel prepara JUICIO! Casillas rojas marcadas: ¡muévete!");
            Telegraph();
            pattern = 2;
        }
        else
        {
            targetUnit.pendingApPenalty += 1;
            Debug.Log("Mirada Opresiva: el Renacido perderá 1 AP en su próximo turno.");
            CombatFeedback.SpawnText(targetUnit.transform.position, "-1 AP", Color.magenta);
            pattern = 0;
        }
    }

    IEnumerator ResolveJudgment()
    {
        Debug.Log("¡JUICIO se desata sobre las casillas marcadas!");
        foreach (Vector2Int cell in telegraphed)
        {
            CombatFeedback.SpawnImpact(GridManager.Instance.GetWorldPosition(cell), Color.red);
            Unit u = Pathfinding.UnitAt(cell);
            if (u != null && !u.isEnemy)
            {
                u.ReceiveAttack(selfUnit, 6);
            }
        }
        ClearOverlays();
        telegraphed.Clear();
        yield return new WaitForSeconds(0.4f);
    }

    void Telegraph()
    {
        telegraphed.Clear();
        Vector2Int c = targetUnit.currentGridPos;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int cell = c + new Vector2Int(dx, dy);
                if (GridManager.Instance.InBounds(cell))
                {
                    telegraphed.Add(cell);
                    GameObject go = new GameObject("Telegraph");
                    go.transform.position = new Vector3(cell.x, cell.y, -0.1f);
                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.Square();
                    sr.color = new Color(0.8f, 0.1f, 0.1f, 0.45f);
                    go.transform.localScale = new Vector3(0.95f, 0.95f, 1);
                    sr.sortingOrder = 4;
                    overlays.Add(go);
                }
            }
        }
    }

    void ClearOverlays()
    {
        foreach (GameObject go in overlays)
        {
            if (go != null) Destroy(go);
        }
        overlays.Clear();
    }

    IEnumerator Approach()
    {
        List<Vector2Int> path = Pathfinding.FindPath(
            selfUnit.currentGridPos, targetUnit.currentGridPos, 99);
        if (path == null || path.Count == 0) yield break;

        int steps = Mathf.Min(path.Count, 2);
        if (path[path.Count - 1] == targetUnit.currentGridPos)
        {
            steps = Mathf.Min(steps, path.Count - 1);
        }

        if (steps > 0)
        {
            Vector2Int dest = path[steps - 1];
            Vector3 wp = GridManager.Instance.GetWorldPosition(dest);
            while (Vector3.Distance(transform.position, wp) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, wp, 5f * Time.deltaTime);
                yield return null;
            }
            transform.position = wp;
            selfUnit.currentGridPos = dest;
        }
    }

    int Dist(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}