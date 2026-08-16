using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class WorldMapEditorWindow : EditorWindow
{
    // Debe coincidir con WorldBootstrap.WorldWidth/WorldHeight
    private const int W = 60;
    private const int H = 40;
    private const float CELL = 12f;

    private readonly char[,] grid = new char[W, H];
    private int tool = 1; // 0 caminable (borrador), 1 roca, 2 agua, 3 ruinas
    private Vector2 scroll;
    private bool painting;

    static readonly char[] ToolChars = { '.', 'R', 'W', 'U' };
    static readonly string[] ToolNames = { "Borrador", "Roca", "Agua", "Ruinas" };

    [MenuItem("Tools/World Map Editor")]
    static void Open()
    {
        var win = GetWindow<WorldMapEditorWindow>("World Map Editor");
        win.minSize = new Vector2(900, 700);
        win.Load();
    }

    void OnEnable() { Load(); }

    string FilePath => Path.Combine(Application.dataPath, "Resources/WorldMapData.txt");

    void Load()
    {
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                grid[x, y] = '.';

        if (!File.Exists(FilePath)) return;

        string[] lines = File.ReadAllLines(FilePath);
        for (int i = 0; i < lines.Length; i++)
        {
            int y = H - 1 - i; // fila 0 del archivo = borde superior (y máximo)
            if (y < 0) break;
            string line = lines[i].TrimEnd('\r');
            for (int x = 0; x < W && x < line.Length; x++)
            {
                char c = line[x];
                grid[x, y] = (c == 'R' || c == 'W' || c == 'U') ? c : '.';
            }
        }
    }

    void Save()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources"));
        StringBuilder sb = new StringBuilder();
        for (int y = H - 1; y >= 0; y--)
        {
            for (int x = 0; x < W; x++) sb.Append(grid[x, y]);
            sb.AppendLine();
        }
        File.WriteAllText(FilePath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[WorldMapEditor] WorldMapData.txt guardado en Assets/Resources.");
    }

    void OnGUI()
    {
        GUILayout.Label("Editor de Mapa Mundial (60x40) — pinte con clic/arrastre", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        for (int i = 0; i < ToolNames.Length; i++)
        {
            if (GUILayout.Toggle(tool == i, ToolNames[i], EditorStyles.toolbarButton, GUILayout.Width(90)))
                tool = i;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Guardar", EditorStyles.toolbarButton, GUILayout.Width(90))) Save();
        if (GUILayout.Button("Recargar", EditorStyles.toolbarButton, GUILayout.Width(90))) Load();
        if (GUILayout.Button("Limpiar", EditorStyles.toolbarButton, GUILayout.Width(90)))
        {
            for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) grid[x, y] = '.';
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Leyenda: gris=Roca, azul=Agua, marrón=Ruinas. El juego carga este mapa al iniciar WorldMap.", EditorStyles.miniLabel);

        scroll = GUILayout.BeginScrollView(scroll);
        Rect area = GUILayoutUtility.GetRect(W * CELL, H * CELL, GUILayout.ExpandWidth(false));
        EditorGUI.DrawRect(area, new Color(0.12f, 0.12f, 0.12f));

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                char c = grid[x, y];
                if (c == '.') continue;
                Color col = c == 'R' ? new Color(0.55f, 0.55f, 0.6f)
                          : c == 'W' ? new Color(0.15f, 0.4f, 0.8f)
                          : new Color(0.55f, 0.42f, 0.28f);
                EditorGUI.DrawRect(CellRect(area, x, y), col);
            }
        }

        if (area.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) painting = true;
            if (Event.current.type == EventType.MouseUp) painting = false;

            if (painting && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown))
            {
                int cx = Mathf.Clamp((int)((Event.current.mousePosition.x - area.x) / CELL), 0, W - 1);
                int cy = Mathf.Clamp((int)((area.y + area.height - Event.current.mousePosition.y) / CELL), 0, H - 1);
                grid[cx, cy] = ToolChars[tool];
                Event.current.Use();
            }
        }

        if (Event.current.type == EventType.MouseUp) painting = false;
        GUILayout.EndScrollView();

        Repaint();
    }

    Rect CellRect(Rect area, int x, int y)
    {
        return new Rect(area.x + x * CELL, area.y + area.height - (y + 1) * CELL, CELL - 1, CELL - 1);
    }
}