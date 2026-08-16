using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterCreationUI : MonoBehaviour
{
    public List<ClassData> availableClasses = new List<ClassData>();
    public bool showContinue = false;
    public System.Action onFinished;

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

        CreateText(canvasObj.transform, "ELIGE TU CLASE", 200, 32);

        float y = 120;

        if (showContinue)
        {
            CreateButton(canvasObj.transform, "CONTINUAR PARTIDA", y, Color.green, () => OnContinue());
            y -= 50;
            CreateText(canvasObj.transform, "— o nueva partida —", y, 14);
            y -= 40;
        }
        else
        {
            y = 60;
        }

        foreach (ClassData cd in availableClasses)
        {
            ClassData captured = cd;
            CreateButton(canvasObj.transform, cd.className + " (" + RoleLabel(cd.role) + ")", y, Color.white,
                () => OnClassSelected(captured));
            y -= 60;
        }

        CreateText(canvasObj.transform, "El destino del Renacido depende de tu elección.", -200, 16);
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

    void OnContinue()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        Debug.Log("[Creation] Continuando partida guardada: " + data.className + " Nv " + data.level);

        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.LoadFrom(data, availableClasses);
        }

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.LoadFrom(data);
        }

        FinishCreation();
    }

    void OnClassSelected(ClassData cd)
    {
        Debug.Log("[Creation] Clase elegida: " + cd.className);
        if (CharacterData.Instance != null) CharacterData.Instance.SetClass(cd);
        FinishCreation();
    }

    void FinishCreation()
    {
        if (onFinished != null)
        {
            onFinished();
        }
        else
        {
            if (Bootstrap.Instance != null) Bootstrap.Instance.SpawnPlayer();
            if (TurnManager.Instance != null) TurnManager.Instance.BeginGame();
        }

        Destroy(canvasObj);
        Destroy(this);
    }

    void CreateButton(Transform parent, string label, float yOffset, Color textColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(380, 44);

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
        t.color = textColor;

        btn.onClick.AddListener(onClick);
    }

    void CreateText(Transform parent, string content, float yOffset, int size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(700, 40);

        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetFont();
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}