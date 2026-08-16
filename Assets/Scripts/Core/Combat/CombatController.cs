using UnityEngine;

public class CombatController : MonoBehaviour
{
    public SkillData basicAttack;
    public SkillData rangedAttack;
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

    void Update()
    {
        if (playerUnit == null)
        {
            playerUnit = GetPlayer();
            if (playerUnit == null) return;
        }

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleSkill(basicAttack);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleSkill(rangedAttack);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryHeal();

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
                    target.ReceiveAttack(playerUnit, armedSkill.damage + playerUnit.stats.damage);
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

    void TryHeal()
    {
        if (CharacterData.Instance == null || CharacterData.Instance.classData == null) return;

        if (CharacterData.Instance.classData.role != ClassRole.Healer)
        {
            Debug.Log("Tu clase no puede curar.");
            return;
        }

        if (playerUnit.currentAP < 1)
        {
            Debug.Log("AP insuficientes para curar.");
            return;
        }

        int baseHeal = 4;
        int amount = Mathf.RoundToInt(baseHeal * (1 + playerUnit.stats.healingPower / 100f));
        playerUnit.currentAP -= 1;
        playerUnit.Heal(amount);
        playerUnit.threat += amount * 2f * playerUnit.stats.threatMult;
        Debug.Log("Curación ejecutada. AP restantes: " + playerUnit.currentAP);
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