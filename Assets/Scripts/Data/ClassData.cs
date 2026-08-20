using UnityEngine;

// 0.3: Clase mínima de compatibilidad. Valerius es el único protagonista.
[System.Serializable]
[CreateAssetMenu(fileName = "NewClass", menuName = "Blasphemus/Class")]
public class ClassData : ScriptableObject
{
    public string className = "Inquisidor";
    public ClassRole role = ClassRole.DPS;
    public string description = "Inquisidor del Bastión de San Veritas. Único protagonista de La Liturgia del Cielo.";
    public string artKey = "inquisitor";
    
    // Stats base (ahora se usan los de CharacterData, pero se mantienen para compatibilidad)
    public StatBlock baseStats = StatBlock.Zero();
    public StatBlock growthPerLevel = StatBlock.Zero();
}

public enum ClassRole
{
    Tank,
    Healer,
    DPS
}