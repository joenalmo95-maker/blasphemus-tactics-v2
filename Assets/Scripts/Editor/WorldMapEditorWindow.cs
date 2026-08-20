using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class WorldMapEditorWindow : EditorWindow
{
    // 0.1: Actualizado a 120x80
    private const int W = 120;
    private const int H = 80;
    private const float CELL = 6f; // Reducido para que quepa en pantalla
    private readonly char[,] grid = new char[W, H];
    private int tool = 1; // 0 caminable, 1 roca, 2 agua, 3 ruinas, 4 bloqueado
    private Vector2 scroll;
    private bool painting;

    static readonly char[] ToolChars = { '.', '#', '~', 'R', 'X' };
    static readonly string[] ToolNames = { "Pasto", "Roca", "Agua", "Ruinas", "Bloqueado" };
    static readonly Color[] ToolColors = {
        new Color(0.3f, 0.6f, 0.3f),
        new Color(0.5f, 0.5f, 0.5f),
        new Color(0.2f, 0.4f, 0.8f),
        new Color(0.6f, 0.4f, 0.3f),
        new Color(0.2f, 0.2f, 0.2f)
    };

    [MenuItem("Tools/World Map Editor")]
    static void Open()
    {
        GetWindow<WorldMapEditorWindow>("World Map Editor");
    }

    void OnEnable()
    {
        Load();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        for (int i = 0; i < ToolNames.Length; i++)
        {
            if (GUILayout.Toggle(tool == i, ToolNames[i], EditorStyles.toolbarButton, GUILayout.Width(90)))
                tool = i;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Guardar", EditorStyles.toolbarButton, GUILayout.Width(90))) Save();
        if (GUILayout.Button("Recargar", EditorStyles.toolbarButton, GUILayout.Width(90))) Load();
        if (GUILayout.Button("Limpiar Región I", EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            for (int x = 0; x < 60; x++)
                for (int y = 0; y < 40; y++)
                    grid[x, y] = '.';
        }
        if (GUILayout.Button("Bloquear Todo", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    grid[x, y] = 'X';
        }
        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll);
        Rect area = GUILayoutUtility.GetRect(W * CELL, H * CELL);

        Event e = Event.current;
        if (e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && e.button == 0))
            painting = true;
        if (e.type == EventType.MouseUp)
            painting = false;

        if (painting && area.Contains(e.mousePosition))
        {
            int gx = Mathf.FloorToInt((e.mousePosition.x - area.x) / CELL);
            int gy = H - 1 - Mathf.FloorToInt((e.mousePosition.y - area.y) / CELL);
            if (gx >= 0 && gx < W && gy >= 0 && gy < H)
            {
                grid[gx, gy] = ToolChars[tool];
                Repaint();
            }
        }

        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                Rect r = new Rect(area.x + x * CELL, area.y + (H - 1 - y) * CELL, CELL, CELL);
                char c = grid[x, y];
                Color col = Color.black;
                switch (c)
                {
                    case '.': col = new Color(0.3f, 0.6f, 0.3f); break;
                    case '#': col = new Color(0.5f, 0.5f, 0.5f); break;
                    case '~': col = new Color(0.2f, 0.4f, 0.8f); break;
                    case 'R': col = new Color(0.6f, 0.4f, 0.3f); break;
                    case 'T': col = new Color(0.1f, 0.4f, 0.1f); break;
                    case 'B': col = new Color(0.6f, 0.5f, 0.3f); break;
                    case 'P': col = new Color(0.2f, 0.8f, 0.2f); break;
                    case 'X': col = new Color(0.2f, 0.2f, 0.2f); break;
                }
                EditorGUI.DrawRect(r, col);
            }
        }

        GUILayout.EndScrollView();
    }

    void Save()
    {
        string path = Path.Combine(Application.dataPath, "Resources", "WorldMapData.txt");
        StringBuilder sb = new StringBuilder();
        for (int y = H - 1; y >= 0; y--)
        {
            for (int x = 0; x < W; x++)
                sb.Append(grid[x, y]);
            if (y > 0) sb.AppendLine();
        }
        File.WriteAllText(path, sb.ToString());
        Debug.Log("[WorldMapEditor] Guardado: " + path);
        AssetDatabase.Refresh();
    }

    void Load()
    {
        string path = Path.Combine(Application.dataPath, "Resources", "WorldMapData.txt");
        if (!File.Exists(path))
        {
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    grid[x, y] = 'X';
            return;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length != H || lines[0].Length != W)
        {
            Debug.LogWarning("[WorldMapEditor] Archivo con tamaño incorrecto. Se cargará parcialmente.");
        }

        for (int y = 0; y < H && y < lines.Length; y++)
        {
            string line = lines[lines.Length - 1 - y];
            for (int x = 0; x < W && x < line.Length; x++)
                grid[x, y] = line[x];
        }
    }
}