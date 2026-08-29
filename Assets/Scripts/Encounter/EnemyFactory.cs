using UnityEngine;
using System.Collections.Generic;

public static class EnemyFactory
{
    public static Unit Spawn(string archetype, EnemyTier tier, Vector2Int cell)
    {
        cell = FindFreeCell(cell);

        // 1.4-B: tiers más agresivos (los enemigos escalan mejor que antes)
        float hpMult = 1f;
        int dmgBonus = 0;
        switch (tier)
        {
            case EnemyTier.Medio: hpMult = 1.5f; dmgBonus = 8; break;
            case EnemyTier.Elite: hpMult = 2.2f; dmgBonus = 15; break;
            case EnemyTier.EliteFuerte: hpMult = 3f; dmgBonus = 25; break;
            case EnemyTier.Jefe: hpMult = 4.5f; dmgBonus = 35; break;
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
        bool applyBleed = false;

        switch (archetype)
        {
            // === ARQUETIPOS BASE (existentes) === — 0.7: rebalance daño/HP
            case "boss":
            case "angel":
                // 1.4-B: Ángel más amenazante (más HP/daño, gana DEF)
                baseHp = 900; baseDamage = 65; baseDefense = 10; art = "angel";
                name = "Ángel de la Vigilia"; scale = 1.6f;
                tint = new Color(1f, 0.95f, 0.6f);
                break;
            // 0.7-F.1d: Capitán del Boss Mundial (HP 1200, gate de set Reliquia completo)
            case "capitan_mundial":
                // 1.4-B: Capitán con más HP/daño pero menos DEF (Valerius puede dañarlo)
                baseHp = 1800; baseDamage = 55; baseDefense = 15;
                art = "capitan"; name = "Capitán de la Cruzada"; scale = 1.7f;
                tint = new Color(1f, 0.85f, 0.3f); // Dorado
                break;
            case "cherub":
                baseHp = 50; baseDamage = 16; art = "cherub";
                name = "Querubín"; scale = 0.7f;
                break;
            case "inquisitor":
                baseHp = 70; baseDamage = 16; art = "inquisitor";
                name = "Inquisidor"; scale = 0.8f;
                applyCurse = true;
                break;
            case "capitan":
                baseHp = 120; baseDamage = 24; art = "capitan";
                name = "Capitán Templario"; scale = 1.0f;
                canCharge = true;
                break;

            // === NUEVOS ARQUETIPOS (FASE A) === — 0.7: rebalance
            case "flagelante":
                baseHp = 70; baseDamage = 28; art = "penitent";
                name = "Flagelante"; scale = 0.85f;
                tint = new Color(0.9f, 0.4f, 0.4f);
                behavior = EnemyBehavior.SelfDamage;
                break;
            case "incensario":
                baseHp = 80; baseDamage = 12; art = "healer";
                name = "Incensario"; scale = 0.9f;
                tint = new Color(1f, 0.85f, 0.4f);
                behavior = EnemyBehavior.Healer;
                break;
            case "censor":
                baseHp = 45; baseDamage = 20; baseDefense = -2; baseAttackRange = 4;
                art = "inquisitor"; name = "Censor"; scale = 0.75f;
                tint = new Color(0.6f, 0.6f, 0.9f);
                behavior = EnemyBehavior.Ranged;
                break;
            case "ceniza":
                baseHp = 60; baseDamage = 14; art = "penitent";
                name = "Penitente de la Ceniza"; scale = 0.8f;
                tint = new Color(0.5f, 0.5f, 0.5f);
                behavior = EnemyBehavior.ExplodeOnDeath;
                break;
            case "heraldo":
                baseHp = 80; baseDamage = 16; art = "inquisitor";
                name = "Heraldo Ciego"; scale = 0.85f;
                tint = new Color(0.3f, 0.3f, 0.5f);
                behavior = EnemyBehavior.Backstabber;
                break;
            case "automata":
                baseHp = 160; baseDamage = 12; baseDefense = 5;
                art = "tank"; name = "Autómata de Reliquias"; scale = 1.1f;
                tint = new Color(0.7f, 0.6f, 0.4f);
                behavior = EnemyBehavior.CCImmune;
                break;

            case "penitent":
            default:
                // 0.8-fix: Cruzado con lanza (rango 2) y sangrado pequeño
                baseHp = 85; baseDamage = 16; baseAttackRange = 2; art = "penitent";
                name = "Cruzado"; scale = 0.8f;
                applyBleed = true;
                break;
        }

        int hp = Mathf.RoundToInt(baseHp * hpMult);
        int playerLevel = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        hp = Mathf.RoundToInt(hp * Progression.EnemyHpMult(playerLevel));

        Unit unit = Unit.Create(name, cell, true, tint, scale, hp, 3, art);

        // 1.9: Stats de combate de enemigos (accuracy, evasion, defense)
        int baseAccuracy = 75;  // Enemigos golpean más consistentemente
        int baseEvasion = 0;    // Enemigos básicos no evaden
        
        // Escalado por tier
        if (tier == EnemyTier.Medio) { baseAccuracy += 5; baseEvasion += 3; }
        if (tier == EnemyTier.Elite) { baseAccuracy += 10; baseEvasion += 8; baseDefense += 5; }
        if (tier == EnemyTier.EliteFuerte) { baseAccuracy += 15; baseEvasion += 12; baseDefense += 8; }
        if (tier == EnemyTier.Jefe) { baseAccuracy += 20; baseEvasion += 15; baseDefense += 12; }

        unit.stats.accuracy = baseAccuracy;
        unit.stats.evasion = baseEvasion;
        unit.stats.defense = baseDefense;

        if (archetype == "capitan_mundial")
        {
            // 0.7-F.1d: el Capitán usa IA propia con mecánicas
            CaptainBossAI ai = unit.gameObject.AddComponent<CaptainBossAI>();
            ai.attackDamage = baseDamage + dmgBonus + Progression.EnemyDamageBonus(playerLevel);
            ai.tier = tier;
        }
        else if (archetype == "boss" || archetype == "angel")
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
            ai.applyBleed = applyBleed;
            ai.unitName = name;
        }

        // 0.7-D.2b: Marcar flags de boss/elite según tier y arquetipo
        if (tier == EnemyTier.Jefe || archetype == "boss" || archetype == "angel")
        {
            unit.isBoss = true;
        }
        else if (tier == EnemyTier.Elite || tier == EnemyTier.EliteFuerte)
        {
            unit.isElite = true;
        }

        // 0.8-fix: Autómata de Reliquias inmune a control
        if (behavior == EnemyBehavior.CCImmune)
        {
            unit.isCCImmune = true;
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