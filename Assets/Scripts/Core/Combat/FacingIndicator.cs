using UnityEngine;

// 1.1-E: triángulo direccional sobre cada enemigo (blanco/amarillo/rojo por tier)
public class FacingIndicator : MonoBehaviour
{
    private Transform arrow;

    void Awake()
    {
        Unit u = GetComponent<Unit>();
        GameObject go = new GameObject("FacingArrow");
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square();
        sr.color = (u != null && u.isBoss) ? Color.red : ((u != null && u.isElite) ? Color.yellow : Color.white);
        sr.sortingOrder = 3;
        go.transform.localScale = new Vector3(0.28f, 0.10f, 1f);
        arrow = go.transform;
    }

    void LateUpdate()
    {
        Unit u = GetComponent<Unit>();
        if (u == null || arrow == null) return;
        float ang = Mathf.Atan2(u.facing.y, u.facing.x) * Mathf.Rad2Deg;
        arrow.localRotation = Quaternion.Euler(0, 0, ang);
        arrow.localPosition = new Vector3(u.facing.x * 0.55f, u.facing.y * 0.55f, -0.15f);
    }
}