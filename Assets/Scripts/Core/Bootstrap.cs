using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Start()
    {
        if (FindAnyObjectByType<Unit>() == null)
        {
            Unit.Create("Renacido", new Vector2Int(1, 1), false, new Color(0.45f, 0.08f, 0.08f));
        }
    }
}