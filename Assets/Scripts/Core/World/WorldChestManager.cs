using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldChestManager : MonoBehaviour
{
    public const float RespawnSeconds = 180f;

    public class ChestDef
    {
        public int id;
        public Vector2Int cell;
        public EnemyTier tier;
        public string[] guardianArchetypes; // arquetipos de los guardianes del cubil
    }

    class ActiveChest
    {
        public ChestDef def;
        public Vector2Int cell;
        public GameObject go;
        public bool opened;
    }

    // Persiste entre escenas dentro de la sesión para el respawn por tiempo
    public static readonly Dictionary<int, float> OpenedAt = new Dictionary<int, float>();

    private readonly List<ChestDef> defs = new List<ChestDef>
    {
        new ChestDef { id = 1, cell = new Vector2Int(12, 15), tier = EnemyTier.Basico, guardianArchetypes = new[] { "penitent", "penitent" } },
        new ChestDef { id = 2, cell = new Vector2Int(22, 8), tier = EnemyTier.Medio, guardianArchetypes = new[] { "cherub", "inquisitor" } },
        new ChestDef { id = 3, cell = new Vector2Int(35, 25), tier = EnemyTier.Elite, guardianArchetypes = new[] { "capitan", "cherub" } },
        new ChestDef { id = 4, cell = new Vector2Int(48, 18), tier = EnemyTier.Elite, guardianArchetypes = new[] { "inquisitor", "inquisitor", "penitent" } },
        new ChestDef { id = 5, cell = new Vector2Int(55, 35), tier = EnemyTier.EliteFuerte, guardianArchetypes = new[] { "capitan", "capitan" } }
    };

    private readonly List<ActiveChest> active = new List<ActiveChest>();
    private Text promptText;
    private ActiveChest nearChest;

    void Awake()
    {
        BuildPrompt();
        SpawnAll();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldChestPromptCanvas", 42);
        promptText = UIFactory.CreateText(canvas.transform, "ChestPrompt", "", 18, TextAnchor.MiddleCenter,
            new Color(1f, 0.85f, 0.2f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 180), new Vector2(700, 40));
    }

    void SpawnAll()
    {
        float now = Time.realtimeSinceStartup;

        foreach (ChestDef d in defs)
        {
            // Respawn por tiempo: si se abrió hace menos de RespawnSeconds, permanece ausente
            if (OpenedAt.TryGetValue(d.id, out float t) && (now - t) < RespawnSeconds) continue;

            // Busca celda caminable cercana sin modificar el terreno
            Vector2Int cell = FindFreeCellNear(d.cell, 4);
            if (cell.x < 0) continue;

            GameObject go = new GameObject("WorldChest_" + d.id);
            go.transform.position = new Vector3(cell.x, cell.y, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtProvider.Get("chest");
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 0.8f;

            active.Add(new ActiveChest { def = d, cell = cell, go = go, opened = false });
        }
    }

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
                    if (IsFreeForChest(c)) return c;
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool IsFreeForChest(Vector2Int c)
    {
        if (c.x < 0 || c.y < 0 || c.x >= WorldBootstrap.WorldWidth || c.y >= WorldBootstrap.WorldHeight) return false;
        if (!TerrainMap.IsWalkable(c)) return false;
        if (c == WorldBootstrap.PlayerSpawn) return false;

        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (Mathf.Abs(z.center.x - c.x) <= 2 && Mathf.Abs(z.center.y - c.y) <= 2) return false;
        }

        foreach (ActiveChest ch in active)
        {
            if (ch.cell == c) return false;
        }

        return true;
    }

    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));

        nearChest = null;
        foreach (ActiveChest ch in active)
        {
            if (Mathf.Abs(ch.cell.x - myCell.x) <= 1 && Mathf.Abs(ch.cell.y - myCell.y) <= 1)
            {
                nearChest = ch;
                break;
            }
        }

        if (nearChest != null && !nearChest.opened)
        {
            promptText.text = "Pulsa E para abrir cofre (custodiado por " + nearChest.def.tier + ")";
            if (Input.GetKeyDown(KeyCode.E)) OpenChest(nearChest);
        }
        else
        {
            promptText.text = "";
        }
    }

    void OpenChest(ActiveChest ch)
    {
        ch.opened = true;
        OpenedAt[ch.def.id] = Time.realtimeSinceStartup;

        // Construye oleada de guardianes
        List<WaveDef> dungeon = new List<WaveDef>();
        WaveDef wave = new WaveDef { spawns = new List<SpawnDef>() };

        int spawnX = 7;
        foreach (string arch in ch.def.guardianArchetypes)
        {
            wave.spawns.Add(new SpawnDef { archetype = arch, tier = ch.def.tier, cell = new Vector2Int(spawnX, 4) });
            spawnX++;
            if (spawnX > 9) spawnX = 7;
        }

        dungeon.Add(wave);

        // Marca el cofre como derrotado para dar loot directo al vencer
        PendingChestLoot = ch.def.tier;

        GameFlow.EnterCombat(ch.def.tier, dungeon);
    }

    // Hook estático para que el Bootstrap de combate sepa que debe dar loot de cofre
    public static EnemyTier? PendingChestLoot = null;
}