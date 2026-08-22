using UnityEngine;
using UnityEditor;
using System.IO;

public static class ClassFactory
{
    [MenuItem("Tools/Generate Class Assets")]
    public static void Generate()
    {
        string dir = "Assets/ScriptableObjects/Classes";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        CreateIfMissing(dir, "RenegadoDeLaCruz", "Renegado de la Cruz", ClassRole.Tank, "Espada y escudo",
            "Vástago de las cruzadas que volvió de la muerte. Alto aguante, buffs ofensivos y generación de amenaza alta.",
            new StatBlock { maxHP = 14, defense = 3, damage = 4, accuracy = 70, critChance = 10, evasion = 5, apMove = 3, healingPower = 0, lifesteal = 5, threatMult = 2f },
            new StatBlock { maxHP = 2, defense = 0, damage = 1, attack = 1, critChance = 0, evasion = 0, apMove = 0, healingPower = 0, lifesteal = 1, threatMult = 0f });

        CreateIfMissing(dir, "SalmodianteHereje", "Salmodiante Hereje", ClassRole.Healer, "Bastón y cetro",
            "Cantor de salmos prohibidos. Curación escalada y buffs defensivos. Su curación genera amenaza alta.",
            new StatBlock { maxHP = 10, defense = 1, damage = 3, accuracy = 75, critChance = 8, evasion = 8, apMove = 3, healingPower = 100, lifesteal = 0, threatMult = 1f },
            new StatBlock { maxHP = 1, defense = 0, damage = 1, accuracy = 1, critChance = 0, evasion = 0, apMove = 0, healingPower = 5, lifesteal = 0, threatMult = 0f });

        CreateIfMissing(dir, "CazadorDeHalos", "Cazador de Halos", ClassRole.DPS, "Látigo",
            "Cazador de seres de luz. Precisión y crítico altos, robo de vida. Látigo melee y ranged.",
            new StatBlock { maxHP = 11, defense = 1, damage = 5, attack = 85, critChance = 25, evasion = 10, apMove = 4, healingPower = 0, lifesteal = 10, threatMult = 1f },
            new StatBlock { maxHP = 1, defense = 0, damage = 1, accuracy = 1, critChance = 1, evasion = 0, apMove = 0, healingPower = 0, lifesteal = 1, threatMult = 0f });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Class assets generated.");
    }

    static void CreateIfMissing(string dir, string file, string name, ClassRole role, string weapon, string desc, StatBlock baseS, StatBlock growth)
    {
        string path = dir + "/" + file + ".asset";
        if (AssetDatabase.LoadAssetAtPath<ClassData>(path) != null) return;

        ClassData data = ScriptableObject.CreateInstance<ClassData>();
        data.className = name;
        data.role = role;
        data.weaponType = weapon;
        data.description = desc;
        data.baseStats = baseS;
        data.growthPerLevel = growth;
        AssetDatabase.CreateAsset(data, path);
    }
}