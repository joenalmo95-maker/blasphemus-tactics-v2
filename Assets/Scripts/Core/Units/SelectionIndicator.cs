using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    private Transform ring;

    void Awake()
    {
        GameObject go = new GameObject("SelectionRing");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * 1.6f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Ring();
        sr.color = Color.green;
        sr.sortingOrder = 1;

        ring = go.transform;
    }

    void Update()
    {
        if (ring != null)
        {
            float s = 1.6f + Mathf.Sin(Time.time * 4f) * 0.1f;
            ring.localScale = new Vector3(s, s, 1f);
        }
    }
}