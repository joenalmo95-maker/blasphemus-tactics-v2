using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName = "Nueva Habilidad";
    public int actionPointCost = 1;
    public int range = 1;
    public int damage = 1;
}