using UnityEngine;

public static class Progression
{
    // 0.3: Escalado de daño del jugador (+3 por nivel)
    public static int PlayerDamageBonus(int level)
    {
        return (level - 1) * 3;
    }

    // 1.4-B: Escalado de HP de enemigos (+7% por nivel → x3.03 al nivel 30)
    public static float EnemyHpMult(int playerLevel)
    {
        return 1f + (playerLevel - 1) * 0.07f;
    }

    // 1.4-B: Escalado de daño de enemigos (+1 cada 2 niveles → +14 al nivel 30)
    public static int EnemyDamageBonus(int playerLevel)
    {
        return (playerLevel - 1) / 2;
    }
}