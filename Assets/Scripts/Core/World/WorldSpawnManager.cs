using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldSpawnManager : MonoBehaviour
{
    public const float RespawnSeconds = 90f;

    public class SpawnDefW
    {
        public int id;
        public string archetype;
        public EnemyTier tier;
        public Vector2Int cell;
    }

    class ActiveEnemy
    {
        public SpawnDefW def;
        public Vector2Int cell;
        public GameObject go;
    }

    // HOOK RESERVADO (Bloque 6.2 — materiales de mejora)
    public static System.Action<SpawnDefW> OnWorldEnemyDefeated;

    // Persiste entre escenas dentro de la sesión para el respawn por tiempo
    public static readonly Dictionary<int, float> DefeatedAt = new Dictionary<int, float>();

    private readonly List<SpawnDefW> defs = new List<SpawnDefW>
    {
        // Campos de Penitencia (cerca del Bastión, fáciles)
        new SpawnDefW { id = 1, archetype = "penitent",   tier = EnemyTier.Basico,     cell = new Vector2Int(8, 35) },
        new SpawnDefW { id = 2, archetype = "penitent",   tier = EnemyTier.Basico,     cell = new Vector2Int(12, 32) },
        new SpawnDefW { id = 3, archetype = "flagelante", tier = EnemyTier.Basico,     cell = new Vector2Int(15, 30) },
        
        // Camino de los Peregrinos (zona media)
        new SpawnDefW { id = 4, archetype = "cherub",     tier = EnemyTier.Medio,      cell = new Vector2Int(25, 25) },
        new SpawnDefW { id = 5, archetype = "censor",     tier = EnemyTier.Medio,      cell = new Vector2Int(30, 20) },
        new SpawnDefW { id = 6, archetype = "ceniza",     tier = EnemyTier.Medio,      cell = new Vector2Int(35, 18) },
        new SpawnDefW { id = 7, archetype = "incensario", tier = EnemyTier.Medio,      cell = new Vector2Int(28, 22) },
        
        // Bosque de los Cirios (zona peligrosa)
        new SpawnDefW { id = 8, archetype = "heraldo",    tier = EnemyTier.Elite,      cell = new Vector2Int(18, 25) },
        new SpawnDefW { id = 9, archetype = "automata",   tier = EnemyTier.Elite,      cell = new Vector2Int(22, 28) },
        new SpawnDefW { id = 10, archetype = "inquisitor",tier = EnemyTier.Elite,      cell = new Vector2Int(20, 30) },
        
        // Canteras de Aurelia (zona élite)
        new SpawnDefW { id = 11, archetype = "capitan",   tier = EnemyTier.EliteFuerte,cell = new Vector2Int(45, 8) },
        new SpawnDefW { id = 12, archetype = "automata",  tier = EnemyTier.EliteFuerte,cell = new Vector2Int(50, 10) },
        new SpawnDefW { id = 13, archetype = "heraldo",   tier = EnemyTier.EliteFuerte,cell = new Vector2Int(48, 6) }
    };

    private readonly List<ActiveEnemy> active = new List<ActiveEnemy>();
    private Text promptText;
    private ActiveEnemy nearEnemy;

    void Awake()
    {
        BuildPrompt();
        SpawnAll();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldSpawnPromptCanvas", 41);
        promptText = UIFactory.CreateText(canvas.transform, "EnemyPrompt", "", 18, TextAnchor.MiddleCenter,
            new Color(1f, 0.4f, 0.4f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 150), new Vector2(700, 40));
    }

    void SpawnAll()
    {
        float now = Time.realtimeSinceStartup;

        foreach (SpawnDefW d in defs)
        {
            // Respawn por tiempo: si cayó hace menos de RespawnSeconds, permanece ausente
            if (DefeatedAt.TryGetValue(d.id, out float t) && (now - t) < RespawnSeconds) continue;

            // NUNCA sobre el terreno: busca la celda caminable más cercana sin modificar el mapa
            Vector2Int cell = FindFreeCellNear(d.cell, 4);
            if (cell.x < 0) continue; // sin hueco seguro: se omite este spawn

            GameObject go = new GameObject("WorldEnemy_" + d.archetype + "_" + d.id);
            go.transform.position = new Vector3(cell.x, cell.y, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtProvider.Get(d.archetype);
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 0.8f;

            active.Add(new ActiveEnemy { def = d, cell = cell, go = go });
        }
    }

    // Búsqueda en anillos concéntricos alrededor de la celda deseada
    Vector2Int FindFreeCellNear(Vector2Int desired, int maxRadius)
    {
        for (int r = 0; r <= maxRadius; r++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                    Vector2Int c = desired + new Vector2Int(dx, dy);
                    if (IsFreeForSpawn(c)) return c;
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool IsFreeForSpawn(Vector2Int c)
    {
        if (c.x < 0 || c.y < 0 || c.x >= WorldBootstrap.WorldWidth || c.y >= WorldBootstrap.WorldHeight) return false;
        if (!TerrainMap.IsWalkable(c)) return false;               // respeta rocas/agua/ruinas
        if (c == WorldBootstrap.PlayerSpawn) return false;         // no sobre el jugador

        // No sobre zonas ni su radio de prompt (evita doble prompt con E)
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (Mathf.Abs(z.center.x - c.x) <= 2 && Mathf.Abs(z.center.y - c.y) <= 2) return false;
        }

        // No sobre otro monstruo
        foreach (ActiveEnemy e in active)
        {
            if (e.cell == c) return false;
        }

        return true;
    }

    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));

        nearEnemy = null;
        foreach (ActiveEnemy e in active)
        {
            if (Mathf.Abs(e.cell.x - myCell.x) <= 1 && Mathf.Abs(e.cell.y - myCell.y) <= 1)
            {
                nearEnemy = e;
                break;
            }
        }

        if (nearEnemy != null)
        {
            promptText.text = "Pulsa E para combatir: " + nearEnemy.def.archetype + " (" + nearEnemy.def.tier + ")";
            if (Input.GetKeyDown(KeyCode.E)) EnterCombat(nearEnemy);
        }
        else
        {
            promptText.text = "";
        }
    }

    void EnterCombat(ActiveEnemy e)
    {
        DefeatedAt[e.def.id] = Time.realtimeSinceStartup;
        if (OnWorldEnemyDefeated != null) OnWorldEnemyDefeated(e.def);

        List<WaveDef> dungeon = new List<WaveDef>
        {
            new WaveDef { spawns = new List<SpawnDef>
            {
                new SpawnDef { archetype = e.def.archetype, tier = e.def.tier, cell = new Vector2Int(7, 4) }
            } }
        };

        GameFlow.pendingIsWorld = true;
        GameFlow.EnterCombat(e.def.tier, dungeon);
    }
}