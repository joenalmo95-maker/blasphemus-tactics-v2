using UnityEngine;

public static class CombatFeedback
{
    public static void SpawnText(Vector3 position, string text, Color color)
    {
        GameObject go = new GameObject("DamagePopup");
        go.transform.position = position + new Vector3(0, 0.6f, -0.5f);

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.12f;
        tm.fontSize = 64;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            tm.font = font;
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = font.material;
                mr.sortingOrder = 20;
            }
        }

        go.AddComponent<PopupBehaviour>().Init(tm);
    }

    public static void SpawnDamage(Vector3 position, int amount)
    {
        SpawnText(position, "-" + amount, Color.red);
    }

    public static void SpawnImpact(Vector3 position, Color color)
    {
        GameObject go = new GameObject("Impact");
        go.transform.position = position + new Vector3(0, 0, -0.2f);
        go.transform.localScale = Vector3.one * 0.3f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle();
        sr.color = color;
        sr.sortingOrder = 5;

        go.AddComponent<ImpactBehaviour>();
    }
}

public class PopupBehaviour : MonoBehaviour
{
    private TextMesh tm;
    private float lifetime = 1f;
    private float age = 0f;
    private Color startColor;

    public void Init(TextMesh textMesh)
    {
        tm = textMesh;
        startColor = tm.color;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime * 1.2f;

        float t = Mathf.Clamp01(age / lifetime);
        Color c = startColor;
        c.a = 1f - t;
        tm.color = c;

        if (age >= lifetime) Destroy(gameObject);
    }
}

public class ImpactBehaviour : MonoBehaviour
{
    private float age = 0f;
    private float lifetime = 0.3f;
    private SpriteRenderer sr;
    private Color start;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        start = sr.color;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / lifetime);
        transform.localScale = Vector3.one * (0.3f + t * 1.2f);

        Color c = start;
        c.a = 1f - t;
        sr.color = c;

        if (age >= lifetime) Destroy(gameObject);
    }
}