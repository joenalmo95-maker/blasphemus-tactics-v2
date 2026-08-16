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
    public const string WorldScene = "WorldMap";
    public const string CombatScene = "SampleScene";

    public static void EnterCombat(EnemyTier tier, List<WaveDef> dungeon)
    {
        pendingTier = tier;
        pendingDungeon = dungeon;
        SceneManager.LoadScene(CombatScene);
    }

    public const string CityScene = "CityScene";

    public static void EnterCity()
    {
        SceneManager.LoadScene(CityScene);
    }

    public static void ReturnToWorld()
    {
        SceneManager.LoadScene(WorldScene);
    }
}