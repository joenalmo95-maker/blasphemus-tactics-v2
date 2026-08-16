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
            case "rock": return PixelSpriteFactory.FromMap(RockMap, RockPalette(), 16);
            case "water": return PixelSpriteFactory.FromMap(WaterMap, WaterPalette(), 16);
            case "ruins": return PixelSpriteFactory.FromMap(RuinsMap, RuinsPalette(), 16);
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

    static Dictionary<char, Color> RockPalette()
    {
        return new Dictionary<char, Color>
        {
            { 'k', Color.clear },
            { 'B', new Color(0.40f, 0.40f, 0.45f) },
            { 'C', new Color(0.30f, 0.30f, 0.35f) },
            { 'D', new Color(0.20f, 0.20f, 0.25f) }
        };
    }

    static Dictionary<char, Color> WaterPalette()
    {
        return new Dictionary<char, Color>
        {
            { 'A', new Color(0.10f, 0.30f, 0.60f) },
            { 's', new Color(0.20f, 0.50f, 0.80f) }
        };
    }

    static Dictionary<char, Color> RuinsPalette()
    {
        return new Dictionary<char, Color>
        {
            { 'k', Color.clear },
            { 'B', new Color(0.35f, 0.30f, 0.25f) }
        };
    }

    // --- Mapas de terreno ---
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

    static readonly string[] RockMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkBBBBBBBBkkkk",
        "kkkBBCCCCCCBBkkk",
        "kkBCCDDDDDCBBkkk",
        "kBCCDDDDDDDCBkkk",
        "kBCCDDDDDDDCBkkk",
        "kBCCDDDDDDDCBkkk",
        "kBCCDDDDDDDCBkkk",
        "kBCCDDDDDDDCBkkk",
        "kkBCCDDDDDCBBkkk",
        "kkkBBCCCCCCBBkkk",
        "kkkkBBBBBBBBkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] WaterMap =
    {
        "AAAAAAAAAAAAAAAA",
        "AsAsAsAsAsAsAsAs",
        "AAAAAAAAAAAAAAAA",
        "AAAAAAAAAAAAAAAA",
        "AsAsAsAsAsAsAsAs",
        "AAAAAAAAAAAAAAAA",
        "AAAAAAAAAAAAAAAA",
        "AsAsAsAsAsAsAsAs",
        "AAAAAAAAAAAAAAAA",
        "AAAAAAAAAAAAAAAA",
        "AsAsAsAsAsAsAsAs",
        "AAAAAAAAAAAAAAAA",
        "AAAAAAAAAAAAAAAA",
        "AsAsAsAsAsAsAsAs",
        "AAAAAAAAAAAAAAAA",
        "AAAAAAAAAAAAAAAA"
    };

    static readonly string[] RuinsMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "BBkkkkkkkkkkkkBB",
        "BBkkkkkkkkkkkkBB",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkBBkkkkkkkkBBkk",
        "kkkkkkkkkkkkkkkk"
    };

    // --- Mapas de unidades (usando paleta Common: k, r, w, g, G, b, h) ---
    static readonly string[] TankMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkggkgkgkkkkk",
        "kkkkkwwwwkwkkkkk",
        "kkkkkhhhhwkkkkkk",
        "kkkkGgggGGkkkkkk",
        "kkkGggggggGkkkkk",
        "kkkGggggrgGkkkkk",
        "kkkGgggggrGkkkkk",
        "kkkGGggggGGkkkkk",
        "kkkkGgggggGkkkkk",
        "kkkkbggggbkkkkkk",
        "kkkkbbggbbkkkkkk",
        "kkkkbggggbkkkkkk",
        "kkkkbkbkbkkkkkkk",
        "kkkkbkbkbkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] HealerMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkGGGGkkkkkkk",
        "kkkkGGwwGGkkkkkk",
        "kkkkGhhhGkkkkkkk",
        "kkkkwwwwkkkkkkkk",
        "kkkwGwwGwkkkkkkk",
        "kkkwwwGwwwkkkkkk",
        "kkkwGwwGwkkkkkkk",
        "kkkkwwwwkkkkkkkk",
        "kkkkkwwkkkkkkkkk",
        "kkkkkbbkkkkkkkkk",
        "kkkkbkkbkkkkkkkk",
        "kkkkbkkbkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] DpsMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkrrrkkrkkkkk",
        "kkkkkhhhhrkkkkkk",
        "kkkkrrrrrrrkkkkk",
        "kkkkkbbrbkkkkkkk",
        "kkkkbrrrrbkkkkkk",
        "kkkbrrrrrrbkkkkk",
        "kkkbrrrrrrbkkkkk",
        "kkkkbrrrrbkkkkkk",
        "kkkkkbbrbkkkkkkk",
        "kkkkkbbbbkkkkkkk",
        "kkkkbkkkkbkkkkkk",
        "kkkkbkkkkbkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] PenitentMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkrrrrkkkkkkk",
        "kkkkrggggkkkkkkk",
        "kkkkkgggkkkkkkkk",
        "kkkkkggggkkkkkkk",
        "kkkkggggggkkkkkk",
        "kkkkggrrggkkkkkk",
        "kkkkggggggkkkkkk",
        "kkkkkggggkkkkkkk",
        "kkkkkbkkbkkkkkkk",
        "kkkkkbkkbkkkkkkk",
        "kkkkbbbbbbkkkkkk",
        "kkkkbkbkbkkkkkkk",
        "kkkkbkbkbkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] CherubMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkwwwwkkkkkkk",
        "kkkkwwwwwwkkkkkk",
        "kkkkwhhhhwwkkkkk",
        "kkkwwwwwwwkkkkkk",
        "kkwwwwwwwwwkkkkk",
        "kkwwwrrrrwwkkkkk",
        "kkwwwwwwwwwkkkkk",
        "kkkwwwwwwwkkkkkk",
        "kkkkwwwwwwkkkkkk",
        "kkkkkwwwwkkkkkkk",
        "kkkkkkwwkkkkkkkk",
        "kkkkkkwwkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] AngelMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkGGGGGGkkkkkk",
        "kkkGGGGGGGGkkkkk",
        "kkGGGwwwwGGGkkkk",
        "kkGGGhhhGGkkkkkk",
        "kkkGGGGGGGkkkkkk",
        "kkGGGrrrGGGkkkkk",
        "kkGGGGGGGGGkkkkk",
        "kkkGGGGGGGkkkkkk",
        "kkkkGGGGGkkkkkkk",
        "kkkkkGGGkkkkkkkk",
        "kkkkkGwGkkkkkkkk",
        "kkkkkwkwkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] InquisitorMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkkrrrkkkkkkk",
        "kkkkkrrrrrkkkkkk",
        "kkkkkrrrrrkkkkkk",
        "kkkkkggggkkkkkkk",
        "kkkkkggggkkkkkkk",
        "kkkkbbbbbkkkkkkk",
        "kkkbbrrrbbkkkkkk",
        "kkkbbbbbbbkkkkkk",
        "kkkkbbbbbkkkkkkk",
        "kkkkkbkbkkkkkkkk",
        "kkkkkbkbkkkkkkkk",
        "kkkkkbkbkkkkkkkk",
        "kkkkkbkbkkkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };

    static readonly string[] CapitanMap =
    {
        "kkkkkkkkkkkkkkkk",
        "kkkkkkrrrkkkkkkk",
        "kkkkkrrrrrkkkkkk",
        "kkkkkGrrGkkkkkkk",
        "kkkkkgggggkkkkkk",
        "kkkkkgggggkkkkkk",
        "kkkkkgggggkkkkkk",
        "kkkkbbrrrbbkkkkk",
        "kkkbbbbbbbbbkkkk",
        "kkkkbbbbbbbkkkkk",
        "kkkkkbkbkbkkkkkk",
        "kkkkkbkbkbkkkkkk",
        "kkkkkbkbkbkkkkkk",
        "kkkkkbkbkbkkkkkk",
        "kkkkkkkkkkkkkkkk",
        "kkkkkkkkkkkkkkkk"
    };
}