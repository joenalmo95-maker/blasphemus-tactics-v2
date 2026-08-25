using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    // 1.1-E.7: instancia única (evita doble consumo de AP por controladores duplicados)
    public static CombatController Instance { get; private set; }

    private SkillData armedSkill = null;
    private Unit playerUnit;
    private Unit lastFlankTarget = null;

    // Indicadores persistentes (movimiento azul / rango naranja)
    private bool isMoving = false;
    private readonly List<GameObject> moveOverlays = new List<GameObject>();
    private readonly List<GameObject> rangeOverlays = new List<GameObject>();
    private HashSet<Vector2Int> reachableCells = new HashSet<Vector2Int>();
    private int lastAP = -1;
    private Vector2Int lastPos = new Vector2Int(int.MinValue, int.MinValue);
    private SkillData lastArmed = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CombatController] Instancia duplicada destruida.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerUnit = GetPlayer();
        DetectAndShowBossBar();
    }
    
    void DetectAndShowBossBar()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (u.isEnemy && u.isBoss)
            {
                string bossName = u.gameObject.name;
                string subtitle = GetBossSubtitle(u);
                BossHealthBarUI.Show(u, bossName, subtitle);
                Debug.Log("[CombatController] Boss detectado: " + bossName);
                break;
            }
        }
    }
    
    string GetBossSubtitle(Unit boss)
    {
        string name = boss.gameObject.name;
        if (name.Contains("Ángel") || name.Contains("Angel")) return "El Guardián de la Luz Eterna";
        if (name.Contains("Capitán") || name.Contains("Capitan")) return "El Templario Caído";
        return "Enemigo Legendario";
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

        // Sin indicadores ni acciones durante turno enemigo
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn())
        {
            ClearAllOverlays();
            lastAP = -1;
            return;
        }

        // Skills 1-4 desde loadout
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryToggleSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryToggleSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryToggleSkill(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryToggleSkill(3);

        // Ultimate (slot 5)
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryUseUltimate();

        // Consumibles (Poción de Energía eliminada)
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryUse(ConsumableType.PocionHP);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TryUse(ConsumableType.ComidaDano);
        if (Input.GetKeyDown(KeyCode.Alpha8)) TryUse(ConsumableType.ComidaDefensa);

        // ÚNICO bloque de clic de movimiento (1 clic = 1 ruta = 1 AP por casilla)
        if (Input.GetMouseButtonDown(0) && armedSkill == null && !isMoving)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            if (reachableCells.Contains(cell))
            {
                TryMoveTo(cell);
            }
        }

        // 1.4: Ataque con skill armada (click derecho) - usa Cooldown en vez de AP
        if (armedSkill != null && Input.GetMouseButtonDown(1))
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            Unit target = Pathfinding.UnitAt(cell);
            if (armedSkill.range <= 0) target = playerUnit;
            if (target != null && !target.isEnemy)
            {
                TryCastSupport(target);
            }
            if (target != null && target.isEnemy)
            {
                int distance = Pathfinding.GridDistance(target.currentGridPos, playerUnit.currentGridPos);
                string usedId = ArmedSkillId();
                SkillMeta meta = SkillPool.Meta(usedId);
                int currentCd = playerUnit.GetSkillCooldown(usedId);

                if (distance <= armedSkill.range && currentCd == 0)
                {
                    // Flanking (solo melee)
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
                    int worldDmg = (CharacterData.Instance != null) ? CharacterData.Instance.worldBuffDamage : 0;
                    int baseDamage = Mathf.RoundToInt((armedSkill.damage + passiveBonus + playerUnit.stats.damage + playerUnit.buffDamage + worldDmg) * flankMult);

                    // 1.4: Sistema de ejecución heroica (Halo Roto)
                    float executeMult = 1f;
                    if (meta != null && meta.executeMultiplier > 1f && meta.executeThreshold > 0)
                    {
                        float hpPercent = (float)playerUnit.currentHealth / playerUnit.maxHealth * 100f;
                        if (hpPercent < meta.executeThreshold)
                        {
                            int roll = Random.Range(0, 100);
                            if (roll < meta.executeChance)
                            {
                                executeMult = meta.executeMultiplier;
                                CombatFeedback.SpawnText(playerUnit.transform.position, "¡EJECUCIÓN!", new Color(1f, 0.3f, 0.1f));
                                Debug.Log("[Ejecución] ¡El Halo responde! x" + meta.executeMultiplier + " daño.");
                            }
                            else
                            {
                                CombatFeedback.SpawnText(playerUnit.transform.position, "el Halo no responde", Color.gray);
                                Debug.Log("[Ejecución] El Halo no respondió (roll " + roll + " vs " + meta.executeChance + "%).");
                            }
                        }
                    }

                    int raw = Mathf.RoundToInt(baseDamage * executeMult);
                    bool hit = target.ReceiveAttack(playerUnit, raw, armedSkill.bonusCrit + flankCrit, armedSkill.threatMult);

                    if (ft == FlankType.Lateral) CombatFeedback.SpawnText(target.transform.position, "FLANK +10%", Color.yellow);
                    if (ft == FlankType.Espalda) CombatFeedback.SpawnText(target.transform.position, "BACKSTAB +15%", Color.red);

                    playerUnit.UpdateFacing(new Vector2(target.currentGridPos.x - playerUnit.currentGridPos.x,
                                                        target.currentGridPos.y - playerUnit.currentGridPos.y).normalized);

                    if (hit) ResolveEffectKey(target);

                    // 1.4-fix: cooldown 0 = siempre disponible (Golpe Básico)
                    int cdToApply = (meta != null) ? meta.cooldown : 0;
                    if (cdToApply > 0) playerUnit.SetSkillCooldown(usedId, cdToApply);
                    Debug.Log("[Cooldown] " + armedSkill.skillName + " en CD: " + cdToApply + " turnos.");

                    QuestSystem.NotifySkillUsed();
                    armedSkill = null;
                }
                else if (currentCd > 0)
                {
                    Debug.Log(armedSkill.skillName + " en cooldown: " + currentCd + " turnos.");
                }
                else
                {
                    Debug.Log("Objetivo fuera de rango.");
                }
            }
        }

        // Arcos de sector mientras se apunta con melee
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

        RefreshIndicators();
    }

    void RefreshIndicators()
    {
        if (playerUnit == null || isMoving) return;

        bool changed = playerUnit.currentAP != lastAP ||
                       playerUnit.currentGridPos != lastPos ||
                       armedSkill != lastArmed;
        if (!changed) return;

        lastAP = playerUnit.currentAP;
        lastPos = playerUnit.currentGridPos;
        lastArmed = armedSkill;

        ClearAllOverlays();

        if (armedSkill != null)
        {
            int r = armedSkill.range;
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int cell = lastPos + new Vector2Int(dx, dy);
                    if (!GridManager.Instance.InBounds(cell)) continue;
                    SpawnOverlay(cell, new Color(0.9f, 0.4f, 0.1f, 0.22f), rangeOverlays);
                }
            }
            reachableCells.Clear();
        }
        else if (playerUnit.currentAP > 0)
        {
            reachableCells = Pathfinding.GetReachableCells(lastPos, playerUnit.currentAP);
            foreach (Vector2Int cell in reachableCells)
            {
                SpawnOverlay(cell, new Color(0.2f, 0.5f, 1f, 0.28f), moveOverlays);
            }
        }
        else
        {
            reachableCells.Clear();
        }
    }

    void SpawnOverlay(Vector2Int cell, Color color, List<GameObject> list)
    {
        GameObject go = new GameObject("CellOverlay");
        go.transform.position = new Vector3(cell.x, cell.y, -0.1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = color;
        sr.sortingOrder = 1;
        go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        list.Add(go);
    }

    void ClearAllOverlays()
    {
        foreach (GameObject go in moveOverlays) if (go != null) Destroy(go);
        moveOverlays.Clear();
        foreach (GameObject go in rangeOverlays) if (go != null) Destroy(go);
        rangeOverlays.Clear();
        reachableCells.Clear();
    }

    void TryMoveTo(Vector2Int target)
    {
        if (isMoving) return;
        List<Vector2Int> path = Pathfinding.FindPath(playerUnit.currentGridPos, target, playerUnit.currentAP);
        if (path == null || path.Count == 0) return;
        StartCoroutine(MoveAlongPath(path));
    }

    IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isMoving = true;

        foreach (Vector2Int cell in path)
        {
            if (playerUnit.currentAP <= 0) break;

            Vector2Int from = playerUnit.currentGridPos;
            playerUnit.currentAP--;
            playerUnit.currentGridPos = cell;

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
            playerUnit.UpdateFacing(new Vector2(cell.x - from.x, cell.y - from.y).normalized);
        }

        isMoving = false;
        Debug.Log("Movimiento completado. AP restantes: " + playerUnit.currentAP);
    }

    void TryToggleSkill(int slot)
    {
        // 0.8-fix: habilidad bloqueada por el Flagelante
        if (CharacterData.Instance != null && CharacterData.Instance.blockedSkillTurns > 0
            && CharacterData.Instance.blockedSkillSlot == slot)
        {
            Debug.Log("Habilidad " + (slot + 1) + " BLOQUEADA por el Flagelante este turno.");
            return;
        }

        SkillData skill = LoadoutSystem.GetActive(slot);
        if (skill == null)
        {
            Debug.Log("Slot " + (slot + 1) + " vacío.");
            return;
        }

        // 1.4-fix2: permitir ARMAR cualquier skill (para ver rango, tooltip, etc.)
        // El check de cooldown solo se hace al EJECUTAR (clic derecho)
        ToggleSkill(skill);
    }

    public void TryUseUltimate()
    {
        if (playerUnit == null) return;
        string ultId = LoadoutSystem.UltimateId();
        int cd = playerUnit.GetSkillCooldown(ultId);
        if (cd > 0)
        {
            Debug.Log("Ultimate en cooldown: " + cd + " turnos.");
            return;
        }

        SkillData ult = LoadoutSystem.GetUltimate();
        if (ult == null)
        {
            Debug.Log("Sin ultimate asignado.");
            return;
        }
        ToggleSkill(ult);
    }

    // 1.4: Helper para obtener el ID de la skill armada
    public int GetCooldownForArmed()
    {
        if (armedSkill == null || playerUnit == null) return 0;
        string id = ArmedSkillId();
        return playerUnit.GetSkillCooldown(id);
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
                    if (target.isBoss || target.isCCImmune) { CombatFeedback.ShowImmune(target.transform.position); break; }
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
                    if (target.isBoss || target.isCCImmune) { CombatFeedback.ShowImmune(target.transform.position); break; }
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
    // FIX 1: uso propio/aliados (curación/buff)
    void TryCastSupport(Unit target)
    {
        int dist = Mathf.Abs(target.currentGridPos.x - playerUnit.currentGridPos.x) +
                   Mathf.Abs(target.currentGridPos.y - playerUnit.currentGridPos.y);
        if (target != playerUnit && dist > armedSkill.range)
        {
            Debug.Log("Aliado fuera de rango.");
            return;
        }
        string usedId = ArmedSkillId();
        int currentCd = playerUnit.GetSkillCooldown(usedId);
        if (currentCd > 0)
        {
            Debug.Log(armedSkill.skillName + " en cooldown: " + currentCd + " turnos.");
            return;
        }

        SkillMeta meta = SkillPool.Meta(usedId);
        if (meta != null && meta.heal > 0)
        {
            target.Heal(meta.heal + playerUnit.stats.healingPower / 2);
        }
        if (meta != null && meta.effectKey == "buff")
        {
            target.AddBuff(2, 2, 5, 3);
        }
        else if (meta == null || meta.heal <= 0)
        {
            target.AddBuff(2, 0, 0, 3);
        }

        int cdToApply = (meta != null) ? meta.cooldown : 0;
        if (cdToApply > 0) playerUnit.SetSkillCooldown(usedId, cdToApply);
        Debug.Log("[Cooldown] " + armedSkill.skillName + " en CD: " + cdToApply + " turnos.");

        QuestSystem.NotifySkillUsed();
        armedSkill = null;
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
        // 0.8-fix: el bloqueo del Flagelante dura 1 turno
        if (CharacterData.Instance != null && CharacterData.Instance.blockedSkillTurns > 0)
        {
            CharacterData.Instance.blockedSkillTurns--;
            if (CharacterData.Instance.blockedSkillTurns == 0)
            {
                CharacterData.Instance.blockedSkillSlot = -1;
                Debug.Log("El bloqueo de habilidad ha expirado.");
            }
        }

        ClearAllOverlays();
        lastAP = -1;
    }
}