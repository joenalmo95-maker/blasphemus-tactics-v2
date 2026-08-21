using UnityEngine;
using System.Collections.Generic;

public static class LoadoutSystem
{
    private static List<string> learned = new List<string>();
    private static readonly string[] active = new string[4];
    private static string ultimate = "";
    private static readonly string[] passives = new string[3];
    private static bool initialized = false;

    public static bool Enabled = true;

    public static void EnsureInitialized()
    {
        if (initialized) return;

        // 0.3: Starter único de Valerius
        foreach (string id in SkillPool.StartersFor(ClassRole.DPS))
        {
            if (!learned.Contains(id)) learned.Add(id);
        }

        string ult = DefaultUltimate(ClassRole.DPS);
        if (!learned.Contains(ult)) learned.Add(ult);

        RepairLoadout();
        initialized = true;
        Debug.Log("[Loadout] listo. Aprendidas: " + string.Join(", ", learned));
    }

    public static void RepairLoadout()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!string.IsNullOrEmpty(active[i]) && SkillPool.Get(active[i]) != null) continue;
            active[i] = "";
            foreach (string id in learned)
            {
                SkillMeta m = SkillPool.Meta(id);
                if (m == null || m.type != SkillType.Activa) continue;
                if (System.Array.IndexOf(active, id) >= 0) continue;
                active[i] = id;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(passives[i]) && SkillPool.Get(passives[i]) != null) continue;
            passives[i] = "";
            foreach (string id in learned)
            {
                SkillMeta m = SkillPool.Meta(id);
                if (m == null || m.type != SkillType.Pasiva) continue;
                if (System.Array.IndexOf(passives, id) >= 0) continue;
                passives[i] = id;
                break;
            }
        }
    }

    static string DefaultUltimate(ClassRole role)
    {
        return "ira_halo";
    }

    public static bool IsLearned(string id)
    {
        EnsureInitialized();
        return learned.Contains(id);
    }

    public static void Learn(string id)
    {
        EnsureInitialized();
        if (!learned.Contains(id)) learned.Add(id);
    }

    public static bool AssignActive(int slot, string id)
    {
        EnsureInitialized();
        if (slot < 0 || slot > 3) return false;
        if (!string.IsNullOrEmpty(id) && (!IsLearned(id) || SkillPool.Meta(id).type != SkillType.Activa)) return false;
        active[slot] = id;
        return true;
    }

    public static bool AssignUltimate(string id)
    {
        EnsureInitialized();
        if (!string.IsNullOrEmpty(id) && (!IsLearned(id) || SkillPool.Meta(id).type != SkillType.Ultimate)) return false;
        ultimate = id;
        return true;
    }

    public static bool AssignPassive(int slot, string id)
    {
        EnsureInitialized();
        if (slot < 0 || slot > 2) return false;
        if (!string.IsNullOrEmpty(id))
        {
            if (!IsLearned(id) || SkillPool.Meta(id).type != SkillType.Pasiva) return false;
            for (int i = 0; i < 3; i++)
            {
                if (i != slot && passives[i] == id) return false;
            }
        }
        passives[slot] = id;
        return true;
    }

    public static string ActiveId(int slot)
    {
        EnsureInitialized();
        if (slot < 0 || slot > 3) return "";
        return active[slot];
    }

    public static string UltimateId()
    {
        EnsureInitialized();
        return ultimate;
    }

    public static string PassiveId(int slot)
    {
        EnsureInitialized();
        if (slot < 0 || slot > 2) return "";
        return passives[slot];
    }

    public static SkillData GetActive(int slot)
    {
        string id = ActiveId(slot);
        return id == "" ? null : SkillPool.Get(id);
    }

    public static SkillData GetUltimate()
    {
        string id = UltimateId();
        return id == "" ? null : SkillPool.Get(id);
    }

    public static List<SkillData> GetPassives()
    {
        List<SkillData> list = new List<SkillData>();
        for (int i = 0; i < 3; i++)
        {
            string id = PassiveId(i);
            if (id != "")
            {
                SkillData s = SkillPool.Get(id);
                if (s != null) list.Add(s);
            }
        }
        return list;
    }

    public static void SnapshotToSave(SaveData data)
    {
        EnsureInitialized();
        data.learnedSkills = new List<string>(learned);
        data.activeSkills = new List<string>(active);
        data.ultimateSkill = ultimate;
        data.passiveSkills = new List<string>(passives);
    }

    public static void ApplyFromSave(SaveData data)
    {
        if (data == null) return;
        if (data.learnedSkills != null) learned = new List<string>(data.learnedSkills);
        if (data.activeSkills != null)
        {
            for (int i = 0; i < 4 && i < data.activeSkills.Count; i++)
                active[i] = data.activeSkills[i];
        }
        ultimate = data.ultimateSkill != null ? data.ultimateSkill : "";
        if (data.passiveSkills != null)
        {
            for (int i = 0; i < 3 && i < data.passiveSkills.Count; i++)
                passives[i] = data.passiveSkills[i];
        }
        initialized = true;
        RepairLoadout();
    }
    // 0.6: Desbloqueo automático de skills al subir de nivel
    public static List<string> AutoUnlockForLevel(int level)
    {
        List<string> unlocked = new List<string>();
        foreach (string id in SkillPool.AllIds())
        {
            SkillMeta meta = SkillPool.Meta(id);
            if (meta == null) continue;
            
            // Solo auto-desbloquear skills de progresión por nivel (no las del Entrenador)
            if (meta.origin == "Entrenador") continue;
            if (meta.cost > 0) continue;
            
            if (meta.unlockLevel <= level && !learned.Contains(id))
            {
                learned.Add(id);
                unlocked.Add(id);
                Debug.Log("[Loadout] Auto-desbloqueada por nivel " + level + ": " + meta.name);
            }
        }
        return unlocked;
    }

    // Reset para NUEVA PARTIDA - solo mantiene golpe_basico
    public static void ResetToStarter()
    {
        learned.Clear();
        
        // active y passives son readonly - asignar elemento por elemento
        for (int i = 0; i < 4; i++) active[i] = "";
        active[0] = "golpe_basico";
        
        ultimate = "";
        
        for (int i = 0; i < 3; i++) passives[i] = "";

        // Aprende el starter
        if (!learned.Contains("golpe_basico"))
            learned.Add("golpe_basico");

        initialized = true;
        Debug.Log("[Loadout] Reset completo. Solo golpe_basico disponible.");
    }
}