using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public int attackRange = 1;
    public int attackDamage = 2;
    public int moveRange = 2;

    private Unit selfUnit;

    void Awake()
    {
        selfUnit = GetComponent<Unit>();
    }

    public IEnumerator ExecuteTurn()
    {
        Debug.Log(gameObject.name + " observa... (IA completa en Paso 4)");
        yield break;
    }
}