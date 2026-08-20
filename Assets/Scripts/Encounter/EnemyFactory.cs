using UnityEngine;
using System.Collections.Generic;

public static class EnemyFactory
{
    public static Unit Spawn(string archetype, EnemyTier tier, Vector2Int cell)
    {
        cell = FindFreeCell(cell);

        float hpMult = 1f;
        int dmgBonus = 0;
        switch (tier)
        {
            case EnemyTier.Medio: hpMult = 1.4f; dmgBonus = 5; break;
            case EnemyTier.Elite: hpMult = 1.8f; dmgBonus = 10; break;
            case EnemyTier.EliteFuerte: hpMult = 2.2f; dmgBonus = 15; break;
            case EnemyTier.Jefe: hpMult = 3f; dmgBonus = 20; break;
        }

        // 0.3: Nuevos stats base escalados
        int baseHp = 100;
        int baseDamage = 20;
        string art = "penitent";
        string name = "Cruzado";
        float scale = 0.8f;

        switch (archetype)
        {
            case "boss":
                baseHp = 400;
                baseDamage = 40;
                art = "angel";
                name = "Ángel de la Vigilia";
                scale = 1.6f;
                break;
            case "cherub":
                baseHp = 60;
                baseDamage = 20;
                art = "cherub";
                name = "Querubín";
                scale = 0.7f;
                break;
            case "inquisitor":
                baseHp = 80;
                baseDamage = 20;
                art = "inquisitor";
                name = "Inquisidor";
                scale = 0.8f;
                break;
            case "capitan":
                baseHp = 140;
                baseDamage = 30;
                art = "capitan";
                name = "Capitán Templario";
                scale = 1.0f;
                break;
            case "penitent":
            default:
                baseHp = 100;
                baseDamage = 20;
                art = "penitent";
                name = "Cruzado";
                scale = 0.8f;
                break;
        }

        int hp = Mathf.RoundToInt(baseHp * hpMult);

        int playerLevel = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        hp = Mathf.RoundToInt(hp * Progression.EnemyHpMult(playerLevel));

        Unit unit = Unit.Create(name, cell, true, Color.white, scale, hp, 3, art);

        if (archetype == "boss")
        {
            BossAI ai = unit.gameObject.AddComponent<BossAI>();
            ai.attackDamage = baseDamage + dmgBonus + Progression.EnemyDamageBonus(playerLevel);
            ai.tier = tier;
        }
        else
        {
            EnemyAI ai = unit.gameObject.AddComponent<EnemyAI>();
            ai.tier = tier;
            ai.attackDamage = baseDamage + dmgBonus + Progression.EnemyDamageBonus(playerLevel);
        }

        return unit;
    }

    static Vector2Int FindFreeCell(Vector2Int start)
    {
        for (int r = 0; r < 5; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    Vector2Int c = new Vector2Int(start.x + dx, start.y + dy);
                    if (TerrainMap.IsWalkable(c) && !Unit.At(c)) return c;
                }
            }
        }
        return start;
    }
}