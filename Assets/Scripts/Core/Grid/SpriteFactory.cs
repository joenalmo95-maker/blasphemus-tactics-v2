using UnityEngine;

public static class SpriteFactory
{
    private static Sprite _square;
    private static Sprite _circle;
    private static Sprite _ring;

    public static Sprite Square()
    {
        if (_square == null) _square = Build(16, (x, y, s) => true, 16);
        return _square;
    }

    public static Sprite Circle()
    {
        if (_circle == null) _circle = Build(64, (x, y, s) =>
        {
            float dx = (x + 0.5f) / s - 0.5f;
            float dy = (y + 0.5f) / s - 0.5f;
            return (dx * dx + dy * dy) <= 0.25f;
        }, 64);
        return _circle;
    }

    public static Sprite Ring()
    {
        if (_ring == null) _ring = Build(64, (x, y, s) =>
        {
            float dx = (x + 0.5f) / s - 0.5f;
            float dy = (y + 0.5f) / s - 0.5f;
            float d = dx * dx + dy * dy;
            return d <= 0.25f && d >= 0.16f;
        }, 64);
        return _ring;
    }

    private static Sprite Build(int size, System.Func<int, int, int, bool> predicate, int pixelsPerUnit)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color[] px = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                px[y * size + x] = predicate(x, y, size) ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}