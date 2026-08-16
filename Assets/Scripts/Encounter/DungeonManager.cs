using UnityEngine;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }
    public List<WaveDef> waves;
    private int index = -1;

    void Awake()
    {
        Instance = this;
    }

    public void StartDungeon()
    {
        NextWave();
    }

    public bool HasNextWave()
    {
        return waves != null && index + 1 < waves.Count;
    }

    public void NextWave()
    {
        index++;
        Debug.Log("=== OLEADA " + (index + 1) + " DE " + waves.Count + " ===");
        foreach (SpawnDef s in waves[index].spawns)
        {
            EnemyFactory.Spawn(s.archetype, s.tier, s.cell);
        }
    }
}