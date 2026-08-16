using UnityEngine;

public class WorldPlayerController : MonoBehaviour
{
    public float speed = 6f;
    private TextMesh prompt;
    private WorldBootstrap.Zone currentZone;
    private bool inZone = false;

    void Awake()
    {
        GameObject p = new GameObject("Prompt");
        p.transform.SetParent(transform);
        p.transform.localPosition = new Vector3(0, 1, 0);
        prompt = p.AddComponent<TextMesh>();
        prompt.fontSize = 48;
        prompt.characterSize = 0.05f;
        prompt.alignment = TextAlignment.Center;
        prompt.anchor = TextAnchor.MiddleCenter;
        prompt.color = Color.yellow;
        prompt.text = "";
        MeshRenderer mr = p.GetComponent<MeshRenderer>();
        mr.sortingOrder = 10;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;
        transform.position += dir * speed * Time.deltaTime;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        CheckZones();
    }

    void CheckZones()
    {
        inZone = false;
        foreach (var z in WorldBootstrap.zones)
        {
            if (Mathf.Abs(transform.position.x - z.center.x) <= z.size.x / 2 &&
                Mathf.Abs(transform.position.y - z.center.y) <= z.size.y / 2)
            {
                currentZone = z;
                inZone = true;
                break;
            }
        }

        if (inZone)
        {
            prompt.text = currentZone.label + "\nPulsa E para combatir";
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameFlow.EnterCombat(currentZone.tier, currentZone.waves);
            }
        }
        else
        {
            prompt.text = "";
        }
    }
}