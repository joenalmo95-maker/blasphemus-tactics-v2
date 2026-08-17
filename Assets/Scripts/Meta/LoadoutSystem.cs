using UnityEngine;
using System.Collections.Generic;

// 1.1-B: loadout persistente (4 activas + 1 ultimate + 3 pasivas) y skills aprendidas
public static class LoadoutSystem
{
    private static List<string> learned = new List<string>();
    private static readonly string[] active = new string[4];
    private static string ultimate = "";
    private static readonly string[] passives = new string[3];
    private static bool initialized;

    public static void EnsureInitialized()
    {
        if (initialized) return;
        if (CharacterData.Instance == null || CharacterData.Instance.classData == null) return;
        if (learned.Count > 0) { initialized = true; return; }

        ClassRole role = CharacterData.Instance.classData.role;
        List<string> starters = SkillPool.StartersFor(role);
        foreach (string id in starters) if (!learned.Contains(id)) learned.Add(id);

        string ult = DefaultUltimate(role);
        if (!learned.Contains(ult)) learned.Add(ult);

        active[0] = starters[0];
        active[1] = starters.Count > 1 ? starters[1] : "";
        active[2] = "";
        active[3] = "";
        ultimate = ult;
        passives[0] = starters.Count > 2 ? starters[2] : "";
        passives[1] = "";
        passives[2] = "";

        initialized = true;
        Debug.Log("[Loadout] inicializado para " + role + ": " + string.Join(", ", learned));
    }

    static string DefaultUltimate(ClassRole role)
    {
        switch (role)
        {
            case ClassRole.Tank: return "bastion_inquebrantable";
            case ClassRole.Healer: return "juicio_divino";
            default: return "ira_halo";
        }
    }

    public static bool IsLearned(string id)
    {
        EnsureInitialized();
        return learned.Contains(id);
    }

    public static List<string> Learned()
    {
        EnsureInitialized();
        return new List<string>(learned);
    }

    public static void Learn(string id)
    {
        EnsureInitialized();
        if (!learned.Contains(id)) learned.Add(id);
    }

    public static string ActiveId(int i)
    {
        EnsureInitialized();
        return (i >= 0 && i <= 3) ? active[i] : "";
    }

    public static string PassiveId(int i)
    {
        EnsureInitialized();
        return (i >= 0 && i <= 2) ? passives[i] : "";
    }

    public static SkillData GetActive(int i)
    {
        EnsureInitialized();
        if (i < 0 || i > 3 || string.IsNullOrEmpty(active[i])) return null;
        return SkillPool.Get(active[i]);
    }

    public static SkillData GetUltimate()
    {
        EnsureInitialized();
        return string.IsNullOrEmpty(ultimate) ? null : SkillPool.Get(ultimate);
    }

    public static string UltimateId()
    {
        EnsureInitialized();
        return ultimate;
    }

    public static List<SkillData> GetPassives()
    {
        EnsureInitialized();
        List<SkillData> list = new List<SkillData>();
        foreach (string id in passives)
        {
            if (!string.IsNullOrEmpty(id))
            {
                SkillData d = SkillPool.Get(id);
                if (d != null) list.Add(d);
            }
        }
        return list;
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

    // --- Guardado v3 ---
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
        if (data.learnedSkills == null || data.learnedSkills.Count == 0) return;

        learned = new List<string>(data.learnedSkills);
        for (int i = 0; i < 4; i++)
            active[i] = (data.activeSkills != null && i < data.activeSkills.Count) ? data.activeSkills[i] : "";
        ultimate = data.ultimateSkill != null ? data.ultimateSkill : "";
        for (int i = 0; i < 3; i++)
            passives[i] = (data.passiveSkills != null && i < data.passiveSkills.Count) ? data.passiveSkills[i] : "";

        initialized = true;
        Debug.Log("[Loadout] restaurado desde save: " + string.Join(", ", learned));
    }
}