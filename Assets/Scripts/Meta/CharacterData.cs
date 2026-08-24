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

    // Stats base de Valerius (nivel 1) — 0.7: rebalance daño
    public int baseHP = 250;
    public int baseDamage = 18;
    public int baseDefense = 0;
    public int baseCritChance = 5;
    public int baseEvasion = 0;
    public int baseAP = 3;
    public int baseAccuracy = 80;
    public int baseHealingPower = 0;
    public int baseLifesteal = 0;
    public int baseThreatMult = 1;

    // Crecimiento por nivel — 0.7: crecimiento más lento
    public int hpPerLevel = 20;
    public int damagePerLevel = 2;
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
        
        // Fix: Limpieza diferida de items corruptos (se ejecuta después de que InventorySystem cargue)
        StartCoroutine(DelayedClean());
    }
    
    System.Collections.IEnumerator DelayedClean()
    {
        yield return new WaitForSeconds(0.5f);
        CleanCorruptedSave();
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
        s.accuracy = baseAccuracy;
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

        // 1.5: Bonos de pasivas de endgame (Nivel 20+)
        foreach (SkillData passive in LoadoutSystem.GetPassives())
        {
            if (passive != null && passive.skillName == "Voluntad de Hierro")
            {
                s.damage += 10;
                s.critChance += 10;
            }
        }

        // 0.7-E: bonuses de set completo (4/4)
        if (SetBonusSystem.HasFullSet(SetType.Rojo))
            s.damage = Mathf.RoundToInt(s.damage * 1.15f);
        if (SetBonusSystem.HasFullSet(SetType.Amarillo))
            s.critChance += 25;
        if (SetBonusSystem.HasFullSet(SetType.Verde))
        {
            s.maxHP += 50;
            s.healOnHit += 2;
        }
        return s;
    }

    public int XpToNextLevel()
    {
        // 0.7: XP requerida multiplicada x2.5 para que nivel 20 tome 20-30h de juego
        return Mathf.RoundToInt(30 * Mathf.Pow(1.22f, level));
    }

    // 0.7-fix: Buffs del mundo (Santuarios) persistentes entre escenas
    public int worldBuffDamage = 0;
    public int worldBuffDefense = 0;
    public int worldBuffCrit = 0;
    public bool hasWorldBuff = false;

    // 0.8-fix: habilidad bloqueada por el Flagelante
    public int blockedSkillSlot = -1;
    public int blockedSkillTurns = 0;

    public void BlockSkillSlot(int slot, int turns)
    {
        blockedSkillSlot = slot;
        blockedSkillTurns = turns;
    }

    public void ApplyWorldBuff(string type)
    {
        // Los buffs del mundo son acumulativos y permanentes hasta morir
        switch (type)
        {
            case "dmg": worldBuffDamage += 5; break;
            case "def": worldBuffDefense += 5; break;
            case "crit": worldBuffCrit += 10; break;
            case "ap": /* AP se maneja aparte */ break;
        }
        hasWorldBuff = true;
        Debug.Log("[WorldBuff] +" + type + " aplicado. Totales: DMG+" + worldBuffDamage + " DEF+" + worldBuffDefense + " CRIT+" + worldBuffCrit);
    }

    public void ClearWorldBuffs()
    {
        worldBuffDamage = 0;
        worldBuffDefense = 0;
        worldBuffCrit = 0;
        hasWorldBuff = false;
        Debug.Log("[WorldBuff] Buffs del mundo limpiados (muerte/reset).");
    }

    public void GainXP(int amount)
    {
        if (level >= 30) { xp = 0; return; }

        xp += amount;
        int needed = XpToNextLevel();
        while (xp >= needed && level < 30)
        {
            xp -= needed;
            level++;
            OnLevelUp();
            needed = XpToNextLevel();
        }
        if (level >= 30) xp = 0;
    }

    void OnLevelUp()
    {
        Debug.Log("[CharacterData] ┬íSubiste al nivel " + level + "!");
        
        // 0.6: Desbloqueo automático de skills por nivel
        List<string> unlocked = LoadoutSystem.AutoUnlockForLevel(level);
        if (unlocked.Count > 0)
        {
            Debug.Log("[CharacterData] Skills desbloqueadas en nivel " + level + ": " + string.Join(", ", unlocked));
            CombatFeedback.SpawnText(transform.position, "+" + unlocked.Count + " SKILLS!", new Color(0.3f, 0.9f, 1f));
        }
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
    // Fix: Limpieza de items dummy corruptos del save antiguo
    public static void CleanCorruptedSave()
    {
        if (InventorySystem.Instance == null) return;
        
        bool cleaned = false;
        
        // Revisar items equipados
        foreach (ItemSlot slot in System.Enum.GetValues(typeof(ItemSlot)))
        {
            ItemData item = InventorySystem.Instance.GetEquipped(slot);
            if (item != null && (item.itemName == "Objeto" || string.IsNullOrEmpty(item.itemName)))
            {
                InventorySystem.Instance.Unequip(slot);
                cleaned = true;
                Debug.Log("[CharacterData] Item dummy removido del slot: " + slot);
            }
        }
        
        if (cleaned)
        {
            InventorySystem.Instance.ApplyToUnit();
            Debug.Log("[CharacterData] Save limpiado de items corruptos.");
        }
    }
}