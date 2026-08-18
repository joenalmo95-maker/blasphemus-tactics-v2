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
        foreach (WaveDef w in z.dungeon)
        {
            if (w == null || w.spawns == null) continue;
            foreach (SpawnDef s in w.spawns)
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
        int level = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        string daily = "Mazmorras hoy: " + DungeonDaily.Count + "/" + DungeonDaily.MaxPerDay;

        string main;
        if (BossDefeated) main = "JEFE derrotado: granjea legendarios o explora.";
        else if (level < 5) main = "Objetivo: mazmorras Básicas hasta Nv 5.";
        else if (level < 10) main = "Objetivo: mazmorras Medias hasta Nv 10.";
        else if (level < 15) main = "Objetivo: mazmorras Elite hasta Nv 15.";
        else main = "Objetivo FINAL: derrota al JEFE (" + BossZoneName() + ").";

        return main + "  |  " + daily;
    }
}