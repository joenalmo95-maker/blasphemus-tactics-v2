using UnityEngine;

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
    }

    public void SetClass(ClassData data)
    {
        classData = data;
        level = 0;
        xp = 0;
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
        while (level < 20 && xp >= XpToNextLevel())
        {
            xp -= XpToNextLevel();
            level++;
            Debug.Log("¡Nivel " + level + " alcanzado!");
        }
    }
}