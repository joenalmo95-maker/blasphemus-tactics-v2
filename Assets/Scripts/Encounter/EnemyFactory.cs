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

        int baseHp = 100;
        int baseDamage = 20;
        int baseDefense = 0;
        int baseAttackRange = 1;
        string art = "penitent";
        string name = "Cruzado";
        float scale = 0.8f;
        Color tint = Color.white;
        EnemyBehavior behavior = EnemyBehavior.Normal;
        bool canCharge = false;
        bool applyCurse = false;

        switch (archetype)
        {
            // === ARQUETIPOS BASE (existentes) ===
            case "boss":
                baseHp = 400; baseDamage = 40; art = "angel";
                name = "Ángel de la Vigilia"; scale = 1.6f;
                tint = new Color(1f, 0.95f, 0.6f);
                break;
            case "cherub":
                baseHp = 60; baseDamage = 20; art = "cherub";
                name = "Querubín"; scale = 0.7f;
                break;
            case "inquisitor":
                baseHp = 80; baseDamage = 20; art = "inquisitor";
                name = "Inquisidor"; scale = 0.8f;
                applyCurse = true;
                break;
            case "capitan":
                baseHp = 140; baseDamage = 30; art = "capitan";
                name = "Capitán Templario"; scale = 1.0f;
                canCharge = true;
                break;

            // === NUEVOS ARQUETIPOS (FASE A) ===
            case "flagelante":
                baseHp = 80; baseDamage = 35; art = "penitent";
                name = "Flagelante"; scale = 0.85f;
                tint = new Color(0.9f, 0.4f, 0.4f);
                behavior = EnemyBehavior.SelfDamage;
                break;
            case "incensario":
                baseHp = 90; baseDamage = 15; art = "healer";
                name = "Incensario"; scale = 0.9f;
                tint = new Color(1f, 0.85f, 0.4f);
                behavior = EnemyBehavior.Healer;
                break;
            case "censor":
                baseHp = 50; baseDamage = 25; baseDefense = -2; baseAttackRange = 4;
                art = "inquisitor"; name = "Censor"; scale = 0.75f;
                tint = new Color(0.6f, 0.6f, 0.9f);
                behavior = EnemyBehavior.Ranged;
                break;
            case "ceniza":
                baseHp = 70; baseDamage = 18; art = "penitent";
                name = "Penitente de la Ceniza"; scale = 0.8f;
                tint = new Color(0.5f, 0.5f, 0.5f);
                behavior = EnemyBehavior.ExplodeOnDeath;
                break;
            case "heraldo":
                baseHp = 90; baseDamage = 20; art = "inquisitor";
                name = "Heraldo Ciego"; scale = 0.85f;
                tint = new Color(0.3f, 0.3f, 0.5f);
                behavior = EnemyBehavior.Backstabber;
                break;
            case "automata":
                baseHp = 180; baseDamage = 15; baseDefense = 5;
                art = "tank"; name = "Autómata de Reliquias"; scale = 1.1f;
                tint = new Color(0.7f, 0.6f, 0.4f);
                behavior = EnemyBehavior.CCImmune;
                break;

            case "penitent":
            default:
                baseHp = 100; baseDamage = 20; art = "penitent";
                name = "Cruzado"; scale = 0.8f;
                break;
        }

        int hp = Mathf.RoundToInt(baseHp * hpMult);
        int playerLevel = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        hp = Mathf.RoundToInt(hp * Progression.EnemyHpMult(playerLevel));

        Unit unit = Unit.Create(name, cell, true, tint, scale, hp, 3, art);

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
            ai.attackRange = baseAttackRange;
            ai.behavior = behavior;
            ai.baseDefense = baseDefense;
            ai.canCharge = canCharge;
            ai.applyCurse = applyCurse;
            ai.unitName = name;
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

public enum EnemyBehavior
{
    Normal,
    SelfDamage,
    Healer,
    Ranged,
    ExplodeOnDeath,
    Backstabber,
    CCImmune
}