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

    static SkillData Build(ClassRole role, int slot)
    {
        SkillData d = ScriptableObject.CreateInstance<SkillData>();

        switch (role)
        {
            case ClassRole.Tank:
                if (slot == 1)
                {
                    d.skillName = "Tajo de Guerra"; d.actionPointCost = 1; d.range = 1; d.damage = 4; d.threatMult = 2f;
                }
                else
                {
                    d.skillName = "Golpe de Escudo"; d.actionPointCost = 2; d.range = 1; d.damage = 5; d.threatMult = 3f;
                }
                break;

            case ClassRole.Healer:
                if (slot == 1)
                {
                    d.skillName = "Golpe de Cetro"; d.actionPointCost = 1; d.range = 1; d.damage = 3;
                }
                else
                {
                    d.skillName = "Salmo Ardiente"; d.actionPointCost = 2; d.range = 4; d.damage = 3;
                }
                break;

            default:
                if (slot == 1)
                {
                    d.skillName = "Latigazo"; d.actionPointCost = 1; d.range = 2; d.damage = 4; d.bonusCrit = 10;
                }
                else
                {
                    d.skillName = "Corte del Halo"; d.actionPointCost = 2; d.range = 3; d.damage = 5; d.bonusCrit = 20;
                }
                break;
        }

        return d;
    }
}