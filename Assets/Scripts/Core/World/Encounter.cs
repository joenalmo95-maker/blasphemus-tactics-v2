using UnityEngine;

public enum EncounterType
{
    Emboscada,
    Tesoro,
    Santuario,
    MercaderErrante,
    Cazador
}

public class Encounter
{
    public int id;
    public EncounterType type;
    public Vector2Int cell;
    public GameObject go;
    public bool consumed;
    public float spawnedAt;
    public float cooldownUntil;
    
    // Datos específicos por tipo
    public EnemyTier tier; // emboscada/tesoro
    public string[] enemyArchetypes; // emboscada
    public int goldReward; // tesoro
    public string hunterContract; // cazador (quest id)
    public long hunterExpiry; // cazador (timestamp)
}