using UnityEngine;
using UnityEngine.EventSystems;

// Garantiza EventSystem en TODAS las escenas desde el arranque
// (corrige tooltips que no aparecen al abrir una UI por primera vez)
public static class EventSystemGuardian
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Ensure()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[EventSystemGuardian] EventSystem creado al arranque.");
        }
    }
}