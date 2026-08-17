using UnityEngine;
using System.Collections.Generic;

public static class SkillCatalog
{
    private static Dictionary<string, SkillData> cache = new Dictionary<string, SkillData>();

    public static SkillData Get(ClassRole role, int slot)
    {
        string key = role + "_" + slot;
        if (cache.TryGetValue(key, out SkillData s)) return s;

        SkillData built = Build(role, slot);
        cache[key] = built;
        return built;
    }

    public static int GetTotalSkillsForRole(ClassRole role)
    {
        return 4; // Todas las clases tienen 4 skills
    }

    public static bool IsSkillUnlocked(ClassRole role, int slot, int playerLevel)
    {
        SkillData skill = Get(role, slot);
        return skill != null && playerLevel >= skill.unlockLevel && SkillTrainer.IsLearned(slot);
    }

    static SkillData Build(ClassRole role, int slot)
    {
        SkillData d = ScriptableObject.CreateInstance<SkillData>();

        switch (role)
        {
            case ClassRole.Tank:
                BuildTankSkill(d, slot);
                break;
            case ClassRole.Healer:
                BuildHealerSkill(d, slot);
                break;
            default:
                BuildDpsSkill(d, slot);
                break;
        }

        return d;
    }

    static void BuildTankSkill(SkillData d, int slot)
    {
        switch (slot)
        {
            case 1:
                d.skillName = "Tajo de Guerra";
                d.description = "Ataque básico con espada que genera alta amenaza";
                d.actionPointCost = 1;
                d.range = 1;
                d.damage = 4;
                d.threatMult = 2f;
                d.unlockLevel = 0;
                break;
            case 2:
                d.skillName = "Golpe de Escudo";
                d.description = "Golpe contundente con el escudo, alto daño y amenaza extrema";
                d.actionPointCost = 2;
                d.range = 1;
                d.damage = 5;
                d.threatMult = 3f;
                d.unlockLevel = 2;
                break;
            case 3:
                d.skillName = "Carga del Penitente";
                d.description = "Embiste al enemigo, causando daño y ganando amenaza";
                d.actionPointCost = 2;
                d.range = 2;
                d.damage = 6;
                d.threatMult = 2.5f;
                d.unlockLevel = 5;
                break;
            case 4:
                d.skillName = "Martillo del Juicio";
                d.description = "Golpe devastador con martillo sagrado";
                d.actionPointCost = 3;
                d.range = 1;
                d.damage = 10;
                d.threatMult = 4f;
                d.unlockLevel = 10;
                break;
        }
    }

    static void BuildHealerSkill(SkillData d, int slot)
    {
        switch (slot)
        {
            case 1:
                d.skillName = "Golpe de Cetro";
                d.description = "Ataque cuerpo a cuerpo con el cetro sagrado";
                d.actionPointCost = 1;
                d.range = 1;
                d.damage = 3;
                d.unlockLevel = 0;
                break;
            case 2:
                d.skillName = "Salmo Ardiente";
                d.description = "Lanza un salmo de fuego a distancia";
                d.actionPointCost = 2;
                d.range = 4;
                d.damage = 3;
                d.unlockLevel = 2;
                break;
            case 3:
                d.skillName = "Llama Purificadora";
                d.description = "Fuego sagrado que causa daño en área";
                d.actionPointCost = 2;
                d.range = 3;
                d.damage = 5;
                d.unlockLevel = 5;
                break;
            case 4:
                d.skillName = "Cólera Divina";
                d.description = "Invoca la ira divina sobre el enemigo";
                d.actionPointCost = 3;
                d.range = 5;
                d.damage = 8;
                d.bonusCrit = 15;
                d.unlockLevel = 10;
                break;
        }
    }

    static void BuildDpsSkill(SkillData d, int slot)
    {
        switch (slot)
        {
            case 1:
                d.skillName = "Latigazo";
                d.description = "Azote rápido con el látigo, alta probabilidad crítica";
                d.actionPointCost = 1;
                d.range = 2;
                d.damage = 4;
                d.bonusCrit = 10;
                d.unlockLevel = 0;
                break;
            case 2:
                d.skillName = "Corte del Halo";
                d.description = "Tajo preciso que aprovecha puntos débiles";
                d.actionPointCost = 2;
                d.range = 3;
                d.damage = 5;
                d.bonusCrit = 20;
                d.unlockLevel = 2;
                break;
            case 3:
                d.skillName = "Veneno del Renegado";
                d.description = "Ataque envenenado con daño extra";
                d.actionPointCost = 2;
                d.range = 2;
                d.damage = 6;
                d.bonusCrit = 15;
                d.unlockLevel = 5;
                break;
            case 4:
                d.skillName = "Ejecución Final";
                d.description = "Golpe letal con crítico garantizado";
                d.actionPointCost = 3;
                d.range = 2;
                d.damage = 9;
                d.bonusCrit = 40;
                d.unlockLevel = 10;
                break;
        }
    }
}