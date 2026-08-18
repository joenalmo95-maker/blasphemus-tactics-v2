using UnityEngine;

// 1.1-E: arcos de sector (frontal gris / lateral amarillo / espalda rojo) al apuntar melee
public class FlankIndicator : MonoBehaviour
{
    private static FlankIndicator inst;

    public static void Show(Unit target, Vector2Int attackerPos)
    {
        Hide();
        GameObject go = new GameObject("FlankIndicator");
        FlankIndicator f = go.AddComponent<FlankIndicator>();
        f.Build(target, attackerPos);
        inst = f;
    }

    public static void Hide()
    {
        if (inst != null)
        {
            Destroy(inst.gameObject);
            inst = null;
        }
    }

    void Build(Unit target, Vector2Int attackerPos)
    {
        transform.position = new Vector3(target.currentGridPos.x, target.currentGridPos.y, -0.3f);

        float baseAng = Mathf.Atan2(target.facing.y, target.facing.x) * Mathf.Rad2Deg;
        FlankType current = target.GetFlankFromPos(attackerPos);

        DrawArc(baseAng - 60f, baseAng + 60f, current == FlankType.Frontal ? Color.white : Color.gray, current == FlankType.Frontal);
        DrawArc(baseAng + 60f, baseAng + 120f, current == FlankType.Lateral ? Color.white : Color.yellow, current == FlankType.Lateral);
        DrawArc(baseAng - 120f, baseAng - 60f, current == FlankType.Lateral ? Color.white : Color.yellow, current == FlankType.Lateral);
        DrawArc(baseAng + 120f, baseAng + 240f, current == FlankType.Espalda ? Color.white : Color.red, current == FlankType.Espalda);
    }

    void DrawArc(float fromDeg, float toDeg, Color col, bool highlight)
    {
        GameObject go = new GameObject("Arc");
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = col;
        lr.endColor = col;
        lr.startWidth = highlight ? 0.14f : 0.06f;
        lr.endWidth = highlight ? 0.14f : 0.06f;
        lr.positionCount = 13;
        lr.useWorldSpace = false;
        lr.sortingOrder = 4;
        float r = 0.78f;
        for (int i = 0; i < 13; i++)
        {
            float a = Mathf.Lerp(fromDeg, toDeg, i / 12f) * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0));
        }
    }
}