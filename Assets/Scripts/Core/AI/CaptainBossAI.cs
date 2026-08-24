using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 0.7-F.1d: IA del Capitán (Boss Mundial) con mecánicas telegrafiadas + reposicionamiento
public class CaptainBossAI : MonoBehaviour
{
    public int attackDamage = 35; // 0.7-F.1d-fix: daño subido
    public EnemyTier tier = EnemyTier.Jefe;
    const int COMBAT_GRID_WIDTH = 10;
    const int COMBAT_GRID_HEIGHT = 10;

    private Unit selfUnit;
    private Unit targetUnit;
    private int pattern = 0; // 0=Embestida, 1=Ejecución, 2=Barrido, 3=Juicio, 4=Onda
    private int pendingTelegraphDamage = 0;
    private List<Vector2Int> telegraphed = new List<Vector2Int>();
    private List<GameObject> overlays = new List<GameObject>();

    // 0.7-F.1d Trampa: trampas invisibles + DoT
    private List<Vector2Int> traps = new List<Vector2Int>();
    private int trapTurnsLeft = 0;
    private bool dotActive = false;
    private int dotTurn = 0;
    private int dotCooldown = 0;

    void Awake()
    {
        selfUnit = GetComponent<Unit>();
        selfUnit.stats.accuracy = 80;
        selfUnit.stats.evasion = 10;
        selfUnit.stats.defense = 10;
        selfUnit.stats.critChance = 15;
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
            if (u.threat > bestThreat) { bestThreat = u.threat; best = u; }
        }
        if (best == null)
        {
            foreach (Unit u in units) { if (!u.isEnemy) return u; }
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

        // Resolver telegrafos del turno anterior
        if (telegraphed.Count > 0)
        {
            yield return ResolveTelegraph();
        }

        switch (pattern)
        {
            case 0: yield return DoEmbestida(); break; // NO reposiciona
            case 1: DoEjecucion(); break;
            case 2: DoBarrido(); break;
            case 3: DoTrampa(); break; // siembra trampas, NO reposiciona (ANTES ERA 5)
            case 4: DoOndaGlobal(); break;
            case 5: DoJuicio(); break; // (ANTES ERA 3)
        }

        // Reposicionar tras todas las mecánicas EXCEPTO embestida (0) y trampa (3)
        if (pattern != 0 && pattern != 3)
        {
            yield return Reposicionar();
        }

        TickTraps(); // expiración de trampas

        pattern = (pattern + 1) % 6;
    }

    // === MECÁNICAS ===

    IEnumerator DoEmbestida()
    {
        if (targetUnit == null) yield break;
        
        Debug.Log("[Capitán] ¡Embestida hacia el Renacido!");
        List<Vector2Int> path = Pathfinding.FindPath(selfUnit.currentGridPos, targetUnit.currentGridPos, 99);
        if (path != null && path.Count > 1)
        {
            int steps = Mathf.Min(path.Count - 1, 3);
            Vector2Int dest = path[steps - 1];
            Vector3 wp = GridManager.Instance.GetWorldPosition(dest);
            while (Vector3.Distance(transform.position, wp) > 0.05f)
            {
                // FIX: Si el jugador muere mientras me acerco, aborto la embestida
                if (targetUnit == null) yield break; 
                
                transform.position = Vector3.MoveTowards(transform.position, wp, 8f * Time.deltaTime);
                yield return null;
            }
            transform.position = wp;
            selfUnit.currentGridPos = dest;
        }

        // FIX: Doble chequeo antes de intentar atacar
        if (targetUnit == null) yield break;

        if (Dist(selfUnit.currentGridPos, targetUnit.currentGridPos) <= 1)
        {
            FaceTarget();
            CombatFeedback.SpawnText(selfUnit.transform.position, "¡Embestida!", Color.red);
            targetUnit.ReceiveAttack(selfUnit, attackDamage);
        }
        yield return new WaitForSeconds(0.3f);
    }

    void DoEjecucion()
    {
        Debug.Log("[Capitán] ¡EJECUCION! Diagonales impares marcadas - ¡pisa casilla par!");
        TelegraphDiagonals(80, new Color(0.9f, 0.1f, 0.1f, 0.6f));
    }

    void DoBarrido()
    {
        Debug.Log("[Capitán] ¡BARRIDO! Filas y columnas pares marcadas - ¡pisa impar,impar!");
        TelegraphEvenLines(45, new Color(0.8f, 0.3f, 0.1f, 0.5f));
    }

    void DoJuicio()
    {
        // FIX: Evitar leer posición de un jugador que ya fue destruido
        if (targetUnit == null) return; 
        
        Debug.Log("[Capitán] ¡JUICIO DEL CAPITAN! Zona 5x5 roja - ¡sal del área!");
        TelegraphArea(targetUnit.currentGridPos, 2, 60, new Color(0.9f, 0.7f, 0.1f, 0.5f));
    }

    void DoOndaGlobal()
    {
        Debug.Log("[Capitán] ¡ONDA DE CHOQUE! 35 de daño a todo el mapa (mitigado por defensa).");
        CombatFeedback.SpawnText(selfUnit.transform.position, "¡ONDA GLOBAL!", Color.yellow);
        
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        
        foreach (Unit u in units)
        {
            if (u.isEnemy) continue;
            u.ReceiveAttack(selfUnit, 35); // ← FIX: Daño base subido a 35 para compensar la defensa del jugador
        }
    }

    // === TRAMPA (mecánica 5) ===

    void DoTrampa()
    {
        Debug.Log("[Capitán] ¡TRAMPA! Siembra 10 trampas invisibles (2 turnos).");
        traps.Clear();
        trapTurnsLeft = 2;
        int placed = 0;
        for (int attempt = 0; attempt < 200 && placed < 10; attempt++)
        {
            Vector2Int cell = new Vector2Int(Random.Range(0, COMBAT_GRID_WIDTH), Random.Range(0, COMBAT_GRID_HEIGHT));
            if (!GridManager.Instance.InBounds(cell)) continue;
            if (!TerrainMap.IsWalkable(cell)) continue;
            if (traps.Contains(cell)) continue;
            if (Pathfinding.UnitAt(cell) != null) continue;
            traps.Add(cell);
            placed++;
        }
        Debug.Log("[Capitán] Trampas sembradas: " + traps.Count);
    }

    void TickTraps()
    {
        if (trapTurnsLeft > 0)
        {
            trapTurnsLeft--;
            if (trapTurnsLeft == 0)
            {
                traps.Clear();
                Debug.Log("[Capitán] Trampas expiradas.");
            }
        }

        // 0.7-F.1d: tick del DoT de trampa (una vez por turno del boss)
        TickDot();
    }

    // === DISPARO DE TRAMPA + DoT ===

    void Update()
    {
        // FIX: Si el jugador murió, no intentar revisar trampas sobre él
        if (targetUnit == null) return;

        // Si Valerius pisa una trampa invisible, se activa
        if (trapTurnsLeft > 0 && !dotActive && dotCooldown <= 0)
        {
            if (traps.Contains(targetUnit.currentGridPos))
            {
                ApplyDot();
            }
        }
    }

    void ApplyDot()
    {
        dotActive = true;
        dotTurn = 1;
        targetUnit.stats.damage = Mathf.Max(1, targetUnit.stats.damage - 5); // reducción de ataque
        CombatFeedback.SpawnText(targetUnit.transform.position, "¡Trampa! DoT + ataque reducido", Color.green);
        Debug.Log("[Capitán] ¡Valerius pisó una trampa! DoT activo (2 turnos).");
    }

    void TickDot()
    {
        if (dotCooldown > 0) dotCooldown--;
        if (!dotActive || targetUnit == null) return;

        int dmg = (dotTurn == 1) ? 10 : 15;
        // Nunca baja de 1 HP (es debuff, no ejecución)
        int real = Mathf.Min(dmg, targetUnit.currentHealth - 1);
        if (real > 0)
        {
            targetUnit.currentHealth -= real;
            CombatFeedback.SpawnText(targetUnit.transform.position, "-" + real + " HP (trampa)", Color.green);
        }

        dotTurn++;
        if (dotTurn > 2) EndDot();
    }

    void EndDot()
    {
        dotActive = false;
        dotTurn = 0;
        dotCooldown = 3; // no recibe uno nuevo hasta pasar 3 turnos
        if (targetUnit != null) targetUnit.stats.damage += 5; // restaura ataque
        Debug.Log("[Capitán] DoT de trampa terminado. Cooldown 3 turnos.");
    }

    // === SISTEMA DE TELEGRAFOS ===

    void TelegraphDiagonals(int damage, Color color)
    {
        ClearTelegraphData();
        pendingTelegraphDamage = damage;
        for (int x = 0; x < COMBAT_GRID_WIDTH; x++)
        {
            for (int y = 0; y < COMBAT_GRID_HEIGHT; y++)
            {
                if ((x + y) % 2 == 1) // diagonales impares
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (GridManager.Instance.InBounds(cell))
                    {
                        telegraphed.Add(cell);
                        CreateOverlay(cell, color);
                    }
                }
            }
        }
    }

    void TelegraphEvenLines(int damage, Color color)
    {
        ClearTelegraphData();
        pendingTelegraphDamage = damage;
        for (int x = 0; x < COMBAT_GRID_WIDTH; x++)
        {
            for (int y = 0; y < COMBAT_GRID_HEIGHT; y++)
            {
                if (x % 2 == 0 || y % 2 == 0) // filas y columnas pares
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (GridManager.Instance.InBounds(cell))
                    {
                        telegraphed.Add(cell);
                        CreateOverlay(cell, color);
                    }
                }
            }
        }
    }

    void TelegraphArea(Vector2Int center, int radius, int damage, Color color)
    {
        ClearTelegraphData();
        pendingTelegraphDamage = damage;
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int cell = center + new Vector2Int(dx, dy);
                if (GridManager.Instance.InBounds(cell))
                {
                    telegraphed.Add(cell);
                    CreateOverlay(cell, color);
                }
            }
        }
    }

    void CreateOverlay(Vector2Int cell, Color color)
    {
        GameObject go = new GameObject("Telegraph");
        go.transform.position = new Vector3(cell.x, cell.y, -0.1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = color;
        go.transform.localScale = new Vector3(0.95f, 0.95f, 1);
        sr.sortingOrder = 4;
        overlays.Add(go);
    }

    IEnumerator ResolveTelegraph()
    {
        Debug.Log("[Capitán] ¡Telegrafo se resuelve!");
        foreach (Vector2Int cell in telegraphed)
        {
            CombatFeedback.SpawnImpact(GridManager.Instance.GetWorldPosition(cell), Color.red);
            Unit u = Pathfinding.UnitAt(cell);
            if (u != null && !u.isEnemy)
            {
                u.ReceiveAttack(selfUnit, pendingTelegraphDamage);
            }
        }
        ClearOverlays();
        telegraphed.Clear();
        yield return new WaitForSeconds(0.4f);
    }

    void ClearTelegraphData()
    {
        telegraphed.Clear();
        ClearOverlays();
    }

    void ClearOverlays()
    {
        foreach (GameObject go in overlays)
        {
            if (go != null) Destroy(go);
        }
        overlays.Clear();
    }

    // === REPOSICIONAMIENTO (cercano, alcanzable con 4 AP) ===

    IEnumerator Reposicionar()
    {
        Debug.Log("[Capitán] Reposicionamiento táctico...");
        Vector2Int playerPos = targetUnit.currentGridPos;
        Vector2Int dest = Vector2Int.zero;
        bool found = false;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            int dx = Random.Range(-3, 4);
            int dy = Random.Range(-3, 4);
            Vector2Int candidate = playerPos + new Vector2Int(dx, dy);

            if (!GridManager.Instance.InBounds(candidate)) continue;
            if (!TerrainMap.IsWalkable(candidate)) continue;
            if (Pathfinding.UnitAt(candidate) != null) continue;

            float dist = Dist(candidate, playerPos);
            if (dist < 2 || dist > 3) continue; // cercano: alcanzable con 4 AP

            dest = candidate;
            found = true;
            break;
        }

        if (!found)
        {
            Debug.LogWarning("[Capitán] No se encontró celda válida para reposicionar.");
            yield break;
        }

        Vector3 wp = GridManager.Instance.GetWorldPosition(dest);
        CombatFeedback.SpawnText(selfUnit.transform.position, "¡Reposicionamiento!", Color.cyan);
        selfUnit.currentGridPos = dest;
        transform.position = wp;
        FaceTarget();
        yield return new WaitForSeconds(0.3f);
    }

    void FaceTarget()
    {
        if (selfUnit != null && targetUnit != null)
        {
            selfUnit.UpdateFacing(new Vector2(
                targetUnit.currentGridPos.x - selfUnit.currentGridPos.x,
                targetUnit.currentGridPos.y - selfUnit.currentGridPos.y).normalized);
        }
    }

    int Dist(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
}