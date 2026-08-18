using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    private SkillData armedSkill = null;
    private Unit playerUnit;
    private int ultimateCooldown = 0;
    private Unit lastFlankTarget = null;

    // 1.1-E.5: sistema de movimiento por click
    private bool isMoving = false;
    private List<GameObject> moveHighlights = new List<GameObject>();

    public int UltimateCooldown { get { return ultimateCooldown; } }
    public SkillData ArmedSkill { get { return armedSkill; } }

    void Start()
    {
        playerUnit = GetPlayer();
    }

    Unit GetPlayer()
    {
        if (playerUnit != null) return playerUnit;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) return u;
        }
        return null;
    }

    ClassRole Role()
    {
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
            return CharacterData.Instance.classData.role;
        return ClassRole.DPS;
    }

    void Update()
    {
        if (playerUnit == null)
        {
            playerUnit = GetPlayer();
            if (playerUnit == null) return;
        }

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn()) return;

        // Skills 1-4 desde loadout
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryToggleSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryToggleSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryToggleSkill(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryToggleSkill(3);

        // Ultimate (slot 5)
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryUseUltimate();

        // Consumibles (1.1-D.1: comidas en 8-9)
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryUse(ConsumableType.PocionHP);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TryUse(ConsumableType.PocionAP);
        if (Input.GetKeyDown(KeyCode.Alpha8)) TryUse(ConsumableType.ComidaDano);
        if (Input.GetKeyDown(KeyCode.Alpha9)) TryUse(ConsumableType.ComidaDefensa);

        // 1.1-E.5: movimiento por click en celda vacía
        if (Input.GetMouseButtonDown(0) && armedSkill == null && !isMoving)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));

            if (cell != playerUnit.currentGridPos && playerUnit.currentAP > 0)
            {
                ShowMoveHighlights();
            }
        }

        // Click para mover (con highlights visibles)
        if (Input.GetMouseButtonDown(0) && armedSkill == null && moveHighlights.Count > 0)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            TryMoveTo(cell);
        }

        // Ataque con skill armada (click derecho)
        if (armedSkill != null && Input.GetMouseButtonDown(1))
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            Unit target = Pathfinding.UnitAt(cell);

            if (target != null && target.isEnemy)
            {
                int distance = Mathf.Abs(target.currentGridPos.x - playerUnit.currentGridPos.x) +
                               Mathf.Abs(target.currentGridPos.y - playerUnit.currentGridPos.y);

                if (distance <= armedSkill.range && playerUnit.currentAP >= armedSkill.actionPointCost)
                {
                    playerUnit.currentAP -= armedSkill.actionPointCost;

                    // 1.1-E: flanking (solo melee)
                    float flankMult = 1f;
                    int flankCrit = 0;
                    FlankType ft = FlankType.Frontal;
                    if (armedSkill.range <= 1)
                    {
                        ft = target.GetFlankFrom(playerUnit);
                        if (ft == FlankType.Lateral) flankMult = 1.10f;
                        else if (ft == FlankType.Espalda) { flankMult = 1.15f; flankCrit = 10; }
                    }

                    int passiveBonus = CalculatePassiveBonus();
                    int raw = Mathf.RoundToInt((armedSkill.damage + passiveBonus + playerUnit.stats.damage + playerUnit.buffDamage) * flankMult);
                    bool hit = target.ReceiveAttack(playerUnit, raw, armedSkill.bonusCrit + flankCrit, armedSkill.threatMult);

                    if (ft == FlankType.Lateral) CombatFeedback.SpawnText(target.transform.position, "FLANK +10%", Color.yellow);
                    if (ft == FlankType.Espalda) CombatFeedback.SpawnText(target.transform.position, "BACKSTAB +15%", Color.red);

                    playerUnit.UpdateFacing(new Vector2(target.currentGridPos.x - playerUnit.currentGridPos.x,
                                                        target.currentGridPos.y - playerUnit.currentGridPos.y).normalized);

                    // 1.1-E: reposicionamiento con inmunidad de jefes
                    if (hit) ResolveEffectKey(target);

                    // Si era ultimate, inicia cooldown
                    if (LoadoutSystem.UltimateId() != "" && SkillPool.Get(LoadoutSystem.UltimateId()) == armedSkill)
                    {
                        SkillMeta meta = SkillPool.Meta(LoadoutSystem.UltimateId());
                        ultimateCooldown = meta.cooldown;
                    }

                    Debug.Log(armedSkill.skillName + " ejecutado. AP restantes: " + playerUnit.currentAP);
                    armedSkill = null;
                    ClearMoveHighlights();
                }
                else
                {
                    Debug.Log("Objetivo fuera de rango o AP insuficientes.");
                }
            }
        }

        // 1.1-E: arcos de sector mientras se apunta con melee
        if (armedSkill != null && armedSkill.range <= 1 && playerUnit != null)
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int c = new Vector2Int(Mathf.RoundToInt(w.x), Mathf.RoundToInt(w.y));
            Unit hov = Pathfinding.UnitAt(c);
            if (hov != null && hov.isEnemy && hov != lastFlankTarget)
            {
                lastFlankTarget = hov;
                FlankIndicator.Show(hov, playerUnit.currentGridPos);
            }
            else if ((hov == null || !hov.isEnemy) && lastFlankTarget != null)
            {
                lastFlankTarget = null;
                FlankIndicator.Hide();
            }
        }
        else if (lastFlankTarget != null)
        {
            lastFlankTarget = null;
            FlankIndicator.Hide();
        }
    }

    // 1.1-E.5: muestra celdas alcanzables con overlay azul
    void ShowMoveHighlights()
    {
        ClearMoveHighlights();
        HashSet<Vector2Int> reachable = Pathfinding.GetReachableCells(playerUnit.currentGridPos, playerUnit.currentAP);

        foreach (Vector2Int cell in reachable)
        {
            GameObject go = new GameObject("MoveHighlight");
            go.transform.position = new Vector3(cell.x, cell.y, -0.1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square();
            sr.color = new Color(0.2f, 0.5f, 1f, 0.3f);
            sr.sortingOrder = 1;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            moveHighlights.Add(go);
        }
    }

    // 1.1-E.5: mueve al jugador por pathfinding 8-dir
    void TryMoveTo(Vector2Int target)
    {
        if (isMoving) return;

        List<Vector2Int> path = Pathfinding.FindPath(playerUnit.currentGridPos, target, playerUnit.currentAP);
        if (path == null || path.Count == 0)
        {
            ClearMoveHighlights();
            return;
        }

        StartCoroutine(MoveAlongPath(path));
    }

    System.Collections.IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isMoving = true;
        ClearMoveHighlights();

        foreach (Vector2Int cell in path)
        {
            if (playerUnit.currentAP <= 0) break;

            playerUnit.currentAP--;
            playerUnit.currentGridPos = cell;

            // Movimiento suave (lerp)
            Vector3 start = playerUnit.transform.position;
            Vector3 end = new Vector3(cell.x, cell.y, 0);
            float duration = 0.15f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                playerUnit.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            playerUnit.transform.position = end;
            playerUnit.UpdateFacing(new Vector2(cell.x - start.x, cell.y - start.y).normalized);
        }

        isMoving = false;
        Debug.Log("Movimiento completado. AP restantes: " + playerUnit.currentAP);
    }

    void ClearMoveHighlights()
    {
        foreach (GameObject go in moveHighlights)
        {
            if (go != null) Destroy(go);
        }
        moveHighlights.Clear();
    }

    void TryToggleSkill(int slot)
    {
        SkillData skill = LoadoutSystem.GetActive(slot);
        if (skill == null)
        {
            Debug.Log("Slot " + (slot + 1) + " vacío.");
            return;
        }
        ClearMoveHighlights();
        ToggleSkill(skill);
    }

    public void TryUseUltimate()
    {
        if (ultimateCooldown > 0)
        {
            Debug.Log("Ultimate en cooldown: " + ultimateCooldown + " turnos.");
            return;
        }

        SkillData ult = LoadoutSystem.GetUltimate();
        if (ult == null)
        {
            Debug.Log("Sin ultimate asignado.");
            return;
        }
        ClearMoveHighlights();
        ToggleSkill(ult);
    }

    string ArmedSkillId()
    {
        for (int i = 0; i < 4; i++)
        {
            if (LoadoutSystem.GetActive(i) == armedSkill) return LoadoutSystem.ActiveId(i);
        }
        if (LoadoutSystem.GetUltimate() == armedSkill) return LoadoutSystem.UltimateId();
        return "";
    }

    void ResolveEffectKey(Unit target)
    {
        string id = ArmedSkillId();
        if (id == "") return;
        SkillMeta meta = SkillPool.Meta(id);
        if (meta == null || string.IsNullOrEmpty(meta.effectKey)) return;

        switch (meta.effectKey)
        {
            case "knockback":
                {
                    if (target == null || target.currentHealth <= 0) break;
                    if (target.isBoss) { CombatFeedback.ShowImmune(target.transform.position); break; }
                    Vector2Int dir = SignVec(target.currentGridPos - playerUnit.currentGridPos);
                    Vector2Int dest = target.currentGridPos + dir;
                    if (Pathfinding.IsFreeCell(dest))
                    {
                        target.currentGridPos = dest;
                        target.transform.position = new Vector3(dest.x, dest.y, 0);
                        CombatFeedback.SpawnText(target.transform.position, "EMPUJE", Color.cyan);
                    }
                }
                break;
            case "pull":
                {
                    if (target == null || target.currentHealth <= 0) break;
                    if (target.isBoss) { CombatFeedback.ShowImmune(target.transform.position); break; }
                    Vector2Int dir = SignVec(playerUnit.currentGridPos - target.currentGridPos);
                    Vector2Int dest = target.currentGridPos + dir;
                    if (Pathfinding.IsFreeCell(dest))
                    {
                        target.currentGridPos = dest;
                        target.transform.position = new Vector3(dest.x, dest.y, 0);
                        CombatFeedback.SpawnText(target.transform.position, "TIRÓN", Color.cyan);
                    }
                }
                break;
            case "lunge":
                {
                    Vector2Int dir = SignVec(target.currentGridPos - playerUnit.currentGridPos);
                    for (int step = 0; step < 2; step++)
                    {
                        Vector2Int dest = playerUnit.currentGridPos + dir;
                        if (dest == target.currentGridPos) break;
                        if (!Pathfinding.IsFreeCell(dest)) break;
                        playerUnit.currentGridPos = dest;
                        playerUnit.transform.position = new Vector3(dest.x, dest.y, 0);
                    }
                    CombatFeedback.SpawnText(playerUnit.transform.position, "EMBESTIDA", Color.cyan);
                }
                break;
        }
    }

    static Vector2Int SignVec(Vector2Int v)
    {
        return new Vector2Int(v.x > 0 ? 1 : v.x < 0 ? -1 : 0, v.y > 0 ? 1 : v.y < 0 ? -1 : 0);
    }

    int CalculatePassiveBonus()
    {
        int bonus = 0;
        foreach (SkillData passive in LoadoutSystem.GetPassives())
        {
            SkillMeta meta = SkillPool.Meta(passive.skillName);
            if (meta == null) continue;

            switch (meta.effectKey)
            {
                case "coloso": bonus += playerUnit.maxHealth / 10; break;
                case "plegaria": bonus += playerUnit.stats.healingPower / 10; break;
                case "ejecutor": bonus += playerUnit.stats.critChance / 5; break;
            }
        }
        return bonus;
    }

    void TryUse(ConsumableType t)
    {
        if (InventorySystem.Instance != null) InventorySystem.Instance.UseConsumable(t);
    }

    public void ToggleSkill(SkillData skill)
    {
        if (skill == null) return;

        if (armedSkill == skill)
        {
            armedSkill = null;
            Debug.Log("Habilidad desarmada.");
        }
        else
        {
            armedSkill = skill;
            Debug.Log("Habilidad armada: " + skill.skillName);
        }
    }

    public SkillData GetArmedSkill()
    {
        return armedSkill;
    }

    public void EndPlayerTurn()
    {
        if (ultimateCooldown > 0) ultimateCooldown--;
    }
}