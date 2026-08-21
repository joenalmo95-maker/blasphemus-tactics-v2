using UnityEngine;

public class HealthBar2D : MonoBehaviour
{
    private Unit unit;
    private SpriteRenderer fill;
    private GameObject borderObj;
    private float barWidth = 0.9f;

    void Awake()
    {
        unit = GetComponent<Unit>();
        Build();
    }

    void Build()
    {
        // Determinar estilo según tier del enemigo
        EnemyTier tier = EnemyTier.Basico;
        EnemyAI ai = GetComponent<EnemyAI>();
        BossAI bossAi = GetComponent<BossAI>();
        
        if (bossAi != null) tier = bossAi.tier;
        else if (ai != null) tier = ai.tier;
        
        float scaleMult = 1f;
        Color borderColor = Color.clear;
        
        switch (tier)
        {
            case EnemyTier.EliteFuerte:
                scaleMult = 1.3f;
                borderColor = new Color(0.9f, 0.5f, 0.2f, 1f); // naranja
                break;
            case EnemyTier.Elite:
                scaleMult = 1.15f;
                borderColor = new Color(0.9f, 0.8f, 0.3f, 1f); // amarillo
                break;
            case EnemyTier.Medio:
                scaleMult = 1.05f;
                borderColor = Color.clear;
                break;
            default:
                scaleMult = 1f;
                borderColor = Color.clear;
                break;
        }
        
        float finalWidth = barWidth * scaleMult;
        
        // Fondo
        GameObject bgObj = new GameObject("HP_BG");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        bgObj.transform.localScale = new Vector3(finalWidth, 0.08f, 1);
        SpriteRenderer bg = bgObj.AddComponent<SpriteRenderer>();
        bg.sprite = SpriteFactory.Square();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bg.sortingOrder = 3;
        
        // Borde (solo para élites)
        if (borderColor.a > 0)
        {
            borderObj = new GameObject("HP_Border");
            borderObj.transform.SetParent(transform);
            borderObj.transform.localPosition = new Vector3(0, 0.6f, 0);
            borderObj.transform.localScale = new Vector3(finalWidth + 0.06f, 0.14f, 1);
            SpriteRenderer brdSr = borderObj.AddComponent<SpriteRenderer>();
            brdSr.sprite = SpriteFactory.Square();
            brdSr.color = borderColor;
            brdSr.sortingOrder = 2;
        }
        
        // Relleno
        GameObject fillObj = new GameObject("HP_Fill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        fill = fillObj.AddComponent<SpriteRenderer>();
        fill.sprite = SpriteFactory.Square();
        fill.color = Color.green;
        fill.sortingOrder = 4;

        // Collider para hover
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(finalWidth, 0.15f);
        collider.offset = new Vector2(0, 0.6f);

        // Tooltip trigger
        UnitTooltipTrigger trigger = gameObject.AddComponent<UnitTooltipTrigger>();
        trigger.targetUnit = unit;
    }

    void LateUpdate()
    {
        if (unit == null || fill == null) return;

        float t = Mathf.Clamp01((float)unit.currentHealth / unit.maxHealth);
        
        EnemyTier tier = EnemyTier.Basico;
        EnemyAI ai = GetComponent<EnemyAI>();
        BossAI bossAi = GetComponent<BossAI>();
        if (bossAi != null) tier = bossAi.tier;
        else if (ai != null) tier = ai.tier;
        
        float scaleMult = 1f;
        if (tier == EnemyTier.EliteFuerte) scaleMult = 1.3f;
        else if (tier == EnemyTier.Elite) scaleMult = 1.15f;
        else if (tier == EnemyTier.Medio) scaleMult = 1.05f;
        
        float finalWidth = barWidth * scaleMult;
        
        fill.transform.localScale = new Vector3(Mathf.Max(0.001f, finalWidth * t), 0.08f, 1);
        fill.transform.localPosition = new Vector3(-(finalWidth * (1 - t)) / 2f, 0.6f, 0);
        fill.color = t > 0.5f ? Color.green : (t > 0.25f ? Color.yellow : Color.red);
        
        // Ocultar barra de boss (usa BossHealthBarUI en su lugar)
        if (unit.isBoss)
        {
            fill.enabled = false;
            if (borderObj != null) borderObj.SetActive(false);
        }
    }
}