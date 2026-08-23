using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class SpawnDef
{
    public string archetype;
    public EnemyTier tier;
    public Vector2Int cell;
}

[System.Serializable]
public class WaveDef
{
    public List<SpawnDef> spawns = new List<SpawnDef>();
}

public static class GameFlow
{
    public static EnemyTier pendingTier = EnemyTier.Basico;
    public static List<WaveDef> pendingDungeon = null;
    public static bool pendingIsWorld = false;
    // 0.7-E.4: zona actual (para drops de set al ganar DNG)
    public static WorldBootstrap.ZoneDef pendingZone = null;
    // 0.7-F.1c: flag para combate de Boss Mundial
    public static bool pendingWorldBoss = false;

    public const string MainMenuScene = "MainMenu";
    public const string WorldScene = "WorldMap";
    public const string CombatScene = "SampleScene";
    public const string CityScene = "CityScene";

    public static void EnterCombat(EnemyTier tier, List<WaveDef> dungeon)
    {
        pendingTier = tier;
        pendingDungeon = dungeon;
        SceneManager.LoadScene(CombatScene);
    }

    public static void EnterCity()
    {
        SceneManager.LoadScene(CityScene);
    }

    public static void ReturnToWorld()
    {
        SceneManager.LoadScene(WorldScene);
    }

    public static void ReturnToMainMenu()
    {
        SaveSystem.Save();
        SceneManager.LoadScene(MainMenuScene);
    }
}