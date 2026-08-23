using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum TurnState { WaitingForPlayer, PlayerTurn, EnemyTurn, GameOver }
    public TurnState currentState = TurnState.WaitingForPlayer;

    private Unit playerUnit;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (currentState == TurnState.WaitingForPlayer)
        {
            playerUnit = GetPlayer();
            if (playerUnit != null) StartPlayerTurn();
        }
    }

    public void BeginGame()
    {
        if (currentState != TurnState.WaitingForPlayer) return;
        StartPlayerTurn();
    }

    Unit GetPlayer()
    {
        if (playerUnit != null) return playerUnit;

        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) return u;
        }
        return null;
    }

    public void StartPlayerTurn()
    {
        if (currentState == TurnState.GameOver) return;
        currentState = TurnState.PlayerTurn;
        Debug.Log("=== TURNO DEL JUGADOR ===");

        playerUnit = GetPlayer();
        if (playerUnit != null)
        {
            playerUnit.ResetAP();
            Debug.Log("AP restaurados: " + playerUnit.currentAP);

            if (playerUnit.pendingApPenalty > 0)
            {
                playerUnit.currentAP = Mathf.Max(0, playerUnit.currentAP - playerUnit.pendingApPenalty);
                Debug.Log("AP drenados: " + playerUnit.pendingApPenalty + ". AP este turno: " + playerUnit.currentAP);
                playerUnit.pendingApPenalty = 0;
            }

         playerUnit.TickBuffs();
         playerUnit.TickDebuffs();
     }

     // 1.1-E: telegrafía la intención enemiga durante el turno del jugador
     EnemyIntentSystem.DecideAll();
    
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn) return;
        Debug.Log("Jugador termina turno.");
        // 1.1-E.7: cooldown de ultimate y limpieza de indicadores
        CombatController cc = FindAnyObjectByType<CombatController>();
        if (cc != null) cc.EndPlayerTurn();
        currentState = TurnState.EnemyTurn;
        StartCoroutine(ExecuteEnemyTurns());
    }

    IEnumerator ExecuteEnemyTurns()
    {
        yield return new WaitForSeconds(0.5f);

        Unit[] currentEnemies = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit enemy in currentEnemies)
        {
            if (enemy == null || !enemy.isEnemy) continue;
            if (currentState == TurnState.GameOver) yield break;

            Debug.Log("Turno de: " + enemy.gameObject.name);
            
            BossAI boss = enemy.GetComponent<BossAI>();
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            
            if (boss != null)
            {
                yield return boss.ExecuteTurn();
            }
            else if (ai != null)
            {
                yield return ai.ExecuteTurn();
            }
            yield return new WaitForSeconds(0.5f);
        }

        if (currentState == TurnState.GameOver) yield break;
        yield return new WaitForSeconds(0.3f);
        StartPlayerTurn();
    }

    public void NotifyUnitDeath(bool wasEnemy)
    {
        if (currentState == TurnState.GameOver) return;

        if (!wasEnemy)
        {
            ForceGameOver();
            Debug.Log("=== DERROTA ===");
            ShowGameOver(false);
            return;
        }

        Unit[] remaining = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        bool anyEnemyVivo = false;
        foreach (Unit u in remaining)
        {
            if (u.isEnemy && u.currentHealth > 0)
            {
                anyEnemyVivo = true;
                break;
            }
        }

        if (!anyEnemyVivo)
        {
            if (DungeonManager.Instance != null && DungeonManager.Instance.HasNextWave())
            {
                DungeonManager.Instance.NextWave();
            }
            else
            {
                ForceGameOver();
                Debug.Log("=== VICTORIA ===");
                // 2.1: progreso de misiones (mazmorra completada)
                QuestSystem.NotifyDungeonCompleted();
                // 0.7-E.4: drop de pieza de set según zona completada
                LootSystem.OnDungeonVictory(GameFlow.pendingZone);
                GameFlow.pendingZone = null;
                ShowGameOver(true);
            }
        }
    }

    void ShowGameOver(bool victory)
    {
        GameOverUI ui = GameOverUI.Instance != null ? GameOverUI.Instance : new GameObject("GameOverUI").AddComponent<GameOverUI>();
        ui.Show(victory);
    }

    public void ForceGameOver()
    {
        currentState = TurnState.GameOver;
    }

    public bool IsPlayerTurn()
    {
        return currentState == TurnState.PlayerTurn;
    }
}