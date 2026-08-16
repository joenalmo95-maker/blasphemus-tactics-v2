using UnityEngine;

public class HealthBar2D : MonoBehaviour
{
    private Unit unit;
    private SpriteRenderer fill;
    private float barWidth = 0.9f;

    void Awake()
    {
        unit = GetComponent<Unit>();
        Build();
    }

    void Build()
    {
        GameObject bgObj = new GameObject("HP_BG");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        bgObj.transform.localScale = new Vector3(barWidth, 0.08f, 1);
        SpriteRenderer bg = bgObj.AddComponent<SpriteRenderer>();
        bg.sprite = SpriteFactory.Square();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bg.sortingOrder = 3;

        GameObject fillObj = new GameObject("HP_Fill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        fill = fillObj.AddComponent<SpriteRenderer>();
        fill.sprite = SpriteFactory.Square();
        fill.color = Color.green;
        fill.sortingOrder = 4;

        // Añadir collider para detección de hover del mouse
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(barWidth, 0.15f);
        collider.offset = new Vector2(0, 0.6f);

        // Añadir trigger de tooltip
        UnitTooltipTrigger trigger = gameObject.AddComponent<UnitTooltipTrigger>();
        trigger.targetUnit = unit;
    }

    void LateUpdate()
    {
        if (unit == null || fill == null) return;

        float t = Mathf.Clamp01((float)unit.currentHealth / unit.maxHealth);
        fill.transform.localScale = new Vector3(Mathf.Max(0.001f, barWidth * t), 0.08f, 1);
        fill.transform.localPosition = new Vector3(-(barWidth * (1 - t)) / 2f, 0.6f, 0);
        fill.color = t > 0.5f ? Color.green : (t > 0.25f ? Color.yellow : Color.red);
    }
}