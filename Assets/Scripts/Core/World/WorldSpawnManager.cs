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
        public GameObject go;
    }

    // HOOK RESERVADO (Bloque 6.2 — materiales de mejora)
    public static System.Action<SpawnDefW> OnWorldEnemyDefeated;

    // Persiste entre escenas dentro de la sesión para el respawn por tiempo
    public static readonly Dictionary<int, float> DefeatedAt = new Dictionary<int, float>();

    private readonly List<SpawnDefW> defs = new List<SpawnDefW>
    {
        new SpawnDefW { id = 1, archetype = "penitent",   tier = EnemyTier.Basico,     cell = new Vector2Int(6, 12) },
        new SpawnDefW { id = 2, archetype = "penitent",   tier = EnemyTier.Basico,     cell = new Vector2Int(14, 6) },
        new SpawnDefW { id = 3, archetype = "cherub",     tier = EnemyTier.Medio,      cell = new Vector2Int(24, 14) },
        new SpawnDefW { id = 4, archetype = "inquisitor", tier = EnemyTier.Medio,      cell = new Vector2Int(26, 26) },
        new SpawnDefW { id = 5, archetype = "capitan",    tier = EnemyTier.Elite,      cell = new Vector2Int(44, 20) },
        new SpawnDefW { id = 6, archetype = "cherub",     tier = EnemyTier.Elite,      cell = new Vector2Int(52, 12) },
        new SpawnDefW { id = 7, archetype = "capitan",    tier = EnemyTier.EliteFuerte,cell = new Vector2Int(46, 34) }
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

            ClearAround(d.cell);

            GameObject go = new GameObject("WorldEnemy_" + d.archetype + "_" + d.id);
            go.transform.position = new Vector3(d.cell.x, d.cell.y, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtProvider.Get(d.archetype);
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 0.8f;

            active.Add(new ActiveEnemy { def = d, go = go });
        }
    }

    static void ClearAround(Vector2Int c)
    {
        TerrainMap.Set(c, TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(1, 0), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(-1, 0), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(0, 1), TerrainType.Caminable);
        TerrainMap.Set(c + new Vector2Int(0, -1), TerrainType.Caminable);
    }

    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));

        nearEnemy = null;
        foreach (ActiveEnemy e in active)
        {
            if (Mathf.Abs(e.def.cell.x - myCell.x) <= 1 && Mathf.Abs(e.def.cell.y - myCell.y) <= 1)
            {
                nearEnemy = e;
                break;
            }
        }

        if (nearEnemy != null)
        {
            promptText.text = "Pulsa E para combatir: " + nearEnemy.def.archetype + " (" + nearEnemy.def.tier + ")";
            if (Input.GetKeyDown(KeyCode.E))
            {
                EnterCombat(nearEnemy);
            }
        }
        else
        {
            promptText.text = "";
        }
    }

    void EnterCombat(ActiveEnemy e)
    {
        // Marca el inicio del combate para el respawn por tiempo
        DefeatedAt[e.def.id] = Time.realtimeSinceStartup;

        // Hook reservado para materiales de mejora (6.2)
        if (OnWorldEnemyDefeated != null) OnWorldEnemyDefeated(e.def);

        // Reutiliza el flujo existente: 1 oleada con el arquetipo del mundo
        List<WaveDef> dungeon = new List<WaveDef>
        {
            new WaveDef { spawns = new List<SpawnDef>
            {
                new SpawnDef { archetype = e.def.archetype, tier = e.def.tier, cell = new Vector2Int(7, 4) }
            } }
        };

        GameFlow.EnterCombat(e.def.tier, dungeon);
    }
}