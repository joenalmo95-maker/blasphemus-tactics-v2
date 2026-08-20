using UnityEngine;

// 5.3: objetivos claros + detección de zonas de jefe para marcadores
public static class ObjectiveSystem
{
    public static bool BossDefeated
    {
        get { return PlayerPrefs.GetInt("BossDefeated", 0) == 1; }
    }

    public static void MarkBossDefeated()
    {
        PlayerPrefs.SetInt("BossDefeated", 1);
        PlayerPrefs.Save();
    }

    public static bool HasBoss(WorldBootstrap.ZoneDef z)
    {
        if (z == null || z.dungeon == null) return false;
        foreach (WorldBootstrap.WaveDef w in z.dungeon)
        {
            if (w == null || w.spawns == null) continue;
            foreach (WorldBootstrap.SpawnDef s in w.spawns)
            {
                if (s.tier == EnemyTier.Jefe || s.archetype == "boss" || s.archetype == "angel") return true;
            }
        }
        return false;
    }

    public static string BossZoneName()
    {
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (HasBoss(z)) return z.name;
        }
        return "zona de jefe";
    }

    public static string Current()
    {
        if (BossDefeated) return "LA LITURGIA HA SIDO DETENIDA";
        string bossZone = BossZoneName();
        return "Investiga " + bossZone + " (marcada en el mapa)";
    }
}