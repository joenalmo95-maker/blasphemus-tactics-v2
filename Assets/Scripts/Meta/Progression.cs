using UnityEngine;

public static class Progression
{
    // 0.3: Escalado de daño del jugador (+3 por nivel)
    public static int PlayerDamageBonus(int level)
    {
        return (level - 1) * 3;
    }

    // Escalado de HP de enemigos (+4% por nivel)
    public static float EnemyHpMult(int playerLevel)
    {
        return 1f + (playerLevel - 1) * 0.04f;
    }

    // Escalado de daño de enemigos (+1 cada 3 niveles)
    public static int EnemyDamageBonus(int playerLevel)
    {
        return (playerLevel - 1) / 3;
    }
}