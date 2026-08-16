using UnityEngine;
using UnityEngine.UI;

public class FleeUI : MonoBehaviour
{
    private Text buttonLabel;
    private Image buttonImage;
    private bool waitingConfirm = false;
    private float confirmTimer = 0f;

    private const string LABEL_NORMAL = "SALIR (ESC)";
    private static readonly Color COLOR_NORMAL = new Color(0.35f, 0.08f, 0.08f, 0.9f);
    private static readonly Color COLOR_CONFIRM = new Color(0.6f, 0.1f, 0.1f, 1f);

    void Awake()
    {
        Build();
    }

    void Build()
    {
        GameObject canvas = UIFactory.CreateCanvas("FleeCanvas", 70);

        RectTransform rt = UIFactory.CreatePanel(canvas.transform, "FleeButton",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(170, 40), COLOR_NORMAL);
        buttonImage = rt.GetComponent<Image>();

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = buttonImage;
        btn.onClick.AddListener(OnFleeClicked);

        buttonLabel = UIFactory.CreateText(rt, "Label", LABEL_NORMAL, 14, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        RectTransform lrt = buttonLabel.rectTransform;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (waitingConfirm)
        {
            confirmTimer -= Time.deltaTime;
            if (confirmTimer <= 0f)
            {
                waitingConfirm = false;
                buttonLabel.text = LABEL_NORMAL;
                buttonImage.color = COLOR_NORMAL;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) OnFleeClicked();
    }

    void OnFleeClicked()
    {
        bool inDungeon = Object.FindAnyObjectByType<DungeonManager>() != null;

        // En mazmorra se exige doble confirmación (anti-abuso y anti-misclick).
        if (inDungeon && !waitingConfirm)
        {
            waitingConfirm = true;
            confirmTimer = 3f;
            int penalty = CharacterData.Instance != null ? CharacterData.Instance.gold / 4 : 0;
            buttonLabel.text = "¿SEGURO? -" + penalty + " ORO (ESC)";
            buttonImage.color = COLOR_CONFIRM;
            return;
        }

        Flee(inDungeon);
    }

    void Flee(bool inDungeon)
    {
        if (inDungeon && CharacterData.Instance != null)
        {
            int penalty = CharacterData.Instance.gold / 4;
            CharacterData.Instance.gold -= penalty;
            Debug.Log("Huida de mazmorra: -" + penalty + " oro de penalización.");
        }
        else
        {
            Debug.Log("Huida del combate de mundo sin penalización.");
        }

        GameFlow.ReturnToWorld();
    }
}