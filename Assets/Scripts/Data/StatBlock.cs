[System.Serializable]
public class StatBlock
{
    public int maxHP = 10;
    public int defense = 0;
    public int damage = 3;
    public int accuracy = 70;
    public int critChance = 5;
    public int evasion = 5;
    public int apMove = 3;
    public int healingPower = 0;
    public int lifesteal = 0;
    public int healOnHit = 0; // 0.7-E: curación plana por golpe (Set Verde)
    public int worldSpeed = 0; // 0.7-F.1a: bonus de velocidad en el mundo (% sobre speed base)
    public float threatMult = 1f;

    public static StatBlock Zero()
    {
        StatBlock s = new StatBlock();
        s.maxHP = 0;
        s.defense = 0;
        s.damage = 0;
        s.accuracy = 0;
        s.critChance = 0;
        s.evasion = 0;
        s.apMove = 0;
        s.healingPower = 0;
        s.lifesteal = 0;
        s.healOnHit = 0;
        s.worldSpeed = 0;
        s.threatMult = 0f;
        return s;
    }

    public StatBlock Clone()
    {
        return (StatBlock)MemberwiseClone();
    }

    public void Add(StatBlock other)
    {
        maxHP += other.maxHP;
        defense += other.defense;
        damage += other.damage;
        accuracy += other.accuracy;
        critChance += other.critChance;
        evasion += other.evasion;
        apMove += other.apMove;
        healingPower += other.healingPower;
        lifesteal += other.lifesteal;
        healOnHit += other.healOnHit;
        worldSpeed += other.worldSpeed;
        threatMult += other.threatMult;
    }
}