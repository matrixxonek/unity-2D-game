using System.IO;
using UnityEditor;
using UnityEngine;

/// Generuje pixel-artowe placeholdery jako PNG w Assets/Art/Generated i wpina je
/// do Assets/Resources/SpriteLibrary.asset. Spójny styl: płaskie kolory + ciemny obrys.
/// Podmiana na docelową grafikę = podmiana PNG albo przypięcie innych sprite'ów w bibliotece.
public static class ArtGenerator
{
    const string Dir = "Assets/Art/Generated";
    const string LibPath = "Assets/Resources/SpriteLibrary.asset";
    const int PPU = 32;

    // Paleta
    static readonly Color32 Outline = C(0x1A, 0x1D, 0x26);
    static readonly Color32 CaveFill = C(0x2A, 0x2F, 0x3D);
    static readonly Color32 CaveDark = C(0x22, 0x26, 0x32);
    static readonly Color32 Moss = C(0x4E, 0x6B, 0x4E);
    static readonly Color32 MossLight = C(0x7A, 0xA3, 0x62);
    static readonly Color32 SpikeGray = C(0xB8, 0xBE, 0xC8);
    static readonly Color32 SpikeTip = C(0xEE, 0xF2, 0xF8);
    static readonly Color32 Coat = C(0xEE, 0xE6, 0xD0);
    static readonly Color32 CoatShadow = C(0xCF, 0xC5, 0xAA);
    static readonly Color32 Pants = C(0x3A, 0x3F, 0x55);
    static readonly Color32 Skin = C(0xE9, 0xC3, 0xA0);
    static readonly Color32 Hair = C(0xA8, 0xA8, 0xA8);
    static readonly Color32 Cyan = C(0x4D, 0xE1, 0xFF);
    static readonly Color32 CyanDark = C(0x2A, 0x9C, 0xB8);
    static readonly Color32 Frame = C(0x26, 0x26, 0x2C);
    static readonly Color32 Metal = C(0xB0, 0xB6, 0xC2);
    static readonly Color32 MetalDark = C(0x6E, 0x74, 0x82);
    static readonly Color32 Walker = C(0xC9, 0x43, 0x43);
    static readonly Color32 WalkerDark = C(0x7F, 0x26, 0x26);
    static readonly Color32 Flyer = C(0x9D, 0x5B, 0xD6);
    static readonly Color32 Wing = C(0xD9, 0xC2, 0xF2);
    static readonly Color32 Heavy = C(0x8E, 0x2F, 0x2F);
    static readonly Color32 HeavyLight = C(0xB0, 0x4B, 0x45);
    static readonly Color32 HeavyDark = C(0x5A, 0x1A, 0x1A);
    static readonly Color32 BossC = C(0x6B, 0x1F, 0x3A);
    static readonly Color32 BossLight = C(0x9A, 0x33, 0x55);
    static readonly Color32 BossDark = C(0x3E, 0x10, 0x22);
    static readonly Color32 Gold = C(0xD9, 0xA4, 0x41);
    static readonly Color32 Yellow = C(0xFF, 0xD2, 0x4A);
    static readonly Color32 Orange = C(0xFF, 0x8C, 0x42);
    static readonly Color32 Green = C(0x5C, 0xE0, 0x7C);
    static readonly Color32 GreenLight = C(0xC8, 0xFF, 0xD4);
    static readonly Color32 White = C(0xFF, 0xFF, 0xFF);
    static readonly Color32 Dust = C(0xC9, 0xB7, 0x9C);
    static readonly Color32 Crumble = C(0x6A, 0x5E, 0x4A);
    static readonly Color32 CrumbleDark = C(0x3A, 0x33, 0x28);
    static readonly Color32 GateC = C(0x55, 0x60, 0x7A);
    static readonly Color32 GateDark = C(0x2E, 0x34, 0x46);
    static readonly Color32 Wood = C(0xA0, 0x6A, 0x3E);
    static readonly Color32 BgFar = C(0x13, 0x16, 0x21);
    static readonly Color32 BgFarShape = C(0x18, 0x1C, 0x2A);
    static readonly Color32 BgMidShape = C(0x1C, 0x21, 0x30);
    static readonly Color32 BgNearShape = C(0x22, 0x28, 0x3A);
    static readonly Color32 Clear = new Color32(0, 0, 0, 0);

    static Color32 C(int r, int g, int b) => new Color32((byte)r, (byte)g, (byte)b, 255);

    [MenuItem("Doomsday/1. Generate Placeholder Art")]
    public static void Generate()
    {
        Directory.CreateDirectory(Dir);
        Directory.CreateDirectory("Assets/Resources");

        var lib = AssetDatabase.LoadAssetAtPath<SpriteLibrary>(LibPath);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<SpriteLibrary>();
            AssetDatabase.CreateAsset(lib, LibPath);
        }

        lib.player = Save("player", DrawPlayer());
        lib.playerJump = lib.player;
        lib.probe = Save("probe", DrawProbe(), new Vector2(0f, 0.5f));
        lib.muzzle = Save("muzzle", DrawMuzzle());

        lib.tileFill = Save("tile_fill", DrawTileFill());
        lib.tileTop = Save("tile_top", DrawTileTop());
        lib.spikes = Save("spikes", DrawSpikes());
        lib.crumble = Save("crumble", DrawCrumble());
        lib.gate = Save("gate", DrawGate());
        lib.bench = Save("bench", DrawBench());
        lib.sampleNode = Save("sample_node", DrawDiamond(24, 24, Green, GreenLight));
        lib.chargeCrystal = Save("charge_crystal", DrawDiamond(20, 28, Cyan, White));

        lib.walker = Save("walker", DrawWalker());
        lib.flyer = Save("flyer", DrawFlyer());
        lib.heavy = Save("heavy", DrawHeavy());
        lib.boss = Save("boss", DrawBoss());
        lib.shockwave = Save("shockwave", DrawShockwave());

        lib.ammoPistol = Save("ammo_pistol", DrawAmmo(Yellow, C(0xB8, 0x92, 0x2A), false));
        lib.ammoRevolver = Save("ammo_revolver", DrawAmmo(Orange, C(0xB0, 0x55, 0x22), true));
        lib.pickupPistol = Save("pickup_pistol", DrawPistol());
        lib.pickupRevolver = Save("pickup_revolver", DrawRevolver());
        lib.pickupDoubleJump = Save("pickup_double_jump", DrawWings());

        lib.arrow = Save("arrow", DrawArrow());
        lib.bgFar = Save("bg_far", DrawBgFar());
        lib.bgMid = Save("bg_mid", DrawBgLayer(BgMidShape, 7, 140, 3));
        lib.bgNear = Save("bg_near", DrawBgLayer(BgNearShape, 4, 200, 9));

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Doomsday] Placeholder art generated into " + Dir + " and wired into " + LibPath);
    }

    // ---------------------------------------------------------------- Player

    static Px DrawPlayer()
    {
        var p = new Px(24, 32);
        p.Fill(5, 0, 6, 4, Frame); p.Fill(13, 0, 6, 4, Frame);          // buty
        p.Fill(6, 4, 12, 8, Pants);                                     // spodnie
        p.Fill(4, 12, 16, 14, Coat); p.Fill(4, 12, 3, 14, CoatShadow);  // fartuch
        p.Set(12, 15, Frame); p.Set(12, 18, Frame); p.Set(12, 21, Frame);
        p.Fill(2, 13, 3, 9, Coat); p.Fill(19, 13, 3, 9, Coat);          // ręce
        p.Fill(1, 12, 3, 3, Skin); p.Fill(20, 12, 3, 3, Skin);           // dłonie
        p.Fill(7, 24, 10, 8, Skin);                                     // głowa
        p.Fill(6, 30, 12, 2, Hair); p.Fill(5, 27, 2, 4, Hair); p.Fill(17, 27, 2, 4, Hair);
        p.Fill(7, 27, 10, 1, Frame);                                    // pasek gogli
        p.Fill(9, 26, 3, 3, Frame); p.Fill(13, 26, 3, 3, Frame);
        p.Set(10, 27, Cyan); p.Set(14, 27, Cyan);
        p.Fill(10, 24, 4, 1, C(0xC0, 0x90, 0x70));                      // usta
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawProbe()
    {
        var p = new Px(32, 8);
        p.Fill(0, 2, 6, 4, Frame);            // uchwyt
        p.Fill(6, 3, 21, 2, Metal);           // pręt
        p.Fill(27, 2, 5, 4, SpikeTip);        // chwytak
        p.Set(30, 1, SpikeTip); p.Set(30, 6, SpikeTip);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawMuzzle()
    {
        var p = new Px(12, 12);
        p.Fill(4, 0, 4, 12, Yellow); p.Fill(0, 4, 12, 4, Yellow); p.Fill(4, 4, 4, 4, White);
        return p;
    }

    // ----------------------------------------------------------------- World

    static Px DrawTileFill()
    {
        var p = new Px(32, 32);
        p.Fill(0, 0, 32, 32, CaveFill);
        p.Noise(0, 0, 32, 32, CaveDark, 0.12f, 11);
        p.Noise(0, 0, 32, 32, C(0x33, 0x39, 0x48), 0.05f, 12);
        return p;
    }

    static Px DrawTileTop()
    {
        var p = DrawTileFill();
        p.Fill(0, 26, 32, 6, Moss);
        p.Fill(0, 25, 32, 1, C(0x3C, 0x50, 0x3C));
        p.Noise(0, 27, 32, 5, MossLight, 0.18f, 21);
        p.Fill(0, 31, 32, 1, MossLight);
        for (int x = 0; x < 32; x += 7) p.Fill(x + 2, 22, 1, 4, Moss);   // korzonki
        return p;
    }

    static Px DrawSpikes()
    {
        var p = new Px(32, 16);
        p.Fill(0, 0, 32, 2, CaveDark);
        for (int i = 0; i < 4; i++) p.Triangle(i * 8, 2, 8, 13, SpikeGray, SpikeTip);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawCrumble()
    {
        var p = new Px(32, 12);
        p.Fill(0, 0, 32, 12, Crumble);
        p.Fill(0, 10, 32, 2, C(0x8A, 0x7C, 0x62));
        for (int i = 0; i < 10; i++) { p.Set(6 + i, 2 + i % 6, CrumbleDark); p.Set(20 + i / 2, 9 - i % 7, CrumbleDark); }
        p.Fill(14, 0, 1, 7, CrumbleDark); p.Fill(26, 4, 1, 8, CrumbleDark);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawGate()
    {
        var p = new Px(32, 128);
        p.Fill(0, 0, 32, 128, GateC);
        for (int y = 0; y < 128; y += 16) p.Fill(0, y, 32, 3, GateDark);
        p.Fill(14, 0, 4, 128, GateDark);
        for (int y = 6; y < 128; y += 16) { p.Set(4, y, Metal); p.Set(27, y, Metal); }
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawBench()
    {
        var p = new Px(40, 20);
        p.Fill(5, 0, 4, 10, Frame); p.Fill(31, 0, 4, 10, Frame);
        p.Fill(2, 10, 36, 5, Cyan); p.Fill(2, 10, 36, 1, CyanDark);
        p.Fill(2, 15, 36, 3, CyanDark); p.Fill(2, 18, 36, 2, Cyan);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawDiamond(int w, int h, Color32 body, Color32 highlight)
    {
        var p = new Px(w, h);
        int cx = w / 2, cy = h / 2;
        for (int y = 0; y < h; y++)
        {
            float t = 1f - Mathf.Abs(y - cy) / (float)cy;
            int half = Mathf.RoundToInt(t * (w / 2 - 1));
            p.Fill(cx - half, y, half * 2 + 1, 1, body);
        }
        p.Fill(cx - 1, cy + 2, 2, h / 4, highlight);
        p.Set(cx + 2, cy + 1, highlight);
        p.OutlineAll(Outline);
        return p;
    }

    // --------------------------------------------------------------- Enemies

    static Px DrawWalker()
    {
        var p = new Px(28, 24);
        p.Fill(6, 0, 6, 5, WalkerDark); p.Fill(16, 0, 6, 5, WalkerDark);
        p.Ellipse(14, 12, 12, 9, Walker);
        p.Ellipse(14, 10, 10, 5, WalkerDark);
        p.Ellipse(14, 13, 11, 7, Walker);
        p.Fill(15, 12, 6, 5, White); p.Fill(18, 13, 2, 2, Frame);
        p.Fill(8, 13, 4, 4, White); p.Fill(10, 14, 2, 2, Frame);
        p.Fill(12, 8, 8, 2, WalkerDark);                       // usta
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawFlyer()
    {
        var p = new Px(20, 16);
        p.Fill(0, 9, 8, 5, Wing); p.Fill(12, 9, 8, 5, Wing);
        p.Ellipse(10, 7, 6, 5, Flyer);
        p.Fill(11, 6, 5, 5, White); p.Fill(13, 7, 2, 2, Frame);
        p.Set(4, 3, Frame); p.Set(16, 3, Frame);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawHeavy()
    {
        var p = new Px(40, 44);
        p.Fill(9, 0, 8, 9, HeavyDark); p.Fill(23, 0, 8, 9, HeavyDark);
        p.Fill(6, 8, 28, 28, Heavy);
        p.Fill(10, 11, 20, 14, HeavyLight);
        p.Fill(0, 6, 8, 22, Heavy); p.Fill(32, 6, 8, 22, Heavy);
        p.Fill(0, 3, 9, 9, HeavyDark); p.Fill(31, 3, 9, 9, HeavyDark);
        p.Fill(12, 32, 18, 11, Heavy);
        p.Fill(15, 36, 4, 3, Yellow); p.Fill(23, 36, 4, 3, Yellow);
        p.Set(17, 37, Frame); p.Set(25, 37, Frame);
        p.Fill(16, 32, 10, 2, HeavyDark);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawBoss()
    {
        var p = new Px(72, 80);
        p.Fill(16, 0, 14, 14, BossDark); p.Fill(42, 0, 14, 14, BossDark);
        p.Fill(10, 12, 52, 46, BossC);
        p.Fill(18, 18, 36, 28, BossLight);
        p.Fill(0, 10, 12, 36, BossC); p.Fill(60, 10, 12, 36, BossC);
        p.Fill(0, 4, 14, 14, BossDark); p.Fill(58, 4, 14, 14, BossDark);
        p.Fill(20, 56, 32, 18, BossC);
        p.Fill(26, 62, 7, 5, Yellow); p.Fill(40, 62, 7, 5, Yellow);
        p.Fill(29, 63, 3, 3, Frame); p.Fill(43, 63, 3, 3, Frame);
        for (int x = 27; x < 45; x += 4) p.Fill(x, 56, 2, 3, White);
        for (int i = 0; i < 4; i++) p.Triangle(22 + i * 8, 72, 8, 8, Gold, Yellow);
        p.Fill(30, 24, 12, 3, BossDark); p.Fill(26, 30, 6, 3, BossDark); p.Fill(40, 30, 6, 3, BossDark);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawShockwave()
    {
        var p = new Px(24, 12);
        p.Ellipse(12, 4, 11, 4, Dust);
        p.Ellipse(12, 6, 7, 4, C(0xE0, 0xD2, 0xB8));
        p.OutlineAll(new Color32(0x6A, 0x5E, 0x4A, 180));
        return p;
    }

    // --------------------------------------------------------------- Pickups

    static Px DrawAmmo(Color32 body, Color32 band, bool dot)
    {
        var p = new Px(14, 10);
        p.Fill(0, 0, 14, 10, body);
        p.Fill(0, 4, 14, 2, band);
        if (dot) p.Fill(5, 2, 4, 6, Frame);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawPistol()
    {
        var p = new Px(24, 14);
        p.Fill(4, 6, 10, 7, Metal); p.Fill(12, 8, 12, 4, Metal);
        p.Fill(5, 0, 6, 8, Frame); p.Fill(10, 3, 3, 3, MetalDark);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawRevolver()
    {
        var p = new Px(26, 16);
        p.Fill(10, 9, 16, 4, Metal); p.Fill(7, 6, 9, 9, MetalDark);
        p.Set(10, 10, Metal); p.Set(13, 10, Metal);
        p.Fill(2, 0, 7, 10, Wood); p.Fill(8, 4, 4, 3, Frame);
        p.OutlineAll(Outline);
        return p;
    }

    static Px DrawWings()
    {
        var p = new Px(24, 24);
        for (int i = 0; i < 9; i++) { p.Fill(2 + i, 6 + i, 1, 9 - i, Cyan); p.Fill(21 - i, 6 + i, 1, 9 - i, Cyan); }
        p.Fill(9, 8, 6, 9, White); p.Fill(10, 4, 4, 5, Cyan);
        p.OutlineAll(Outline);
        return p;
    }

    // ------------------------------------------------------------------- UI

    static Px DrawArrow()
    {
        var p = new Px(32, 32);
        p.Triangle(2, 8, 28, 22, White, White);
        p.Fill(12, 2, 8, 8, White);
        return p;
    }

    // ------------------------------------------------------------ Backgrounds

    static Px DrawBgFar()
    {
        var p = new Px(512, 256);
        p.Fill(0, 0, 512, 256, BgFar);
        var rng = new System.Random(5);
        for (int i = 0; i < 14; i++)
        {
            int w = rng.Next(40, 120), h = rng.Next(60, 200), x = rng.Next(0, 512);
            p.FillWrap(x, 0, w, h, BgFarShape);
        }
        for (int i = 0; i < 18; i++)
        {
            int w = rng.Next(12, 40), h = rng.Next(30, 110), x = rng.Next(0, 512);
            p.FillWrap(x, 256 - h, w, h, BgFarShape);
        }
        return p;
    }

    static Px DrawBgLayer(Color32 shape, int pillars, int maxHeight, int seed)
    {
        var p = new Px(512, 256);
        var rng = new System.Random(seed);
        for (int i = 0; i < pillars; i++)
        {
            int w = rng.Next(18, 50), h = rng.Next(maxHeight / 2, maxHeight), x = rng.Next(0, 512);
            p.FillWrap(x, 0, w, h, shape);
            p.FillWrap(x - 6, 0, w + 12, h / 4, shape);
        }
        for (int i = 0; i < pillars; i++)
        {
            int w = rng.Next(10, 30), h = rng.Next(20, 90), x = rng.Next(0, 512);
            p.FillWrap(x, 256 - h, w, h, shape);
        }
        return p;
    }

    // ------------------------------------------------------------------ IO

    static Sprite Save(string name, Px px, Vector2? pivot = null)
    {
        string path = $"{Dir}/{name}.png";
        File.WriteAllBytes(path, px.ToTexture().EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PPU;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Repeat;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = pivot.HasValue ? (int)SpriteAlignment.Custom : (int)SpriteAlignment.Center;
        if (pivot.HasValue) settings.spritePivot = pivot.Value;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// Bufor pikseli z y = 0 na dole (jak Texture2D).
    class Px
    {
        readonly int w, h;
        readonly Color32[] p;

        public Px(int w, int h)
        {
            this.w = w; this.h = h;
            p = new Color32[w * h];
            for (int i = 0; i < p.Length; i++) p[i] = Clear;
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            p[y * w + x] = c;
        }

        Color32 Get(int x, int y) => (x < 0 || y < 0 || x >= w || y >= h) ? Clear : p[y * w + x];

        public void Fill(int x, int y, int fw, int fh, Color32 c)
        {
            for (int yy = y; yy < y + fh; yy++)
                for (int xx = x; xx < x + fw; xx++) Set(xx, yy, c);
        }

        public void FillWrap(int x, int y, int fw, int fh, Color32 c)
        {
            for (int yy = y; yy < y + fh; yy++)
                for (int xx = x; xx < x + fw; xx++) Set(((xx % w) + w) % w, yy, c);
        }

        public void Ellipse(int cx, int cy, int rx, int ry, Color32 c)
        {
            for (int yy = cy - ry; yy <= cy + ry; yy++)
                for (int xx = cx - rx; xx <= cx + rx; xx++)
                {
                    float dx = (xx - cx) / (float)rx, dy = (yy - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f) Set(xx, yy, c);
                }
        }

        public void Triangle(int x, int y, int baseW, int height, Color32 c, Color32 tip)
        {
            for (int row = 0; row < height; row++)
            {
                float t = row / (float)height;
                int half = Mathf.RoundToInt((1f - t) * baseW * 0.5f);
                int cx = x + baseW / 2;
                Fill(cx - half, y + row, Mathf.Max(1, half * 2), 1, row >= height - 3 ? tip : c);
            }
        }

        public void Noise(int x, int y, int fw, int fh, Color32 c, float density, int seed)
        {
            var rng = new System.Random(seed);
            for (int yy = y; yy < y + fh; yy++)
                for (int xx = x; xx < x + fw; xx++)
                    if (rng.NextDouble() < density) Set(xx, yy, c);
        }

        /// Ciemny obrys wokół wszystkich nieprzezroczystych pikseli.
        public void OutlineAll(Color32 c)
        {
            var src = (Color32[])p.Clone();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (src[y * w + x].a != 0) continue;
                    bool near = A(src, x + 1, y) || A(src, x - 1, y) || A(src, x, y + 1) || A(src, x, y - 1);
                    if (near) p[y * w + x] = c;
                }
        }

        bool A(Color32[] src, int x, int y) => x >= 0 && y >= 0 && x < w && y < h && src[y * w + x].a != 0;

        public Texture2D ToTexture()
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(p);
            tex.Apply();
            return tex;
        }
    }
}
