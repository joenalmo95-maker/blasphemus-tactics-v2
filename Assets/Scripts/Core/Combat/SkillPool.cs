using UnityEngine;
using System.Collections.Generic;

public enum SkillType { Activa, Ultimate, Pasiva }

[System.Serializable]
public class SkillMeta
{
    public string id;
    public string name;
    public SkillType type;
    public string affinity;
    public string rarity;
    public string tag;
    public string pattern;
    public string effectKey;
    public string origin;
    public string desc;
    public int ap;
    public int damage;
    public int range;
    public int cooldown;
    public int bonusCrit;
    public int cost;
    public int heal;
    public float threatMult;
    public int unlockLevel;
}

public static class SkillPool
{
    static Dictionary<string, SkillMeta> cache;
    static Dictionary<string, SkillData> dataCache;

    public static SkillData Get(string id)
    {
        if (dataCache == null) Build();
        SkillData s;
        dataCache.TryGetValue(id, out s);
        return s;
    }

    public static SkillMeta Meta(string id)
    {
        if (cache == null) Build();
        SkillMeta m;
        cache.TryGetValue(id, out m);
        return m;
    }

    public static List<string> AllIds()
    {
        if (cache == null) Build();
        return new List<string>(cache.Keys);
    }

    public static List<string> StartersFor(ClassRole role)
    {
        // 0.3: Starter único de Valerius
        return new List<string> { "golpe_basico" };
    }

    static void Build()
    {
        cache = new Dictionary<string, SkillMeta>();
        dataCache = new Dictionary<string, SkillData>();
        List<SkillMeta> l = new List<SkillMeta>();

        // 0.3: Starter único de Valerius (nivel 1)
        l.Add(C("golpe_basico", "Golpe Básico", SkillType.Activa, "Universal", "Common", "Daño", "0,0", "", "Starter", "Ataque básico con el espadón.", 1, 3, 1, 0, 0, 0, 0, 0, 1));

        // Nivel 3
        l.Add(C("empuje", "Empuje", SkillType.Activa, "Universal", "Common", "Control", "0,0", "knockback", "Nivel 3", "Ataca y empuja 1 celda.", 1, 2, 1, 0, 0, 0, 0, 0, 3));

        // Nivel 5
        l.Add(C("barrido", "Barrido", SkillType.Activa, "Universal", "Rare", "Daño en Área", "0,0", "sweep", "Nivel 5", "Daño en cruz (4 celdas adyacentes).", 2, 4, 1, 0, 0, 0, 0, 0, 5));

        // Nivel 7 (Pasiva)
        l.Add(C("piel_hierro", "Piel de Hierro", SkillType.Pasiva, "Universal", "Rare", "", "", "piel", "Nivel 7", "+2 DEF.", 0, 0, 0, 0, 0, 400, 0, 0, 7));

        // Nivel 10
        l.Add(C("embestida", "Embestida", SkillType.Activa, "Universal", "Rare", "DAÑO + Desplazamiento", "0,0", "lunge", "Nivel 10", "Ataca y avanza 2 celdas hacia el objetivo.", 2, 6, 1, 0, 0, 300, 0, 0, 10));

        // Nivel 12 (Pasiva)
        l.Add(C("furia", "Furia", SkillType.Pasiva, "Universal", "Rare", "", "", "furia", "Nivel 12", "+5% crítico.", 0, 0, 0, 0, 0, 400, 0, 0, 12));

        // Nivel 15 (Ultimate)
        l.Add(C("ira_halo", "Ira del Halo", SkillType.Ultimate, "Universal", "Legendary", "DAÑO Devastador", "0,0", "execute", "Nivel 15", "Daño doble si el objetivo está por debajo del 50%.", 0, 20, 2, 3, 0, 500, 0, 0, 15));

        // Nivel 18 (Pasiva)
        l.Add(C("vampirismo", "Vampirismo", SkillType.Pasiva, "Universal", "Epic", "", "", "vampirismo", "Nivel 18", "10% de robo de vida.", 0, 0, 0, 0, 0, 400, 0, 0, 18));

        // === 1.5: HABILIDADES DE ENDGAME (Nivel 20-30) ===
        // Nivel 20 (Pasiva)
        l.Add(C("voluntad_hierro", "Voluntad de Hierro", SkillType.Pasiva, "Universal", "Epic", "", "", "voluntad", "Nivel 20", "+10 Daño base y +10% Crítico.", 0, 0, 0, 0, 0, 0, 0, 0, 20));

        // Nivel 25 (Activa)
        l.Add(C("carga_sacrificial", "Carga Sacrificial", SkillType.Activa, "Universal", "Epic", "DAÑO + Desplazamiento", "0,0", "lunge", "Nivel 25", "Embiste 3 celdas e inflige 15 de daño.", 2, 15, 1, 2, 15, 0, 0, 0, 25));

        // Nivel 30 (Ultimate)
        l.Add(C("sentencia_halo", "Sentencia del Halo", SkillType.Ultimate, "Universal", "Legendary", "DAÑO EJECUCIÓN", "0,0", "execute", "Nivel 30", "Daño cuádruple si el objetivo está por debajo del 30% HP.", 3, 25, 2, 4, 50, 0, 0, 0, 30));

        // Skills del Entrenador (comprables con oro)
        l.Add(C("tiro_preciso", "Tiro Preciso", SkillType.Activa, "Universal", "Common", "Daño", "0,0", "", "Entrenador", "Ataque a distancia (rango 3).", 1, 3, 3, 0, 0, 150, 0, 0, 1));
        l.Add(C("impacto_pesado", "Impacto Pesado", SkillType.Activa, "Universal", "Rare", "Daño", "0,0", "", "Entrenador", "Golpe contundente con +50% daño.", 2, 5, 1, 0, 0, 300, 0, 0, 1));
        l.Add(C("marca", "Marca", SkillType.Activa, "Universal", "Rare", "Debuff de Daño", "0,0", "mark", "Entrenador", "El objetivo marcado recibe +15% daño.", 1, 0, 3, 0, 0, 300, 0, 0, 1));
        l.Add(C("tiron", "Tirón", SkillType.Activa, "Universal", "Epic", "Control", "0,0", "pull", "Entrenador", "Ataca a distancia y atrae al enemigo 1 celda.", 2, 4, 3, 0, 0, 400, 0, 0, 1));
        l.Add(C("curar", "Curar", SkillType.Activa, "Universal", "Common", "Curación", "0,0", "", "Entrenador", "Restaura 4 HP.", 2, 0, 3, 0, 0, 150, 4, 0, 1));
        l.Add(C("escudo", "Escudo", SkillType.Activa, "Universal", "Common", "Defensa", "0,0", "shield", "Entrenador", "+2 DEF durante 3 turnos.", 1, 0, 0, 0, 0, 150, 0, 0, 1));
        l.Add(C("provocacion", "Provocación", SkillType.Activa, "Universal", "Common", "Amenaza", "0,0", "taunt", "Entrenador", "Aumenta tu amenaza y reduce el ataque enemigo 1 turno.", 1, 0, 0, 0, 0, 150, 0, 0, 1));
        l.Add(C("maldicion", "Maldición", SkillType.Activa, "Universal", "Rare", "Debuff", "0,0", "curse", "Entrenador", "-2 ataque enemigo por 3 turnos.", 2, 0, 3, 0, 0, 300, 0, 0, 1));
        l.Add(C("reflejos", "Reflejos", SkillType.Pasiva, "Universal", "Rare", "", "", "reflejos", "Entrenador", "+10% evasión.", 0, 0, 0, 0, 0, 400, 0, 0, 1));
        l.Add(C("impetu", "Ímpetu", SkillType.Pasiva, "Universal", "Rare", "", "", "impetu", "Entrenador", "+1 movimiento.", 0, 0, 0, 0, 0, 400, 0, 0, 1));
        l.Add(C("ejecutor", "Ojos del Ejecutor", SkillType.Pasiva, "Universal", "Epic", "", "", "ejecutor", "Entrenador", "+1 daño por cada 5% de crítico.", 0, 0, 0, 0, 0, 400, 0, 0, 1));
        l.Add(C("coloso", "Coloso", SkillType.Pasiva, "Universal", "Epic", "", "", "coloso", "Entrenador", "+1 daño por cada 10 de HP máximo.", 0, 0, 0, 0, 0, 400, 0, 0, 1));
        l.Add(C("plegaria", "Plegaria Ofensiva", SkillType.Pasiva, "Universal", "Epic", "", "", "plegaria", "Entrenador", "+1 daño por cada 10 de Curación y te cura al golpear.", 0, 0, 0, 0, 0, 400, 0, 0, 1));

        foreach (SkillMeta m in l)
        {
            if (!string.IsNullOrEmpty(m.id))
            {
                cache[m.id] = m;
                dataCache[m.id] = ConvertToSkillData(m);
            }
        }
    }

    static SkillData ConvertToSkillData(SkillMeta m)
    {
        SkillData d = ScriptableObject.CreateInstance<SkillData>();
        d.skillName = m.name;
        d.description = m.desc;
        d.actionPointCost = m.ap;
        d.damage = m.damage;
        d.range = m.range;
        d.bonusCrit = m.bonusCrit;
        d.unlockLevel = m.unlockLevel;
        return d;
    }

    static SkillMeta C(string id, string name, SkillType type, string affinity, string rarity, string tag, string pattern, string effectKey, string origin, string desc, int ap, int damage, int range, int cooldown, int bonusCrit, int cost, int heal, int threat, int unlockLevel)
    {
        SkillMeta m = new SkillMeta();
        m.id = id;
        m.name = name;
        m.type = type;
        m.affinity = affinity;
        m.rarity = rarity;
        m.tag = tag;
        m.pattern = pattern;
        m.effectKey = effectKey;
        m.origin = origin;
        m.desc = desc;
        m.ap = ap;
        m.damage = damage;
        m.range = range;
        m.cooldown = cooldown;
        m.bonusCrit = bonusCrit;
        m.cost = cost;
        m.heal = heal;
        m.threatMult = 1f + threat * 0.1f;
        m.unlockLevel = unlockLevel;
        return m;
    }
}