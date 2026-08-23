using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// 0.7-F.1c: Sistema de Boss Mundial (ciclo 7 días reales)
public class WorldBossSystem : MonoBehaviour
{
    private const int SPAWN_DELAY_HOURS = 24; // Primer boss: 24h tras crear partida
    private const int RESPAWN_DAYS = 7; // Siguiente: 7 días tras derrotar
    private const int MIN_DISTANCE_FROM_PLAYER = 20; // Distancia mínima del spawn

    private Text promptText;
    private GameObject bossMarker;
    private Vector2Int bossPosition;
    private bool isBossActive = false;

    void Awake()
    {
        BuildPrompt();
        TrySpawnBoss();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("BossPromptCanvas", 42);
        promptText = UIFactory.CreateText(canvas.transform, "BossPrompt", "", 18, TextAnchor.MiddleCenter,
            new Color(1f, 0.85f, 0.3f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 200), new Vector2(800, 40));
    }

    void TrySpawnBoss()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 1. Verificar si hay un boss activo y si ha expirado (24 horas)
        if (PlayerPrefs.GetInt("WorldBossActive", 0) == 1)
        {
            long spawnTime = long.Parse(PlayerPrefs.GetString("BossSpawnTime", "0"));
            if (spawnTime > 0 && (currentTime - spawnTime) >= 24 * 3600)
            {
                Debug.Log("[WorldBoss] El Capitán ha expirado (24h sin ser derrotado). Desapareciendo...");
                PlayerPrefs.SetInt("WorldBossActive", 0);
                PlayerPrefs.DeleteKey("BossSpawnTime");
                PlayerPrefs.DeleteKey("BossSpawnX");
                PlayerPrefs.DeleteKey("BossSpawnY");
                PlayerPrefs.Save();
                if (bossMarker != null) Destroy(bossMarker);
                isBossActive = false;
            }
            else
            {
                Debug.Log("[WorldBoss] Boss activo y vigente. Restaurando marcador en el mapa...");
                // FIX: Restaurar la posición guardada y reconstruir el marcador visual
                int savedX = PlayerPrefs.GetInt("BossSpawnX", -1);
                int savedY = PlayerPrefs.GetInt("BossSpawnY", -1);
                if (savedX != -1 && savedY != -1)
                {
                    bossPosition = new Vector2Int(savedX, savedY);
                    isBossActive = true;
                    RebuildBossMarker(bossPosition);
                }
                return; // Ya está activo y restaurado visualmente
            }
        }

        // 2. Verificar si ha pasado tiempo suficiente desde el último kill para un nuevo spawn
        long lastKillTime = long.Parse(PlayerPrefs.GetString("LastBossKillTime", "0"));
        long secondsSinceLastKill = currentTime - lastKillTime;

        if (lastKillTime == 0)
        {
            if (secondsSinceLastKill < 24 * 3600)
            {
                Debug.Log("[WorldBoss] Primer boss aparecerá en " + ((24 * 3600 - secondsSinceLastKill) / 3600) + " horas.");
                return;
            }
        }
        else
        {
            if (secondsSinceLastKill < 7 * 24 * 3600)
            {
                Debug.Log("[WorldBoss] Próximo boss en " + ((7 * 24 * 3600 - secondsSinceLastKill) / 3600) + " horas.");
                return;
            }
        }

        // 3. Spawnear el boss
        SpawnBoss(currentTime);
    }

    void SpawnBoss(long currentTime)
    {
        Vector2Int playerPos = WorldBootstrap.LastKnownPosition;
        Vector2Int spawnCell = Vector2Int.zero;
        bool found = false;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = UnityEngine.Random.Range(0, WorldBootstrap.WorldWidth);
            int y = UnityEngine.Random.Range(0, WorldBootstrap.WorldHeight);
            Vector2Int candidate = new Vector2Int(x, y);

            if (!TerrainMap.IsWalkable(candidate)) continue;
            if (Vector2Int.Distance(candidate, playerPos) < 20) continue;

            spawnCell = candidate;
            found = true;
            break;
        }

        if (!found)
        {
            Debug.LogWarning("[WorldBoss] No se encontró celda válida para spawn del boss.");
            return;
        }

        bossPosition = spawnCell;
        isBossActive = true;
        PlayerPrefs.SetInt("WorldBossActive", 1);
        PlayerPrefs.SetString("BossSpawnTime", currentTime.ToString());
        PlayerPrefs.SetInt("BossSpawnX", spawnCell.x); // ← FIX: Guardar X
        PlayerPrefs.SetInt("BossSpawnY", spawnCell.y); // ← FIX: Guardar Y
        PlayerPrefs.Save();

        RebuildBossMarker(spawnCell);
        Debug.Log("[WorldBoss] ★ Capitán spawneado en (" + spawnCell.x + ", " + spawnCell.y + ")");
    }

    // ← FIX: Nuevo método para reconstruir el marcador visual (se usa al spawnear Y al recargar el mundo)
    void RebuildBossMarker(Vector2Int pos)
    {
        if (bossMarker != null) Destroy(bossMarker);
        
        bossMarker = new GameObject("BossMarker_Capitan");
        bossMarker.transform.position = new Vector3(pos.x, pos.y, 0);

        SpriteRenderer sr = bossMarker.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = new Color(1f, 0.85f, 0.3f, 0.9f); // Dorado
        sr.sortingOrder = 1;
        bossMarker.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

        GameObject border = new GameObject("Border");
        border.transform.SetParent(bossMarker.transform);
        border.transform.localPosition = Vector3.zero;
        SpriteRenderer brdSr = border.AddComponent<SpriteRenderer>();
        brdSr.sprite = SpriteFactory.Square();
        brdSr.color = Color.black;
        brdSr.sortingOrder = 0;
        border.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
    }

    // Llamado desde TurnManager al ganar el combate del boss
    public static void OnBossDefeated()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString("LastBossKillTime", currentTime.ToString());
        PlayerPrefs.SetInt("WorldBossActive", 0);
        PlayerPrefs.DeleteKey("BossSpawnTime");
        PlayerPrefs.DeleteKey("BossSpawnX"); // ← FIX: Limpiar X
        PlayerPrefs.DeleteKey("BossSpawnY"); // ← FIX: Limpiar Y
        PlayerPrefs.Save();
        Debug.Log("[WorldBoss] ★ Capitán derrotado. Próximo boss en 7 días.");
    }
    void Update()
    {
        // Tecla F10: forzar spawn (testing)
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("[WorldBoss] F10: Forzando spawn del Capitán...");
            ForceSpawn();
        }

        if (!isBossActive || promptText == null) return;

        WorldPlayerController pc = UnityEngine.Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));

        // Verificar si el jugador está cerca del boss
        if (Mathf.Abs(bossPosition.x - myCell.x) <= 2 && Mathf.Abs(bossPosition.y - myCell.y) <= 2)
        {
            promptText.text = "Pulsa E para desafiar al Capitán (Boss Mundial)";
            if (Input.GetKeyDown(KeyCode.E)) StartBossFight();
        }
        else
        {
            promptText.text = "";
        }
    }

    void ForceSpawn()
    {
        // Eliminar boss existente si hay
        if (bossMarker != null) Destroy(bossMarker);
        isBossActive = false;
        PlayerPrefs.SetInt("WorldBossActive", 0);

        // Forzar spawn inmediato (FIX: pasar el tiempo actual)
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SpawnBoss(currentTime);
    }

    void StartBossFight()
    {
        Debug.Log("[WorldBoss] Iniciando combate contra el Capitán...");

        // Marcar que estamos en combate de boss mundial
        GameFlow.pendingWorldBoss = true;

        // Crear oleada: 1 Capitán + 4 élites
        List<WaveDef> waves = new List<WaveDef>();
        WaveDef bossWave = new WaveDef();
        bossWave.spawns = new List<SpawnDef>
        {
            new SpawnDef { archetype = "capitan_mundial", tier = EnemyTier.Jefe, cell = new Vector2Int(7, 4) },
            new SpawnDef { archetype = "heraldo", tier = EnemyTier.EliteFuerte, cell = new Vector2Int(5, 4) },
            new SpawnDef { archetype = "automata", tier = EnemyTier.EliteFuerte, cell = new Vector2Int(6, 4) },
            new SpawnDef { archetype = "capitan", tier = EnemyTier.EliteFuerte, cell = new Vector2Int(8, 4) },
            new SpawnDef { archetype = "inquisitor", tier = EnemyTier.EliteFuerte, cell = new Vector2Int(9, 4) }
        };
        waves.Add(bossWave);

        GameFlow.pendingIsWorld = false;
        GameFlow.EnterCombat(EnemyTier.Jefe, waves);
    }
}