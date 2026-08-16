using UnityEngine;
using System.Collections.Generic;

public class PersistentManagers : MonoBehaviour
{
    public static PersistentManagers Instance { get; private set; }
    public List<ClassData> availableClasses = new List<ClassData>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (FindAnyObjectByType<CharacterData>() == null)
        {
            GameObject cd = new GameObject("CharacterData");
            cd.AddComponent<CharacterData>();
            DontDestroyOnLoad(cd);
        }

        if (FindAnyObjectByType<InventorySystem>() == null)
        {
            GameObject inv = new GameObject("InventorySystem");
            inv.AddComponent<InventorySystem>();
            DontDestroyOnLoad(inv);
        }
    }
}