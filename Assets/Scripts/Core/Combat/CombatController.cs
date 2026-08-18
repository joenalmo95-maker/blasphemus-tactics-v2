using UnityEngine;

public class CombatController : MonoBehaviour
{
    private SkillData armedSkill = null;
    private Unit playerUnit;
    private int ultimateCooldown = 0;

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

        // Consumibles
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryUse(ConsumableType.PocionHP);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TryUse(ConsumableType.PocionAP);

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
                    // 4.4: pasiva de clase suma daño según el build
                    // 1.1-D: daño base + bonos de pasivas del loadout
                    int passiveBonus = CalculatePassiveBonus();
                    int raw = armedSkill.damage + passiveBonus + playerUnit.stats.damage + playerUnit.buffDamage;
                    target.ReceiveAttack(playerUnit, raw, armedSkill.bonusCrit, armedSkill.threatMult);

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