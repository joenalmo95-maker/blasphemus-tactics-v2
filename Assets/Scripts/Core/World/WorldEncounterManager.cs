using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldEncounterManager : MonoBehaviour
{
    public const float DespawnSeconds = 3600f; // 60 min sin interacción
    public const float EmboscadaCooldown = 240f; // 4 min
    public const float TesoroCooldown = 900f; // 15 min
    public const float SantuarioCooldown = 1800f; // 30 min
    public const float MercaderCooldown = 2700f; // 45 min
    public const float CazadorCooldown = 1800f; // 30 min
    
    public static readonly Dictionary<int, float> Cooldowns = new Dictionary<int, float>();
    
    private readonly List<Encounter> encounters = new List<Encounter>();
    private Text promptText;
    private Encounter nearEncounter;
    private int nextId = 1;
    
    void Awake()
    {
        BuildPrompt();
        SpawnInitial();
    }
    
    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("EncounterPromptCanvas", 45);
        promptText = UIFactory.CreateText(canvas.transform, "EncounterPrompt", "", 18, TextAnchor.MiddleCenter,
            new Color(0.9f, 0.8f, 0.3f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 210), new Vector2(800, 40));
    }
    
    void SpawnInitial()
    {
        float now = Time.realtimeSinceStartup;
        
        // 5 emboscadas (más presentes, pero invisibles y no invasivas)
        for (int i = 0; i < 5; i++) TrySpawn(EncounterType.Emboscada, now);
        
        // 4 tesoros
        for (int i = 0; i < 4; i++) TrySpawn(EncounterType.Tesoro, now);
        
        // 1 santuario
        TrySpawn(EncounterType.Santuario, now);
        
        // 1 mercader errante
        TrySpawn(EncounterType.MercaderErrante, now);
        
        // 1 cazador
        TrySpawn(EncounterType.Cazador, now);
        
        Debug.Log("[Encounters] " + encounters.Count + " encuentros iniciales spawnados.");
    }
    
    void TrySpawn(EncounterType type, float now)
    {
        // Check cooldown
        int typeKey = (int)type + 100;
        if (Cooldowns.TryGetValue(typeKey, out float cd) && now < cd) return;
        
        // Find free cell
        Vector2Int cell = FindFreeCell();
        if (cell.x < 0) return;
        
        Encounter e = new Encounter
        {
            id = nextId++,
            type = type,
            cell = cell,
            spawnedAt = now,
            consumed = false
        };
        
        // Config específica por tipo
        switch (type)
        {
            case EncounterType.Emboscada:
                e.tier = Random.Range(0, 10) < 7 ? EnemyTier.Basico : EnemyTier.Medio;
                e.enemyArchetypes = new[] { "penitent", "cherub" };
                e.cooldownUntil = now + EmboscadaCooldown;
                break;
            case EncounterType.Tesoro:
                e.tier = Random.Range(0, 10) < 5 ? EnemyTier.Basico : EnemyTier.Medio;
                e.goldReward = Random.Range(50, 300);
                e.cooldownUntil = now + TesoroCooldown;
                break;
            case EncounterType.Santuario:
                e.cooldownUntil = now + SantuarioCooldown;
                break;
            case EncounterType.MercaderErrante:
                e.cooldownUntil = now + MercaderCooldown;
                break;
            case EncounterType.Cazador:
                e.hunterContract = "hunter_" + nextId;
                e.hunterExpiry = System.DateTime.UtcNow.Ticks + 1800L * 10000000L; // 30 min
                e.cooldownUntil = now + CazadorCooldown;
                break;
        }
        
        // Visual (las emboscadas son INVISIBLES: elemento sorpresa)
        if (type != EncounterType.Emboscada)
        {
            GameObject go = new GameObject("Encounter_" + type + "_" + e.id);
            go.transform.position = new Vector3(cell.x, cell.y, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(type);
            sr.color = GetColor(type);
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 0.7f;
            e.go = go;
        }
        
        encounters.Add(e);
    }
    
    Sprite GetSprite(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Emboscada: return SpriteFactory.Circle();
            case EncounterType.Tesoro: return SpriteFactory.Square();
            case EncounterType.Santuario: return SpriteFactory.Circle();
            case EncounterType.MercaderErrante: return ArtProvider.Get("healer");
            case EncounterType.Cazador: return ArtProvider.Get("capitan");
        }
        return SpriteFactory.Square();
    }
    
    Color GetColor(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Emboscada: return new Color(0.8f, 0.2f, 0.2f, 0.7f); // rojo pulsante
            case EncounterType.Tesoro: return new Color(1f, 0.85f, 0.2f, 0.9f); // dorado
            case EncounterType.Santuario: return new Color(0.3f, 0.7f, 1f, 0.8f); // azul aura
            case EncounterType.MercaderErrante: return Color.white;
            case EncounterType.Cazador: return new Color(0.9f, 0.6f, 0.2f); // naranja
        }
        return Color.white;
    }
    
    Vector2Int FindFreeCell()
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            int x = Random.Range(3, WorldBootstrap.WorldWidth - 3);
            int y = Random.Range(3, WorldBootstrap.WorldHeight - 3);
            Vector2Int c = new Vector2Int(x, y);
            if (IsFreeForEncounter(c)) return c;
        }
        return new Vector2Int(-1, -1);
    }
    
    bool IsFreeForEncounter(Vector2Int c)
    {
        // 2.2: no invasivo — nunca cerca del spawn del jugador
        if (Vector2Int.Distance(c, WorldBootstrap.PlayerSpawn) < 6) return false;
        
        // No sobre zonas
        foreach (WorldBootstrap.ZoneDef z in WorldBootstrap.Zones)
        {
            if (Mathf.Abs(z.center.x - c.x) <= 3 && Mathf.Abs(z.center.y - c.y) <= 3) return false;
        }
        
        // No sobre otros encuentros
        foreach (Encounter e in encounters)
        {
            if (Mathf.Abs(e.cell.x - c.x) <= 2 && Mathf.Abs(e.cell.y - c.y) <= 2) return false;
        }
        
        // No sobre spawns de WorldSpawnManager
        WorldSpawnManager ws = Object.FindAnyObjectByType<WorldSpawnManager>();
        if (ws != null)
        {
            // Distancia mínima de enemigos del mundo
            if (Vector2Int.Distance(c, new Vector2Int(6, 12)) < 3) return false;
            if (Vector2Int.Distance(c, new Vector2Int(14, 6)) < 3) return false;
            if (Vector2Int.Distance(c, new Vector2Int(24, 14)) < 3) return false;
        }
        
        return true;
    }
    
    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { if (promptText != null) promptText.text = ""; return; }
        
        float now = Time.realtimeSinceStartup;
        
        // Despawn encounters viejos y spawn nuevos
        for (int i = encounters.Count - 1; i >= 0; i--)
        {
            Encounter e = encounters[i];
            if (!e.consumed && (now - e.spawnedAt) > DespawnSeconds)
            {
                if (e.go != null) Destroy(e.go);
                encounters.RemoveAt(i);
                TrySpawn(e.type, now);
            }
        }
        
        // 2.2: emboscadas invisibles → combate automático al pisar la celda (sin E)
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));
        for (int i = encounters.Count - 1; i >= 0; i--)
        {
            Encounter amb = encounters[i];
            if (amb.type == EncounterType.Emboscada && !amb.consumed && amb.cell == myCell)
            {
                amb.consumed = true;
                encounters.RemoveAt(i);
                Debug.Log("[Encounters] ¡EMBOSCADA! Combate sorpresa.");
                EnterAmbush(amb);
                return;
            }
        }

        // Detectar proximidad (solo encuentros visibles)
        nearEncounter = null;
        foreach (Encounter e in encounters)
        {
            if (e.consumed || e.type == EncounterType.Emboscada) continue;
            if (Mathf.Abs(e.cell.x - myCell.x) <= 1 && Mathf.Abs(e.cell.y - myCell.y) <= 1)
            {
                nearEncounter = e;
                break;
            }
        }
        
        if (nearEncounter != null)
        {
            string label = GetLabel(nearEncounter);
            if (promptText != null) promptText.text = "Pulsa E: " + label;
            
            if (Input.GetKeyDown(KeyCode.E)) Interact(nearEncounter);
        }
        else
        {
            if (promptText != null) promptText.text = "";
        }
    }
    
    string GetLabel(Encounter e)
    {
        switch (e.type)
        {
            case EncounterType.Emboscada: return "Emboscada (" + e.tier + ")";
            case EncounterType.Tesoro: return "Tesoro oculto";
            case EncounterType.Santuario: return "Santuario (buff temporal)";
            case EncounterType.MercaderErrante: return "Mercader errante";
            case EncounterType.Cazador: return "Cazador de recompensas";
        }
        return "Encuentro";
    }
    
    void Interact(Encounter e)
    {
        switch (e.type)
        {
            case EncounterType.Emboscada:
                EnterAmbush(e);
                break;
            case EncounterType.Tesoro:
                OpenTreasure(e);
                break;
            case EncounterType.Santuario:
                EncounterUI.ShowShrine(e);
                break;
            case EncounterType.MercaderErrante:
                EncounterUI.ShowWanderingMerchant(e);
                break;
            case EncounterType.Cazador:
                EncounterUI.ShowHunter(e);
                break;
        }
        
        e.consumed = true;
        if (e.go != null) Destroy(e.go);
        Cooldowns[(int)e.type + 100] = e.cooldownUntil;
        encounters.Remove(e);
    }
    
    void EnterAmbush(Encounter e)
    {
        // 0.7-D.1: Combate dinámico 2-4 enemigos MIXTOS según tier
        int enemyCount = Random.Range(2, 5); // 2 a 4
        string[] pool = GetArchetypePool(e.tier);
        Vector2Int[] positions = GetSpawnPositions(enemyCount);

        List<SpawnDef> spawns = new List<SpawnDef>();
        for (int i = 0; i < enemyCount; i++)
        {
            string archetype = pool[Random.Range(0, pool.Length)];
            spawns.Add(new SpawnDef
            {
                archetype = archetype,
                tier = e.tier,
                cell = positions[i]
            });
        }

        List<WaveDef> dungeon = new List<WaveDef>
        {
            new WaveDef { spawns = spawns }
        };

        Debug.Log("[Emboscada] Combate: " + enemyCount + " enemigos mixtos (tier " + e.tier + ")");

        GameFlow.pendingIsWorld = true;
        GameFlow.EnterCombat(e.tier, dungeon);
    }

    // 0.7-D.1: Pools de arquetipos por tier (mixto, no predecible)
    string[] GetArchetypePool(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico:
                return new[] { "penitent", "flagelante", "cherub", "ceniza" };
            case EnemyTier.Medio:
                return new[] { "inquisitor", "censor", "heraldo", "incensario", "cherub" };
            case EnemyTier.Elite:
                return new[] { "heraldo", "automata", "inquisitor", "capitan" };
            case EnemyTier.EliteFuerte:
                return new[] { "capitan", "automata", "heraldo" };
            case EnemyTier.Jefe:
                return new[] { "angel" };
            default:
                return new[] { "penitent" };
        }
    }

    // 0.7-D.1: Posiciones distribuidas según cantidad de enemigos
    Vector2Int[] GetSpawnPositions(int count)
    {
        switch (count)
        {
            case 1: return new[] { new Vector2Int(7, 4) };
            case 2: return new[] { new Vector2Int(6, 4), new Vector2Int(8, 4) };
            case 3: return new[] { new Vector2Int(6, 4), new Vector2Int(7, 4), new Vector2Int(8, 4) };
            case 4: return new[] { new Vector2Int(5, 4), new Vector2Int(6, 4), new Vector2Int(7, 4), new Vector2Int(8, 4) };
            default: return new[] { new Vector2Int(7, 4) };
        }
    }
    
    void OpenTreasure(Encounter e)
    {
        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.gold += e.goldReward;
            Debug.Log("[Encounters] Tesoro: +" + e.goldReward + " oro");
            CombatFeedback.SpawnText(new Vector3(e.cell.x, e.cell.y, 0), "+" + e.goldReward + " oro", Color.yellow);
        }
        
        // 25% prob de item (rarity según tier, generador real del proyecto)
        if (Random.Range(0, 100) < 25)
        {
            ClassData cd = CharacterData.Instance != null ? CharacterData.Instance.classData : null;
            Rarity rar = e.tier == EnemyTier.EliteFuerte ? Rarity.Legendary
                       : e.tier == EnemyTier.Elite ? Rarity.Epic
                       : e.tier == EnemyTier.Medio ? Rarity.Rare : Rarity.Common;
            ItemData item = ItemGenerator.GenerateWithRarity(cd, rar);
            if (InventorySystem.Instance != null && item != null)
            {
                InventorySystem.Instance.items.Add(item);
                Debug.Log("[Encounters] Tesoro: item raro " + item.itemName);
            }
        }
    }
}