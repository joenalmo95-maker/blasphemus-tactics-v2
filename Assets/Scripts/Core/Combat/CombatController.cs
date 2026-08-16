using UnityEngine;

public class CombatController : MonoBehaviour
{
    private SkillData armedSkill = null;
    private Unit playerUnit;

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

        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleSkill(SkillCatalog.Get(Role(), 1));
        if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleSkill(SkillCatalog.Get(Role(), 2));
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryUtility();

        if (Input.GetKeyDown(KeyCode.Alpha4)) TryUse(ConsumableType.PocionHP);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryUse(ConsumableType.PocionAP);
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryUse(ConsumableType.ComidaDano);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TryUse(ConsumableType.ComidaDefensa);

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
                    int raw = armedSkill.damage + playerUnit.stats.damage + playerUnit.buffDamage;
                    target.ReceiveAttack(playerUnit, raw, armedSkill.bonusCrit, armedSkill.threatMult);
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

    void TryUtility()
    {
        if (playerUnit.currentAP < 1)
        {
            Debug.Log("AP insuficientes para la utilidad.");
            return;
        }

        switch (Role())
        {
            case ClassRole.Tank:
                playerUnit.currentAP -= 1;
                playerUnit.AddBuff(2, 0, 3);
                playerUnit.threat += 5f * playerUnit.stats.threatMult;
                Debug.Log("Grito de Guerra: +2 daño por 3 turnos y amenaza alta. AP restantes: " + playerUnit.currentAP);
                break;

            case ClassRole.Healer:
            {
                int baseHeal = 4;
                int amount = Mathf.RoundToInt(baseHeal * (1 + playerUnit.stats.healingPower / 100f));
                playerUnit.currentAP -= 1;
                playerUnit.Heal(amount);
                playerUnit.threat += amount * 2f * playerUnit.stats.threatMult;
                Debug.Log("Curación ejecutada. AP restantes: " + playerUnit.currentAP);
                break;
            }

            default:
                playerUnit.currentAP -= 1;
                playerUnit.AddBuff(0, 0, 15, 3);
                Debug.Log("Ojos del Halo: +15% crítico por 3 turnos. AP restantes: " + playerUnit.currentAP);
                break;
        }
    }

    void TryUse(ConsumableType t)
    {
        if (InventorySystem.Instance != null) InventorySystem.Instance.UseConsumable(t);
    }

    void ToggleSkill(SkillData skill)
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
}