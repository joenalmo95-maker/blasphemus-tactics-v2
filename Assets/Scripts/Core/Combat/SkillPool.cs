using UnityEngine;
using System.Collections.Generic;

// 1.1: tipo de skill del pool global
public enum SkillType { Activa, Ultimate, Pasiva }

[System.Serializable]
public class SkillConfig
{
    public string id;
    public string name;
    public string type;
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
    public int unlockLevel;
    public int cost;
    public int heal;
    public int crit;
}

[System.Serializable]
public class SkillConfigFile
{
    public List<SkillConfig> skills = new List<SkillConfig>();
}

// Metadatos extendidos (no rompen SkillData existente)
public class SkillMeta
{
    public string id;
    public SkillType type;
    public string affinity;
    public Rarity rarity;
    public string tag;
    public string effectKey;
    public string origin;
    public int heal;
    public int cooldown;
    public int cost;
    public List<Vector2Int> pattern = new List<Vector2Int>();
}

public static class SkillPool
{
    private static readonly Dictionary<string, SkillData> dataCache = new Dictionary<string, SkillData>();
    private static readonly Dictionary<string, SkillMeta> metaCache = new Dictionary<string, SkillMeta>();
    private static readonly List<string> order = new List<string>();
    private static bool loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        EnsureLoaded();
    }

    public static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        List<SkillConfig> configs = null;
        bool fromJson = false;

        TextAsset asset = Resources.Load<TextAsset>("SkillsConfig");
        if (asset != null)
        {
            SkillConfigFile file = JsonUtility.FromJson<SkillConfigFile>(asset.text);
            if (file != null && file.skills != null && file.skills.Count > 0)
            {
                configs = file.skills;
                fromJson = true;
            }
        }
        if (configs == null) configs = DefaultConfigs();

        foreach (SkillConfig c in configs) Register(c);

        int a = ByType(SkillType.Activa).Count;
        int u = ByType(SkillType.Ultimate).Count;
        int p = ByType(SkillType.Pasiva).Count;
        Debug.Log("[SkillPool] " + order.Count + " skills cargadas (" + a + " activas, " + u + " ultimates, " + p + " pasivas)"
                  + (fromJson ? " desde SkillsConfig.json" : " (fallback hardcodeado)"));

        SkillData demo = Get("impacto_pesado");
        if (demo != null) Debug.Log("[SkillPool] impacto_pesado => daño " + demo.damage + ", AP " + demo.actionPointCost + ", rareza " + Meta("impacto_pesado").rarity);
    }

    static void Register(SkillConfig c)
    {
        if (c == null || string.IsNullOrEmpty(c.id) || dataCache.ContainsKey(c.id)) return;

        SkillData d = ScriptableObject.CreateInstance<SkillData>();
        d.skillName = c.name;
        d.description = c.desc;
        d.actionPointCost = c.ap;
        d.damage = c.damage;
        d.range = c.range;
        d.bonusCrit = c.crit;
        d.unlockLevel = c.unlockLevel;
        d.threatMult = 1f;

        SkillMeta m = new SkillMeta();
        m.id = c.id;
        m.type = ParseType(c.type);
        m.affinity = string.IsNullOrEmpty(c.affinity) ? "Universal" : c.affinity;
        m.rarity = ParseRarity(c.rarity);
        m.tag = c.tag;
        m.effectKey = c.effectKey;
        m.origin = c.origin;
        m.heal = c.heal;
        m.cooldown = c.cooldown;
        m.cost = c.cost;
        m.pattern = ParsePattern(c.pattern);

        dataCache[c.id] = d;
        metaCache[c.id] = m;
        order.Add(c.id);
    }

    public static SkillData Get(string id)
    {
        EnsureLoaded();
        return dataCache.TryGetValue(id, out SkillData d) ? d : null;
    }

    public static SkillMeta Meta(string id)
    {
        EnsureLoaded();
        return metaCache.TryGetValue(id, out SkillMeta m) ? m : null;
    }

    public static List<SkillData> All()
    {
        EnsureLoaded();
        List<SkillData> list = new List<SkillData>();
        foreach (string id in order) list.Add(dataCache[id]);
        return list;
    }

    public static List<string> AllIds()
    {
        EnsureLoaded();
        return new List<string>(order);
    }

    public static List<SkillData> ByType(SkillType t)
    {
        EnsureLoaded();
        List<SkillData> list = new List<SkillData>();
        foreach (string id in order)
        {
            if (metaCache[id].type == t) list.Add(dataCache[id]);
        }
        return list;
    }

    public static List<string> StartersFor(ClassRole role)
    {
        switch (role)
        {
            case ClassRole.Tank: return new List<string> { "golpe_basico", "impacto_pesado", "coloso" };
            case ClassRole.Healer: return new List<string> { "golpe_basico", "curar", "plegaria" };
            default: return new List<string> { "golpe_basico", "tiro_preciso", "ejecutor" };
        }
    }

    public static SkillType ParseType(string s)
    {
        SkillType t;
        if (!string.IsNullOrEmpty(s) && System.Enum.TryParse(s, true, out t)) return t;
        return SkillType.Activa;
    }

    static Rarity ParseRarity(string s)
    {
        Rarity r;
        if (!string.IsNullOrEmpty(s) && System.Enum.TryParse(s, true, out r)) return r;
        return Rarity.Common;
    }

    // "0,0|1,0" -> celdas relativas (para el grid de área tipo Sword x Staff)
    public static List<Vector2Int> ParsePattern(string s)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        if (string.IsNullOrEmpty(s)) return cells;
        foreach (string part in s.Split('|'))
        {
            string[] xy = part.Split(',');
            if (xy.Length == 2)
            {
                int x, y;
                if (int.TryParse(xy[0], out x) && int.TryParse(xy[1], out y))
                    cells.Add(new Vector2Int(x, y));
            }
        }
        return cells;
    }

    // --- Fallback: mismo pool si falta el JSON ---
    static List<SkillConfig> DefaultConfigs()
    {
        List<SkillConfig> l = new List<SkillConfig>();
        l.Add(C("golpe_basico", "Golpe Básico", "Activa", "Universal", "Common", "DAÑO de Golpe Único", "0,0", "", "Starter", "Un golpe sencillo cuerpo a cuerpo.", 1, 1, 1, 0, 0, 0, 0, 0));
        l.Add(C("impacto_pesado", "Impacto Pesado", "Activa", "Universal", "Rare", "DAÑO de Golpe Único y Empuje", "0,0", "knockback", "Entrenador", "Ataca a 1 enemigo adyacente, le inflige daño y lo empuja 1 celda.", 2, 3, 1, 0, 0, 300, 0, 0));
        l.Add(C("barrido", "Barrido", "Activa", "Tank", "Rare", "DAÑO en Área", "0,0|1,0|-1,0|0,1|0,-1", "", "Entrenador", "Golpea a todos los enemigos en cruz alrededor del lanzador.", 2, 2, 1, 0, 0, 300, 0, 0));
        l.Add(C("tiro_preciso", "Tiro Preciso", "Activa", "DPS", "Common", "DAÑO a Distancia", "0,0", "", "Entrenador", "Disparo preciso a distancia.", 2, 2, 3, 0, 0, 150, 0, 0));
        l.Add(C("punalada", "Puñalada", "Activa", "DPS", "Rare", "DAÑO Crítico", "0,0", "", "Entrenador", "Apunta a un punto vital: +20% crítico.", 2, 2, 1, 0, 0, 300, 0, 20));
        l.Add(C("embestida", "Embestida", "Activa", "Universal", "Rare", "DAÑO + Desplazamiento", "0,0", "lunge", "Entrenador", "Ataca y avanza 2 celdas hacia el objetivo.", 2, 1, 1, 0, 0, 300, 0, 0));
        l.Add(C("tiron", "Tirón", "Activa", "Universal", "Epic", "Control", "0,0", "pull", "Entrenador", "Ataca a distancia y atrae al enemigo 1 celda.", 2, 1, 3, 0, 0, 400, 0, 0));
        l.Add(C("curar", "Curar", "Activa", "Healer", "Common", "Curación", "0,0", "", "Entrenador", "Restaura 4 HP.", 2, 0, 3, 0, 0, 150, 4, 0));
        l.Add(C("escudo", "Escudo", "Activa", "Tank", "Common", "Defensa", "0,0", "shield", "Entrenador", "+2 DEF durante 3 turnos.", 1, 0, 0, 0, 0, 150, 0, 0));
        l.Add(C("provocacion", "Provocación", "Activa", "Tank", "Common", "Amenaza", "0,0", "taunt", "Entrenador", "Aumenta tu amenaza y reduce el ataque enemigo 1 turno.", 1, 0, 0, 0, 0, 150, 0, 0));
        l.Add(C("maldicion", "Maldición", "Activa", "Universal", "Rare", "Debuff", "0,0", "curse", "Entrenador", "-2 ataque enemigo por 3 turnos.", 2, 0, 3, 0, 0, 300, 0, 0));
        l.Add(C("marca", "Marca", "Activa", "DPS", "Rare", "Debuff de Daño", "0,0", "mark", "Entrenador", "El objetivo marcado recibe +15% daño.", 1, 0, 3, 0, 0, 300, 0, 0));
        l.Add(C("ira_halo", "Ira del Halo", "Ultimate", "DPS", "Legendary", "DAÑO Devastador", "0,0", "execute", "Entrenador", "Daño doble si el objetivo está por debajo del 50%.", 0, 6, 2, 3, 0, 500, 0, 0));
        l.Add(C("bastion_inquebrantable", "Bastión Inquebrantable", "Ultimate", "Tank", "Legendary", "Supervivencia", "0,0", "bastion", "Entrenador", "Cura 30% de HP máx y +3 DEF 2 turnos.", 0, 0, 0, 3, 0, 500, 0, 0));
        l.Add(C("juicio_divino", "Juicio Divino", "Ultimate", "Healer", "Legendary", "Curación + Daño", "0,0", "juicio", "Entrenador", "Cura 4 HP y golpea con 3 de daño.", 0, 3, 3, 3, 0, 500, 4, 0));
        l.Add(C("coloso", "Coloso", "Pasiva", "Tank", "Epic", "", "", "coloso", "Starter Tanque", "+1 daño por cada 10 de HP máximo.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("plegaria", "Plegaria Ofensiva", "Pasiva", "Healer", "Epic", "", "", "plegaria", "Starter Sanador", "+1 daño por cada 10 de Curación y te cura al golpear.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("ejecutor", "Ojos del Ejecutor", "Pasiva", "DPS", "Epic", "", "", "ejecutor", "Starter DPS", "+1 daño por cada 5% de crítico.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("piel_hierro", "Piel de Hierro", "Pasiva", "Universal", "Rare", "", "", "piel", "Entrenador", "+2 DEF.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("reflejos", "Reflejos", "Pasiva", "Universal", "Rare", "", "", "reflejos", "Entrenador", "+10% evasión.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("vampirismo", "Vampirismo", "Pasiva", "DPS", "Epic", "", "", "vampirismo", "Entrenador", "10% de robo de vida.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("furia", "Furia", "Pasiva", "Universal", "Rare", "", "", "furia", "Entrenador", "+5% crítico.", 0, 0, 0, 0, 0, 400, 0, 0));
        l.Add(C("impetu", "Ímpetu", "Pasiva", "Universal", "Epic", "", "", "impetu", "Entrenador", "+1 celda de movimiento.", 0, 0, 0, 0, 0, 400, 0, 0));
        return l;
    }

    static SkillConfig C(string id, string name, string type, string affinity, string rarity, string tag,
        string pattern, string effectKey, string origin, string desc,
        int ap, int damage, int range, int cooldown, int unlockLevel, int cost, int heal, int crit)
    {
        return new SkillConfig
        {
            id = id, name = name, type = type, affinity = affinity, rarity = rarity, tag = tag,
            pattern = pattern, effectKey = effectKey, origin = origin, desc = desc,
            ap = ap, damage = damage, range = range, cooldown = cooldown,
            unlockLevel = unlockLevel, cost = cost, heal = heal, crit = crit
        };
    }
}