using UnityEngine;
using System.Collections.Generic;

public class CharacterData : MonoBehaviour
{
    public static CharacterData Instance { get; private set; }

    public ClassData classData;

    public int level = 0;
    public int xp = 0;
    public int gold = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // FIX: autopersistencia garantizada (la clase y el oro sobreviven a cambios de escena)
        DontDestroyOnLoad(gameObject);
    }

    public void SetClass(ClassData data)
    {
        classData = data;
        level = 0;
        xp = 0;
        gold = 20;
    }

    public void LoadFrom(SaveData data, List<ClassData> classes)
    {
        if (classes != null)
        {
            foreach (ClassData cd in classes)
            {
                if (cd.className == data.className)
                {
                    classData = cd;
                    break;
                }
            }
        }
        level = data.level;
        xp = data.xp;
        gold = data.gold;
    }

    public StatBlock GetDerivedStats()
    {
        if (classData == null) return new StatBlock();
        StatBlock s = classData.baseStats.Clone();
        for (int i = 0; i < level; i++)
        {
            s.Add(classData.growthPerLevel);
        }
        return s;
    }

    public StatBlock GetTotalStats()
    {
        StatBlock s = GetDerivedStats();
        if (InventorySystem.Instance != null)
        {
            s.Add(InventorySystem.Instance.GetEquippedStats());
        }
        return s;
    }

    public int XpToNextLevel()
    {
        return 10 + level * 5;
    }

    public void GainXP(int amount)
    {
        xp += amount;
        bool leveled = false;
        while (level < 20 && xp >= XpToNextLevel())
        {
            xp -= XpToNextLevel();
            level++;
            leveled = true;
            Debug.Log("¡Nivel " + level + " alcanzado!");
        }
        if (leveled)
        {
            if (InventorySystem.Instance != null) InventorySystem.Instance.ApplyToUnit();

            Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
            foreach (Unit u in units)
            {
                if (!u.isEnemy)
                {
                    u.currentHealth = u.maxHealth;
                    Debug.Log("Curación completa por subida de nivel.");
                    break;
                }
            }
        }
    }
}