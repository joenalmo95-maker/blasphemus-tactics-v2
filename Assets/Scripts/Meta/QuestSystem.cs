using UnityEngine;
using System.Collections.Generic;

public enum QuestType { Diaria, Semanal, Temporada, Evento }

[System.Serializable]
public class QuestSaveEntry
{
    public string id;
    public int progress;
    public bool claimed;
    public bool accepted;
    public long expiry;
}

public class QuestDef
{
    public string id;
    public QuestType type;
    public string description;
    public string evt;
    public int target;
    public int gold;
    public int xp;
}

public class QuestState
{
    public string id;
    public int progress;
    public bool claimed;
    public bool accepted;
    public long expiry;
}

// 2.1/2.2: misiones temporizadas con aceptación obligatoria y contratos con timer
public static class QuestSystem
{
    private static readonly List<QuestDef> catalog = new List<QuestDef>();
    private static readonly List<QuestState> actives = new List<QuestState>();
    private static long lastDaily;
    private static long lastWeekly;
    private static long eventCooldownUntil;
    private static int seasonPhase;
    private static bool initialized;

    private const long HOUR_TICKS = 3600L * 10000000L;
    private const long MIN_TICKS = 60L * 10000000L;

    static QuestSystem()
    {
        BuildCatalog();
    }

    static void Add(string id, QuestType t, string desc, string evt, int target, int gold, int xp)
    {
        catalog.Add(new QuestDef { id = id, type = t, description = desc, evt = evt, target = target, gold = gold, xp = xp });
    }

    static void BuildCatalog()
    {
        // Diarias
        Add("d_kill_5", QuestType.Diaria, "Mata 5 enemigos", "enemy_killed", 5, 150, 50);
        Add("d_kill_10", QuestType.Diaria, "Mata 10 enemigos", "enemy_killed", 10, 250, 80);
        Add("d_kill_15", QuestType.Diaria, "Mata 15 enemigos", "enemy_killed", 15, 350, 110);
        Add("d_dun_1", QuestType.Diaria, "Completa 1 mazmorra", "dungeon_completed", 1, 200, 60);
        Add("d_dun_2", QuestType.Diaria, "Completa 2 mazmorras", "dungeon_completed", 2, 350, 100);
        Add("d_skill_8", QuestType.Diaria, "Ejecuta 8 skills", "skill_used", 8, 150, 50);
        Add("d_skill_15", QuestType.Diaria, "Ejecuta 15 skills", "skill_used", 15, 250, 80);
        Add("d_boss_1", QuestType.Diaria, "Derrota 1 jefe o élite", "boss_killed", 1, 300, 100);

        // Semanales
        Add("w_kill_40", QuestType.Semanal, "Mata 40 enemigos", "enemy_killed", 40, 1200, 400);
        Add("w_kill_60", QuestType.Semanal, "Mata 60 enemigos", "enemy_killed", 60, 1600, 500);
        Add("w_dun_6", QuestType.Semanal, "Completa 6 mazmorras", "dungeon_completed", 6, 1500, 500);
        Add("w_dun_10", QuestType.Semanal, "Completa 10 mazmorras", "dungeon_completed", 10, 2000, 650);
        Add("w_boss_3", QuestType.Semanal, "Derrota 3 jefes/élites", "boss_killed", 3, 1800, 600);
        Add("w_skill_60", QuestType.Semanal, "Ejecuta 60 skills", "skill_used", 60, 1200, 400);

        // Temporada (línea encadenada "El Halo Roto")
        Add("s_halo_1", QuestType.Temporada, "Temporada: mata 25 enemigos", "enemy_killed", 25, 500, 200);
        Add("s_halo_2", QuestType.Temporada, "Temporada: completa 5 mazmorras", "dungeon_completed", 5, 800, 300);
        Add("s_halo_3", QuestType.Temporada, "Temporada: ejecuta 50 skills", "skill_used", 50, 1000, 400);
        Add("s_halo_4", QuestType.Temporada, "Temporada: derrota 2 jefes/élites", "boss_killed", 2, 1500, 500);
        Add("s_halo_5", QuestType.Temporada, "Temporada: mata 75 enemigos", "enemy_killed", 75, 2000, 700);
        Add("s_halo_6", QuestType.Temporada, "Temporada: completa 12 mazmorras", "dungeon_completed", 12, 2500, 900);
        Add("s_halo_7", QuestType.Temporada, "Temporada: derrota 4 jefes/élites", "boss_killed", 4, 3000, 1200);
        Add("s_halo_8", QuestType.Temporada, "Temporada: mata 150 enemigos", "enemy_killed", 150, 5000, 2000);

        // Eventos (rotación, 48 h)
        Add("e_luna", QuestType.Evento, "EVENTO Luna Roja: mata 30 enemigos", "enemy_killed", 30, 1000, 300);
        Add("e_festival", QuestType.Evento, "EVENTO Festival de Skills: ejecuta 40 skills", "skill_used", 40, 1000, 300);
        Add("e_caceria", QuestType.Evento, "EVENTO Cacería: derrota 2 jefes/élites", "boss_killed", 2, 1200, 350);
        Add("e_worldboss", QuestType.Evento, "EVENTO: Derrota al Capitán antes de que desaparezca", "world_boss_defeated", 1, 2500, 1000);
    }

    public static QuestDef GetDef(string id)
    {
        foreach (QuestDef d in catalog) if (d.id == id) return d;
        return null;
    }

    public static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        long now = System.DateTime.UtcNow.Ticks;
        if (lastDaily == 0) lastDaily = now;
        if (lastWeekly == 0) lastWeekly = now;
        if (actives.Count == 0) GenerateAll();
        ResetChecks();
        EnsureEvent();
        Debug.Log("[Quests] sistema listo: " + actives.Count + " misiones activas.");
    }

    static void GenerateAll()
    {
        GenerateDailies();
        GenerateWeeklies();
        GenerateSeason();
    }

    static List<QuestDef> Pool(QuestType t)
    {
        List<QuestDef> list = new List<QuestDef>();
        foreach (QuestDef d in catalog) if (d.type == t) list.Add(d);
        return list;
    }

    static void GenerateDailies()
    {
        List<QuestDef> pool = Pool(QuestType.Diaria);
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            actives.Add(new QuestState { id = pool[idx].id });
            pool.RemoveAt(idx);
        }
    }

    static void GenerateWeeklies()
    {
        List<QuestDef> pool = Pool(QuestType.Semanal);
        for (int i = 0; i < 2 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            actives.Add(new QuestState { id = pool[idx].id });
            pool.RemoveAt(idx);
        }
    }

    static void GenerateSeason()
    {
        List<QuestDef> line = Pool(QuestType.Temporada);
        if (seasonPhase >= 0 && seasonPhase < line.Count)
        {
            actives.Add(new QuestState { id = line[seasonPhase].id });
        }
    }

    static void EnsureEvent()
    {
        long now = System.DateTime.UtcNow.Ticks;
        foreach (QuestState q in actives) if (GetDef(q.id) != null && GetDef(q.id).type == QuestType.Evento) return;
        if (now < eventCooldownUntil) return;
        List<QuestDef> pool = Pool(QuestType.Evento);
        if (pool.Count == 0) return;
        QuestDef pick = pool[Random.Range(0, pool.Count)];
        actives.Add(new QuestState { id = pick.id, expiry = now + 48 * HOUR_TICKS });
        Debug.Log("[Quests] Evento activo: " + pick.description);
    }

    static void ClearType(QuestType t)
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            QuestDef d = GetDef(actives[i].id);
            if (d != null && d.type == t) actives.RemoveAt(i);
        }
    }

    static void ResetChecks()
    {
        long now = System.DateTime.UtcNow.Ticks;
        if (now - lastDaily > 24 * HOUR_TICKS)
        {
            ClearType(QuestType.Diaria);
            GenerateDailies();
            lastDaily = now;
            Debug.Log("[Quests] Nuevas misiones diarias generadas.");
        }
        if (now - lastWeekly > 7 * 24 * HOUR_TICKS)
        {
            ClearType(QuestType.Semanal);
            GenerateWeeklies();
            lastWeekly = now;
            Debug.Log("[Quests] Nuevas misiones semanales generadas.");
        }
        TickExpiry();
    }

    // 2.2: elimina contratos/eventos expirados (llamado también por el tracker J)
    static void TickExpiry()
    {
        long now = System.DateTime.UtcNow.Ticks;
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            if (actives[i].expiry > 0 && now > actives[i].expiry)
            {
                Debug.Log("[Quests] Expirada: " + actives[i].id);
                actives.RemoveAt(i);
                eventCooldownUntil = now + 24 * HOUR_TICKS;
            }
        }
    }

    public static void Tick()
    {
        if (!initialized) return;
        TickExpiry();
    }

    // --- Hooks de progreso (solo misiones ACEPTADAS) ---
    public static void Notify(string evt, int amount = 1)
    {
        EnsureInitialized();
        List<QuestState> toRemove = new List<QuestState>();
        foreach (QuestState q in actives)
        {
            if (q.claimed || !q.accepted) continue; // 2.2: aceptación obligatoria
            QuestDef d = GetDef(q.id);
            if (d == null || d.evt != evt) continue;
            if (q.progress < d.target)
            {
                q.progress = Mathf.Min(d.target, q.progress + amount);
                if (q.progress >= d.target)
                {
                    Debug.Log("[Quests] COMPLETADA: " + d.description);
                    if (q.expiry > 0)
                    {
                        // Contrato con timer: autorrecompensa y desaparece
                        GrantReward(d);
                        toRemove.Add(q);
                        Debug.Log("[Quests] Contrato completado: desaparece del seguimiento (J).");
                    }
                }
            }
        }
        foreach (QuestState q in toRemove) actives.Remove(q);
    }

    public static void NotifyEnemyKilled(bool boss, bool elite)
    {
        Notify("enemy_killed");
        if (boss || elite) Notify("boss_killed");
    }

    public static void NotifyDungeonCompleted() { Notify("dungeon_completed"); }
    public static void NotifySkillUsed() { Notify("skill_used"); }

    // --- 2.2: aceptación ---
    public static bool Accept(string id)
    {
        EnsureInitialized();
        foreach (QuestState q in actives)
        {
            if (q.id == id && !q.accepted)
            {
                q.accepted = true;
                QuestDef d = GetDef(id);
                Debug.Log("[Quests] Misión aceptada: " + (d != null ? d.description : id) + " (J para seguimiento).");
                return true;
            }
        }
        return false;
    }

    // 2.2: contrato de cazador (aceptado al instante, timer 30 min)
    public static void AcceptHunterContract()
    {
        EnsureInitialized();
        long now = System.DateTime.UtcNow.Ticks;
        QuestDef def = new QuestDef
        {
            id = "hunter_" + now.ToString(),
            type = QuestType.Evento,
            description = "CONTRATO: mata 10 enemigos",
            evt = "enemy_killed",
            target = 10,
            gold = 1000,
            xp = 500
        };
        catalog.Add(def);
        actives.Add(new QuestState { id = def.id, accepted = true, expiry = now + 30 * MIN_TICKS });
        Debug.Log("[Quests] Contrato de cazador aceptado (J para seguimiento).");
    }

    static void GrantReward(QuestDef d)
    {
        CharacterData cd = CharacterData.Instance;
        if (cd == null) return;
        cd.gold += d.gold;
        cd.GainXP(d.xp); // respeta el tope de nivel 20
        Debug.Log("[Quests] Recompensa: +" + d.gold + " oro, +" + d.xp + " XP");
    }

    // --- Reclamar (misiones sin timer) ---
    public static bool Claim(string id)
    {
        EnsureInitialized();
        QuestState q = null;
        foreach (QuestState s in actives) if (s.id == id) { q = s; break; }
        if (q == null || q.claimed || !q.accepted) return false;
        QuestDef d = GetDef(id);
        if (d == null || q.progress < d.target) return false;

        q.claimed = true;
        GrantReward(d);

        if (d.type == QuestType.Temporada)
        {
            seasonPhase++;
            List<QuestDef> line = Pool(QuestType.Temporada);
            if (seasonPhase < line.Count)
            {
                q.id = line[seasonPhase].id;
                q.progress = 0;
                q.claimed = false;
                q.accepted = false; // la siguiente fase también debe aceptarse
            }
            else
            {
                actives.Remove(q);
                Debug.Log("[Quests] ¡TEMPORADA COMPLETADA!");
            }
        }
        return true;
    }

    // --- Utilidades para UI ---
    public static List<QuestState> Actives()
    {
        EnsureInitialized();
        return new List<QuestState>(actives);
    }

    public static int HoursLeftDaily()
    {
        long now = System.DateTime.UtcNow.Ticks;
        return Mathf.Max(0, Mathf.RoundToInt((24 * HOUR_TICKS - (now - lastDaily)) / (float)HOUR_TICKS));
    }

    public static int HoursLeftWeekly()
    {
        long now = System.DateTime.UtcNow.Ticks;
        return Mathf.Max(0, Mathf.RoundToInt((7 * 24 * HOUR_TICKS - (now - lastWeekly)) / (float)HOUR_TICKS));
    }

    public static int HoursLeftEvent(string id)
    {
        long now = System.DateTime.UtcNow.Ticks;
        foreach (QuestState q in actives)
        {
            if (q.id == id && q.expiry > 0)
                return Mathf.Max(0, Mathf.RoundToInt((q.expiry - now) / (float)HOUR_TICKS));
        }
        return 0;
    }

    public static int MinutesLeft(string id)
    {
        long now = System.DateTime.UtcNow.Ticks;
        foreach (QuestState q in actives)
        {
            if (q.id == id && q.expiry > 0)
                return Mathf.Max(0, Mathf.RoundToInt((q.expiry - now) / (float)MIN_TICKS));
        }
        return 0;
    }

    public static int SeasonPhase() { return seasonPhase; }

    public static void DebugForceDailyReset()
    {
        lastDaily = 0;
        ResetChecks();
        EnsureEvent();
    }

    // --- Persistencia v5/v6 ---
    public static void SnapshotToSave(SaveData data)
    {
        EnsureInitialized();
        data.activeQuests = new List<QuestSaveEntry>();
        foreach (QuestState q in actives)
        {
            data.activeQuests.Add(new QuestSaveEntry
            {
                id = q.id,
                progress = q.progress,
                claimed = q.claimed,
                accepted = q.accepted,
                expiry = q.expiry
            });
        }
        data.lastDailyReset = lastDaily;
        data.lastWeeklyReset = lastWeekly;
        data.seasonPhase = seasonPhase;
    }

    public static void ApplyFromSave(SaveData data)
    {
        if (data == null) return;
        initialized = true;
        lastDaily = data.lastDailyReset;
        lastWeekly = data.lastWeeklyReset;
        seasonPhase = data.seasonPhase;
        if (data.activeQuests != null && data.activeQuests.Count > 0)
        {
            actives.Clear();
            foreach (QuestSaveEntry e in data.activeQuests)
            {
                if (GetDef(e.id) == null)
                {
                    // Reconstruye contratos de cazador persistidos
                    if (e.id.StartsWith("hunter_"))
                    {
                        catalog.Add(new QuestDef
                        {
                            id = e.id,
                            type = QuestType.Evento,
                            description = "CONTRATO: mata 10 enemigos",
                            evt = "enemy_killed",
                            target = 10,
                            gold = 1000,
                            xp = 500
                        });
                    }
                    else continue;
                }
                actives.Add(new QuestState { id = e.id, progress = e.progress, claimed = e.claimed, accepted = e.accepted, expiry = e.expiry });
            }
        }
        if (actives.Count == 0) GenerateAll();
        ResetChecks();
        EnsureEvent();
    }
    // Reset para NUEVA PARTIDA
    public static void ResetForNewGame()
    {
        actives.Clear();
        lastDaily = 0;
        lastWeekly = 0;
        eventCooldownUntil = 0;
        seasonPhase = 0;
        initialized = false;
        Debug.Log("[QuestSystem] Reset completo para nueva partida.");
    }

    // 0.5-fix: Inyectar misión del Boss Mundial cuando spawnea (24h para derrotarlo)
    public static void InjectWorldBossQuest(long bossSpawnTicks)
    {
        EnsureInitialized();
        // Si ya existe la misión del Capitán, no duplicar
        foreach (QuestState q in actives) if (q.id == "e_worldboss") return;
        
        long expiry = bossSpawnTicks + 24 * HOUR_TICKS; // 24h desde el spawn
        actives.Add(new QuestState 
        { 
            id = "e_worldboss", 
            accepted = true, // aceptada automáticamente para el tracker J
            expiry = expiry 
        });
        Debug.Log("[Quests] Misión del Capitán inyectada en el Tablón (24h).");
    }
}