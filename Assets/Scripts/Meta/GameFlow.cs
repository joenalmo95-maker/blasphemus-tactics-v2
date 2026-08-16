using UnityEngine.SceneManagement;

public static class GameFlow
{
    public static EnemyTier pendingTier = EnemyTier.Basico;
    public const string WorldScene = "WorldMap";
    public const string CombatScene = "SampleScene";

    public static void EnterCombat(EnemyTier tier)
    {
        pendingTier = tier;
        SceneManager.LoadScene(CombatScene);
    }

    public static void ReturnToWorld()
    {
        SceneManager.LoadScene(WorldScene);
    }
}