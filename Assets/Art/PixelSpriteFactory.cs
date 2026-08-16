using UnityEngine;
using System.Collections.Generic;

public static class PixelSpriteFactory
{
    public static Sprite FromMap(string[] rows, Dictionary<char, Color> palette, int pixelsPerUnit)
    {
        int h = rows.Length;
        int w = rows[0].Length;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            string row = rows[y];
            for (int x = 0; x < w; x++)
            {
                char c = x < row.Length ? row[x] : '.';
                int ty = h - 1 - y;
                px[ty * w + x] = palette.TryGetValue(c, out Color col) ? col : Color.clear;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}