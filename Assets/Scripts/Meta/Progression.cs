using UnityEngine;

// 4.2: fuente única de verdad del escalado por nivel (simétrico personajes/enemigos)
public static class Progression
{
    // Personaje: +1 daño cada 2 niveles
    public static int PlayerDamageBonus(int level)
    {
        return level / 2;
    }

    // Enemigos: +4% HP por nivel del jugador
    public static float EnemyHpMult(int playerLevel)
    {
        return 1f + playerLevel * 0.04f;
    }

    // Enemigos: +1 daño cada 3 niveles del jugador
    public static int EnemyDamageBonus(int playerLevel)
    {
        return playerLevel / 3;
    }
}