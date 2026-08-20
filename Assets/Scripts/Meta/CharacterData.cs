using UnityEngine;
using System.Collections.Generic;

public class CharacterData : MonoBehaviour
{
    public static CharacterData Instance { get; private set; }

    // 0.3: Stats base de Valerius (sin clases)
    public string playerName = "Valerius";
    public int level = 1;
    public int xp = 0;
    public int gold = 100;

    // 0.3: ClassData por defecto (Inquisidor = DPS)
    public ClassData classData;

    // Stats base de Valerius (nivel 1)
    public int baseHP = 250;
    public int baseDamage = 30;
    public int baseDefense = 0;
    public int baseCritChance = 5;
    public int baseEvasion = 0;
    public int baseAP = 3;
    public int baseAttack = 80;
    public int baseHealingPower = 0;
    public int baseLifesteal = 0;
    public int baseThreatMult = 1;

    // Crecimiento por nivel
    public int hpPerLevel = 20;
    public int damagePerLevel = 3;
    public int defensePerLevel = 1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 0.3: Crear ClassData por defecto si no existe
        if (classData == null)
        {
            classData = ScriptableObject.CreateInstance<ClassData>();
            classData.className = "Inquisidor";
            classData.role = ClassRole.DPS;
            classData.artKey = "inquisitor";
        }
    }

    // 0.3: Stats derivados (usa stats base de Valerius + crecimiento)
    public StatBlock GetDerivedStats()
    {
        StatBlock s = new StatBlock();
        s.maxHP = baseHP + (level - 1) * hpPerLevel;
        s.damage = baseDamage + (level - 1) * damagePerLevel;
        s.defense = baseDefense + (level - 1) * defensePerLevel;
        s.critChance = baseCritChance;
        s.evasion = baseEvasion;
        s.apMove = baseAP;
        s.attack = baseAttack;
        s.healingPower = baseHealingPower;
        s.lifesteal = baseLifesteal;
        s.threatMult = baseThreatMult;
        return s;
    }

    public StatBlock GetTotalStats()
    {
        StatBlock s = GetDerivedStats();
        if (InventorySystem.Instance != null)
        {
            s.Add(InventorySystem.Instance.GetEquippedStats());
        }
        s.damage += Progression.PlayerDamageBonus(level);
        return s;
    }

    public int XpToNextLevel()
    {
        return Mathf.RoundToInt(12 * Mathf.Pow(1.22f, level));
    }

    public void GainXP(int amount)
    {
        if (level >= 20) { xp = 0; return; }

        xp += amount;
        int needed = XpToNextLevel();
        while (xp >= needed && level < 20)
        {
            xp -= needed;
            level++;
            OnLevelUp();
            needed = XpToNextLevel();
        }
        if (level >= 20) xp = 0;
    }

    void OnLevelUp()
    {
        Debug.Log("[CharacterData] ¡Subiste al nivel " + level + "!");
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }
}