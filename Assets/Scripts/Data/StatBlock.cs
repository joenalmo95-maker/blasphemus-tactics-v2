[System.Serializable]
public class StatBlock
{
    public int maxHP = 10;
    public int defense = 0;
    public int damage = 3;
    public int attack = 70;
    public int critChance = 5;
    public int evasion = 5;
    public int apMove = 3;
    public int healingPower = 0;
    public int lifesteal = 0;
    public float threatMult = 1f;

    public StatBlock Clone()
    {
        return (StatBlock)MemberwiseClone();
    }

    public void Add(StatBlock other)
    {
        maxHP += other.maxHP;
        defense += other.defense;
        damage += other.damage;
        attack += other.attack;
        critChance += other.critChance;
        evasion += other.evasion;
        apMove += other.apMove;
        healingPower += other.healingPower;
        lifesteal += other.lifesteal;
        threatMult += other.threatMult;
    }
}