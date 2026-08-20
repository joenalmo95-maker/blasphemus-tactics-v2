using UnityEngine;

public enum ClassRole { Tank, Healer, DPS }

[CreateAssetMenu(fileName = "NewClass", menuName = "Classes/ClassData")]
public class ClassData : ScriptableObject
{
    public string className = "Nueva Clase";
    public ClassRole role = ClassRole.DPS;
    public string weaponType = "Espada";
    public string artKey = "dps";  // ← AÑADIDO para CharacterData.cs
    [TextArea] public string description = "";
    public StatBlock baseStats = new StatBlock();
    public StatBlock growthPerLevel = new StatBlock();
}