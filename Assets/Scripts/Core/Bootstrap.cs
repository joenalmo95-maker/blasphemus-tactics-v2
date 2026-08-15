using UnityEngine;
using System.Collections.Generic;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }
    public List<ClassData> availableClasses = new List<ClassData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[Bootstrap] Duplicado detectado. Se destruye este objeto.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[Bootstrap] Awake correcto.");

        Unit enemy = Unit.Create("Cruzado", new Vector2Int(7, 4), true, new Color(0.35f, 0.36f, 0.40f));
        enemy.gameObject.AddComponent<EnemyAI>();

        GameObject cdObj = new GameObject("CharacterData");
        cdObj.AddComponent<CharacterData>();

        if (availableClasses.Count > 0)
        {
            GameObject uiObj = new GameObject("CharacterCreation");
            CharacterCreationUI ui = uiObj.AddComponent<CharacterCreationUI>();
            ui.availableClasses = availableClasses;
            ui.Build();
        }
        else
        {
            SpawnPlayer();
            if (TurnManager.Instance != null) TurnManager.Instance.BeginGame();
        }
    }

    public void SpawnPlayer()
    {
        Debug.Log("[Bootstrap] SpawnPlayer invocado.");

        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy)
            {
                Debug.Log("[Bootstrap] Ya existe un jugador (" + u.name + "). No se spawnea.");
                return;
            }
        }

        StatBlock stats = CharacterData.Instance != null
            ? CharacterData.Instance.GetDerivedStats()
            : new StatBlock();

        Unit.Create("Renacido", new Vector2Int(1, 1), false,
            new Color(0.45f, 0.08f, 0.08f), 0.8f, stats.maxHP, stats.apMove);

        string className = (CharacterData.Instance != null && CharacterData.Instance.classData != null)
            ? CharacterData.Instance.classData.className
            : "Sin clase";

        Debug.Log("Renacido creado como " + className + " | HP: " + stats.maxHP + " | AP: " + stats.apMove);
    }
}