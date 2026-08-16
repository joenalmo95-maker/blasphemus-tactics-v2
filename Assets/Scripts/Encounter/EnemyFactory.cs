using UnityEngine;

public static class EnemyFactory
{
    public static Unit Spawn(string archetype, EnemyTier tier, Vector2Int cell)
    {
        cell = FindFreeCell(cell);

        float hpMult = 1f;
        int dmgBonus = 0;
        switch (tier)
        {
            case EnemyTier.Medio: hpMult = 1.4f; dmgBonus = 1; break;
            case EnemyTier.Elite: hpMult = 1.8f; dmgBonus = 2; break;
            case EnemyTier.EliteFuerte: hpMult = 2.2f; dmgBonus = 3; break;
            case EnemyTier.Jefe: hpMult = 3f; dmgBonus = 4; break;
        }

        bool boss = archetype == "boss";
        bool cherub = archetype == "cherub";

        int baseHp = boss ? 40 : (cherub ? 6 : 10);
        int hp = boss ? baseHp : Mathf.RoundToInt(baseHp * hpMult);
        string art = boss ? "angel" : (cherub ? "cherub" : "penitent");
        float scale = boss ? 1.6f : (cherub ? 0.7f : 0.8f);
        string name = boss ? "Ángel de la Vigilia" : (cherub ? "Querubín" : "Cruzado");

        Unit unit = Unit.Create(name, cell, true, Color.white, scale, hp, 3, art);

        if (boss)
        {
            BossAI ai = unit.gameObject.AddComponent<BossAI>();
            ai.attackDamage = 4;
            ai.tier = tier;
        }
        else
        {
            EnemyAI ai = unit.gameObject.AddComponent<EnemyAI>();
            ai.tier = tier;
            ai.attackDamage = 2 + dmgBonus;
            ai.attackRange = cherub ? 3 : 1;
            ai.moveRange = 2;
        }

        return unit;
    }

    static Vector2Int FindFreeCell(Vector2Int desired)
    {
        if (GridManager.Instance.InBounds(desired) && !Pathfinding.IsOccupied(desired))
            return desired;

        for (int r = 1; r <= 4; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                    Vector2Int c = desired + new Vector2Int(dx, dy);
                    if (GridManager.Instance.InBounds(c) && !Pathfinding.IsOccupied(c))
                        return c;
                }
            }
        }

        return desired;
    }
}