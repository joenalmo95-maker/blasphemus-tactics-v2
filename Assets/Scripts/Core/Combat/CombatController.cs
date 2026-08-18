using UnityEngine;

public class CombatController : MonoBehaviour
{
    private SkillData armedSkill = null;
    private Unit playerUnit;
    private int ultimateCooldown = 0;
    private Unit lastFlankTarget = null;

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

        // 1.1-D: skills 1-4 desde loadout, slot 5 = ultimate
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

    // 1.1-D: lee del loadout persistente
    void TryToggleSkill(int slot)
    {
        SkillData skill = LoadoutSystem.GetActive(slot);
        if (skill == null)
        {
            Debug.Log("Slot " + (slot + 1) + " vacío.");
            return;
        }
        ToggleSkill(skill);
    }

    // 1.1-D: ultimate con cooldown
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

        ToggleSkill(ult);
    }

    // 1.1-E: id de la skill armada (para effectKey)
    string ArmedSkillId()
    {
        for (int i = 0; i < 4; i++)
        {
            if (LoadoutSystem.GetActive(i) == armedSkill) return LoadoutSystem.ActiveId(i);
        }
        if (LoadoutSystem.GetUltimate() == armedSkill) return LoadoutSystem.UltimateId();
        return "";
    }

    // 1.1-E: knockback/pull/lunge con inmunidad de jefes a control
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


    // 1.1-D: calcula bonos de pasivas del loadout
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

    // 1.1-D: decrementa cooldown de ultimate al final del turno del jugador
    public void EndPlayerTurn()
    {
        if (ultimateCooldown > 0) ultimateCooldown--;
    }    
}