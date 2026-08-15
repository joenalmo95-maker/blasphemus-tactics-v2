using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterCreationUI : MonoBehaviour
{
    public List<ClassData> availableClasses = new List<ClassData>();

    private GameObject canvasObj;

    public void Build()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        canvasObj = new GameObject("CreationCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Background");
        panel.transform.SetParent(canvasObj.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        Image pimg = panel.AddComponent<Image>();
        pimg.sprite = SpriteFactory.Square();
        pimg.color = new Color(0.02f, 0.02f, 0.03f, 0.92f);

        CreateText(canvasObj.transform, "ELIGE TU CLASE", 160, 32);

        float y = 60;
        foreach (ClassData cd in availableClasses)
        {
            CreateButton(canvasObj.transform, cd.className + " (" + RoleLabel(cd.role) + ")", y, cd);
            y -= 60;
        }

        CreateText(canvasObj.transform, "El destino del Renacido depende de tu elección.", -160, 16);
    }

    string RoleLabel(ClassRole role)
    {
        switch (role)
        {
            case ClassRole.Tank: return "Tanque";
            case ClassRole.Healer: return "Sanador";
            default: return "DPS";
        }
    }

    void OnClassSelected(ClassData cd)
    {
        Debug.Log("[Creation] Clase elegida: " + cd.className);

        CharacterData data = CharacterData.Instance != null ? CharacterData.Instance : FindAnyObjectByType<CharacterData>();
        Debug.Log("[Creation] CharacterData encontrado: " + (data != null));
        if (data != null) data.SetClass(cd);

        Bootstrap boot = Bootstrap.Instance != null ? Bootstrap.Instance : FindAnyObjectByType<Bootstrap>();
        Debug.Log("[Creation] Bootstrap encontrado: " + (boot != null));
        if (boot != null) boot.SpawnPlayer();

        TurnManager tm = TurnManager.Instance != null ? TurnManager.Instance : FindAnyObjectByType<TurnManager>();
        Debug.Log("[Creation] TurnManager encontrado: " + (tm != null));
        if (tm != null) tm.BeginGame();

        Destroy(canvasObj);
        Destroy(this);
    }

    void CreateText(Transform parent, string content, float yOffset, int size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(700, 50);

        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    void CreateButton(Transform parent, string label, float yOffset, ClassData cd)
    {
        GameObject go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(380, 50);

        Image img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Square();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

        Button btn = go.AddComponent<Button>();

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(go.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = GetFont();
        t.fontSize = 18;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;

        btn.onClick.AddListener(() => OnClassSelected(cd));
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}