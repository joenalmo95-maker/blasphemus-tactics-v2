using UnityEngine;
using System.IO;

public static class SkillTrainer
{
    public const int MaxTrain = 3;

    [System.Serializable]
    class Data
    {
        public bool learned2;
        public bool learned3;
        public bool learned4;
        public int train1;
        public int train2;
        public int train3;
        public int train4;
    }

    private static Data _data;
    static string SavePath { get { return Path.Combine(Application.persistentDataPath, "skilltraining.json"); } }

    static Data D
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    static void Load()
    {
        if (File.Exists(SavePath))
        {
            _data = JsonUtility.FromJson<Data>(File.ReadAllText(SavePath));
            if (_data != null) return;
        }
        // Primera ejecución: conserva aprendidas las skills que el nivel ya desbloqueó
        _data = new Data();
        int lvl = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        _data.learned2 = lvl >= 2;
        _data.learned3 = lvl >= 5;
        _data.learned4 = lvl >= 10;
        Save();
    }

    public static void Save()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(D, true));
    }

    public static bool IsLearned(int slot)
    {
        switch (slot)
        {
            case 2: return D.learned2;
            case 3: return D.learned3;
            case 4: return D.learned4;
            default: return true;
        }
    }

    public static int TrainLevel(int slot)
    {
        switch (slot)
        {
            case 1: return D.train1;
            case 2: return D.train2;
            case 3: return D.train3;
            case 4: return D.train4;
            default: return 0;
        }
    }

    public static int LearnCost(int slot)
    {
        switch (slot)
        {
            case 2: return 100;
            case 3: return 250;
            case 4: return 500;
            default: return 0;
        }
    }

    public static int TrainCost(int slot)
    {
        return 50 * (TrainLevel(slot) + 1) * slot;
    }

    // +1 daño por nivel de entreno; identifica el slot por identidad del SkillData
    public static int BonusDamageFor(ClassRole role, SkillData skill)
    {
        if (skill == null) return 0;
        for (int i = 1; i <= 4; i++)
        {
            if (SkillCatalog.Get(role, i) == skill) return TrainLevel(i);
        }
        return 0;
    }

    public static bool TryLearn(int slot)
    {
        if (slot < 2 || slot > 4 || IsLearned(slot)) return false;
        int lvl = CharacterData.Instance != null ? CharacterData.Instance.level : 0;
        if (lvl < SkillCatalog.Get(Role(), slot).unlockLevel) return false;
        if (CharacterData.Instance == null || CharacterData.Instance.gold < LearnCost(slot)) return false;
        CharacterData.Instance.gold -= LearnCost(slot);
        switch (slot)
        {
            case 2: D.learned2 = true; break;
            case 3: D.learned3 = true; break;
            case 4: D.learned4 = true; break;
        }
        Save();
        return true;
    }

    public static bool TryTrain(int slot)
    {
        if (slot < 1 || slot > 4 || !IsLearned(slot) || TrainLevel(slot) >= MaxTrain) return false;
        if (CharacterData.Instance == null || CharacterData.Instance.gold < TrainCost(slot)) return false;
        CharacterData.Instance.gold -= TrainCost(slot);
        switch (slot)
        {
            case 1: D.train1++; break;
            case 2: D.train2++; break;
            case 3: D.train3++; break;
            case 4: D.train4++; break;
        }
        Save();
        return true;
    }

    static ClassRole Role()
    {
        if (CharacterData.Instance != null && CharacterData.Instance.classData != null)
            return CharacterData.Instance.classData.role;
        return ClassRole.DPS;
    }

    // 5.1: snapshot para el guardado unificado
    public static TrainingSnapshot GetSnapshot()
    {
        return new TrainingSnapshot
        {
            learned2 = D.learned2,
            learned3 = D.learned3,
            learned4 = D.learned4,
            train1 = D.train1,
            train2 = D.train2,
            train3 = D.train3,
            train4 = D.train4
        };
    }

    public static void ApplySnapshot(TrainingSnapshot s)
    {
        if (s == null) return;
        D.learned2 = s.learned2;
        D.learned3 = s.learned3;
        D.learned4 = s.learned4;
        D.train1 = s.train1;
        D.train2 = s.train2;
        D.train3 = s.train3;
        D.train4 = s.train4;
        Save();
    }
}

// 0.3: Definición de TrainingSnapshot (faltaba en el archivo original)
[System.Serializable]
public class TrainingSnapshot
{
    public bool learned2;
    public bool learned3;
    public bool learned4;
    public int train1;
    public int train2;
    public int train3;
    public int train4;
}