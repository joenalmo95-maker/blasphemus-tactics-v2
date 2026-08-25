using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName = "Nueva Habilidad";
    public string description = "Descripción de la habilidad";
    public int actionPointCost = 1;
    public int range = 1;
    public int damage = 1;
    public int bonusCrit = 0;
    public float threatMult = 1f;
    public int unlockLevel = 0;
    public int cooldown = 0; // 1.4: cooldown de la habilidad en turnos (0 = siempre disponible)
}