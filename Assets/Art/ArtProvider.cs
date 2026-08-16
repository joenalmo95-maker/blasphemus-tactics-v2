using UnityEngine;
using System.Collections.Generic;

public static class ArtProvider
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Get(string key)
    {
        if (cache.TryGetValue(key, out Sprite s)) return s;
        Sprite built = Build(key);
        cache[key] = built;
        return built;
    }

    static Sprite Build(string key)
    {
        switch (key)
        {
            case "tileA": return PixelSpriteFactory.FromMap(TileMap, TilePalette(new Color(0.13f, 0.12f, 0.12f), new Color(0.10f, 0.09f, 0.09f), new Color(0.06f, 0.06f, 0.06f)), 16);
            case "tileB": return PixelSpriteFactory.FromMap(TileMap, TilePalette(new Color(0.22f, 0.20f, 0.18f), new Color(0.18f, 0.16f, 0.15f), new Color(0.10f, 0.09f, 0.09f)), 16);
            case "tank": return PixelSpriteFactory.FromMap(TankMap, Common(), 12);
            case "healer": return PixelSpriteFactory.FromMap(HealerMap, Common(), 12);
            case "dps": return PixelSpriteFactory.FromMap(DpsMap, Common(), 12);
            case "penitent": return PixelSpriteFactory.FromMap(PenitentMap, Common(), 12);
            case "cherub": return PixelSpriteFactory.FromMap(CherubMap, Common(), 12);
            case "angel": return PixelSpriteFactory.FromMap(AngelMap, Common(), 12);            
            case "inquisitor": return PixelSpriteFactory.FromMap(InquisitorMap, Common(), 12);
            case "capitan": return PixelSpriteFactory.FromMap(CapitanMap, Common(), 12);            
            default: return SpriteFactory.Circle();
        }
    }

    static Dictionary<char, Color> TilePalette(Color a, Color s, Color k)
    {
        return new Dictionary<char, Color> { { 'A', a }, { 's', s }, { 'k', k } };
    }

    static Dictionary<char, Color> Common()
    {
        return new Dictionary<char, Color>
        {
            { 'k', new Color(0.05f, 0.05f, 0.06f) },
            { 'r', new Color(0.70f, 0.10f, 0.10f) },
            { 'w', new Color(0.95f, 0.95f, 0.95f) },
            { 'g', new Color(0.45f, 0.46f, 0.50f) },
            { 'G', new Color(1.00f, 0.80f, 0.30f) },
            { 'b', new Color(0.35f, 0.20f, 0.10f) },
            { 'h', new Color(1.00f, 0.85f, 0.50f) }
        };
    }

    static readonly string[] TileMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kAAAAAAAAAAAAAAk",
        "kAAAsAAAAAsAAAAk",
        "kAAAAAAAAAAAAAAk",
        "kAAAAAsAAAAAsAAk",
        "kAAAAAAAAAAAAAAk",
        "kAsAAAAAsAAAAAsk",
        "kAAAAAAAAAAAAAAk",
        "kAAAAAAAAAAAAAAk",
        "kAAAsAAAAAsAAAAk",
        "kAAAAAAAAAAAAAAk",
        "kAAAAAsAAAAAsAAk",
        "kAsAAAAAsAAAAAsk",
        "kAAAAAAAAAAAAAAk",
        "kAAAAAAAAAAAAAAk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] TankMap =
    {
        ".kkkkkkkkkk.",
        "kkkkkkkkkkkk",
        "kkkkkrrkkkkk",
        "kkkkkrrkkkkk",
        "rrrrrrrrrrrr",
        "rrrrrrrrrrrr",
        "kkkkkrrkkkkk",
        "kkkkkrrkkkkk",
        ".kkkkrrkkkk.",
        ".kkkkkkkkkk.",
        "..kkkkkkkk..",
        "...kkkkkk..."
    };

    static readonly string[] HealerMap =
    {
        "....GGGG....",
        "...GGGGGG...",
        "...GGwGGG...",
        "...GGGGGG...",
        "....GGGG....",
        ".....bb.....",
        ".....bb.....",
        ".....bb.....",
        ".....bb.....",
        ".....bb.....",
        ".....bb.....",
        "....bbbb...."
    };

    static readonly string[] DpsMap =
    {
        ".........rb.",
        "........bb..",
        ".......bb...",
        "......bb....",
        ".....bb.....",
        "....bb......",
        "...bb.......",
        "...bb.......",
        "..bb........",
        "..bb........",
        ".bb.........",
        ".b.........."
    };

    static readonly string[] PenitentMap =
    {
        "....gggggg....",
        "...gggggggg...",
        "...gggggggg...",
        "...gkkkkkkg...",
        "...gggggggg...",
        "....gggggg....",
        "...gggggggg...",
        "..gggrrggggg..",
        "..gggrrggggg..",
        "..ggrrrrrrgg..",
        "..gggrrggggg..",
        "...gggggggg..."
    };

    static readonly string[] CherubMap =
    {
        "...hhhhhh...",
        "...h....h...",
        "............",
        "ww..wwww..ww",
        "www.wwww.www",
        "wwwwwwwwwwww",
        ".www.ww.www.",
        "....wwww....",
        "....wwww....",
        ".....ww.....",
        "............",
        "............"
    };

        static readonly string[] AngelMap =
    {
        ".....hh.....",
        ".....hh.....",
        "....wwww....",
        "ww..wwww..ww",
        "www.wwww.www",
        "wwwwwwwwwwww",
        "wwwwwwwwwwww",
        ".www.ww.www.",
        "..ww.ww.ww..",
        "..ww.ww.ww..",
        "...wwwwww...",
        "....w..w...."
    };

        static readonly string[] InquisitorMap =
    {
        "....kkkk....",
        "...kkkkkk...",
        "...kssssk...",
        "...ksrrsk...",
        "...kkkkkk...",
        "..kkkkkkkk..",
        ".kkkkkkkkkk.",
        ".kkk.kk.kkk.",
        ".kk..kk..kk.",
        "......G.....",
        "......G.....",
        "............"
    };

    static readonly string[] CapitanMap =
    {
        ".....rr.....",
        "....gggg....",
        "...gggggg...",
        "...gkggkg...",
        "...gggggg...",
        "....GGGG....",
        "...GGGGGG...",
        "..GG.GG.GG..",
        "..GG.GG.GG..",
        ".....GG.....",
        "....GGGG....",
        "....G..G...."
    };
}


