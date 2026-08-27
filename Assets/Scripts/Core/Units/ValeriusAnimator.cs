using UnityEngine;
using System.Collections.Generic;

// 1.7-arte: animador de Valerius con deteccion automatica de sprites por transparencia.
// Filas (de arriba a abajo): 0=Idle, 1=Caminar, 2=Ataque, 3=Recibir da駉, 4=Muerte.
public class ValeriusAnimator : MonoBehaviour
{
    // Ajusta el tamano en el mundo (mas bajo = personaje mas grande)
    public float pixelsPerUnit = 130f;

    private const int ROW_IDLE = 0;
    private const int ROW_WALK = 1;
    private const int ROW_ATTACK = 2;
    private const int ROW_HIT = 3;
    private const int ROW_DEATH = 4;

    // 1.7-arte fix7: FPS subidos para mas fluidez (idle/caminar/ataque/dano/muerte)
    private readonly float[] fps  = { 8f, 12f, 18f, 10f, 8f };
    private readonly bool[] loopA = { true, true, false, false, false };

    private SpriteRenderer sr;
    private readonly List<Sprite>[] rows = new List<Sprite>[5];
    private int currentRow = ROW_IDLE;
    private bool oneShot = false;
    private int oneShotRow = -1;
    private int frameIndex = 0;
    private float frameTimer = 0f;
    private Vector3 prevPos;
    private float baseScale = 1f;
    private bool flipX = false;
    private bool ready = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        baseScale = Mathf.Abs(transform.localScale.x);
        LoadSprites();
        prevPos = transform.position;
        if (ready) { currentRow = ROW_IDLE; frameIndex = 0; }
    }

    // --- Carga y deteccion automatica ---
    void LoadSprites()
    {
        Texture2D src = Resources.Load<Texture2D>("Sprites/valerius");
        if (src == null) { ready = false; return; }
        src.filterMode = FilterMode.Point;

        Color[] px = ReadPixelsCopy(src);
        if (px == null) { ready = false; return; }

        int w = src.width, h = src.height;
        bool[] occ = new bool[w * h];
        for (int i = 0; i < px.Length; i++) occ[i] = px[i].a > 0.35f;

        // Bandas de filas (de arriba hacia abajo)
        List<Rect> bands = new List<Rect>();
        List<List<Rect>> bandBoxes = new List<List<Rect>>();
        int y = h - 1;
        while (y >= 0)
        {
            while (y >= 0 && !LineHas(occ, w, y)) y--;
            if (y < 0) break;
            int yTop = y, yBottom = y, gap = 0;
            while (y >= 0)
            {
                if (LineHas(occ, w, y)) { yBottom = y; gap = 0; }
                else { gap++; if (gap > 8) break; }
                y--;
            }

            // 1.7-arte fix6: separar por DENSIDAD de columnas (los cuerpos tienen muchos
            // pixeles; las espadas/sangre pocos). Evita que la fila de ataque se fusione.
            int[] colDensity = new int[w];
            int bandMinX = w, bandMaxX = -1;
            for (int x = 0; x < w; x++)
            {
                int c = 0;
                for (int yy = yBottom; yy <= yTop; yy++)
                    if (occ[yy * w + x]) c++;
                colDensity[x] = c;
                if (c > 0) { if (x < bandMinX) bandMinX = x; if (x > bandMaxX) bandMaxX = x; }
            }
            int maxDen = 0;
            for (int x = 0; x < w; x++) maxDen = Mathf.Max(maxDen, colDensity[x]);
            int thr = Mathf.Max(20, maxDen / 4);

            // Nucleos = cuerpos del caballero
            List<int[]> cores = new List<int[]>();
            int cx = 0;
            while (cx < w)
            {
                while (cx < w && colDensity[cx] <= thr) cx++;
                if (cx >= w) break;
                int c0 = cx, c1 = cx, gapX = 0;
                while (cx < w)
                {
                    if (colDensity[cx] > thr) { c1 = cx; gapX = 0; }
                    else { gapX++; if (gapX > 4) break; }
                    cx++;
                }
                if (c1 - c0 >= 16) cores.Add(new int[] { c0, c1 });
            }

            // Rects con frontera en el punto medio entre cuerpos vecinos
            List<Rect> boxes = new List<Rect>();
            for (int i = 0; i < cores.Count; i++)
            {
                int x0 = (i == 0) ? bandMinX : (cores[i - 1][1] + cores[i][0]) / 2;
                int x1 = (i == cores.Count - 1) ? bandMaxX : (cores[i][1] + cores[i + 1][0]) / 2;
                Rect r = new Rect(x0, yBottom, x1 - x0 + 1, yTop - yBottom + 1);
                if (r.width >= 24 && r.height >= 32) boxes.Add(r);
            }

            if (boxes.Count > 0)
            {
                // Filtro: descarta pedazos sueltos (ej. la espada sola) comparando alturas
                float maxH = 0;
                foreach (Rect r in boxes) maxH = Mathf.Max(maxH, r.height);
                List<Rect> clean = new List<Rect>();
                foreach (Rect r in boxes)
                    if (r.height >= maxH * 0.35f) clean.Add(r);
                if (clean.Count > 0) { bands.Add(new Rect(0, yBottom, 1, yTop - yBottom)); bandBoxes.Add(clean); }
            }
        }

        // Construir sprites por fila (maximo 5 filas)
        int rowCount = Mathf.Min(bandBoxes.Count, 5);
        for (int r = 0; r < rowCount; r++)
        {
            List<Rect> bs = bandBoxes[r];
            bs.Sort((a, b) => a.x.CompareTo(b.x));
            rows[r] = new List<Sprite>();
            foreach (Rect rect in bs)
            {
                // 1.7-arte fix5: pivot anclado al CUERPO (densidad de columnas) para que
                // el caballero NO se deslice cuando el espadon/sangre ensancha el frame
                float pivotX = ComputeBodyPivotX(px, w, rect);
                rows[r].Add(Sprite.Create(src, rect, new Vector2(pivotX, 0f), pixelsPerUnit));
            }
        }

        ready = rows[ROW_IDLE] != null && rows[ROW_IDLE].Count > 0;
        Debug.Log("[ValeriusAnimator] Filas detectadas: " + rowCount +
                  " | frames: " + Frames(0) + "/" + Frames(1) + "/" + Frames(2) + "/" + Frames(3) + "/" + Frames(4));
    }

    int Frames(int r) { return rows[r] != null ? rows[r].Count : 0; }
        // Calcula la X del "cuerpo" del caballero: las columnas con mas pixeles opacos
    // (el torso domina sobre la espada/sangre, que son delgadas)
    static float ComputeBodyPivotX(Color[] px, int texWidth, Rect rect)
    {
        int x0 = Mathf.RoundToInt(rect.x);
        int x1 = Mathf.RoundToInt(rect.x + rect.width);
        int y0 = Mathf.RoundToInt(rect.y);
        int y1 = Mathf.RoundToInt(rect.y + rect.height);

        float sum = 0f, weighted = 0f;
        for (int x = x0; x < x1; x++)
        {
            float colCount = 0f;
            for (int yy = y0; yy < y1; yy++)
            {
                if (px[yy * texWidth + x].a > 0.35f) colCount++;
            }
            sum += colCount;
            weighted += colCount * x;
        }

        if (sum <= 0f) return 0.5f;
        float bodyX = weighted / sum;
        float pivotX = (bodyX - rect.x) / rect.width;
        return Mathf.Clamp(pivotX, 0.15f, 0.85f);
    }
    static bool LineHas(bool[] occ, int w, int y)
    {
        int baseI = y * w;
        for (int x = 0; x < w; x++) if (occ[baseI + x]) return true;
        return false;
    }

    static bool ColHas(bool[] occ, int w, int x, int y0, int y1)
    {
        for (int yy = y0; yy <= y1; yy++) if (occ[yy * w + x]) return true;
        return false;
    }

    static Color[] ReadPixelsCopy(Texture2D src)
    {
        // 1.7-arte fix4: capturar active ANTES del Blit (Blit lo cambia) para evitar el warning
        RenderTexture prev = RenderTexture.active;
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        Color[] px = tex.GetPixels();
        Destroy(tex);
        return px;
    }

    // --- API publica ---
    public void PlayOneShot(string anim)
    {
        if (!ready) return;
        int r = -1;
        if (anim == "attack") r = ROW_ATTACK;
        else if (anim == "hit") r = ROW_HIT;
        else if (anim == "death") r = ROW_DEATH;
        if (r < 0 || Frames(r) == 0) return;
        oneShot = true;
        oneShotRow = r;
        frameIndex = 0;
        frameTimer = 0f;
    }

    // --- Bucle de animacion ---
    void LateUpdate()
    {
        if (!ready) return;

        Vector3 delta = transform.position - prevPos;
        bool moved = delta.sqrMagnitude > 0.00001f;
        prevPos = transform.position;

        Unit u = GetComponent<Unit>();
        if (u != null)
        {
            if (u.facing.x < -0.001f) flipX = true;
            else if (u.facing.x > 0.001f) flipX = false;
        }
        else if (moved)
        {
            if (delta.x < -0.001f) flipX = true;
            else if (delta.x > 0.001f) flipX = false;
        }

        int target;
        if (oneShot)
        {
            target = oneShotRow;
            if (frameIndex >= Frames(oneShotRow))
            {
                oneShot = false;
                frameIndex = 0;
                frameTimer = 0f;
                target = moved ? ROW_WALK : ROW_IDLE;
            }
        }
        else
        {
            target = moved ? ROW_WALK : ROW_IDLE;
        }

        if (target != currentRow) { currentRow = target; frameIndex = 0; frameTimer = 0f; }

        // Avanzar frame
        int count = Frames(currentRow);
        if (count > 0)
        {
            frameTimer += Time.deltaTime;
            float step = 1f / fps[currentRow];
            while (frameTimer >= step)
            {
                frameTimer -= step;
                frameIndex++;
                if (loopA[currentRow]) frameIndex %= count;
                else if (frameIndex > count) frameIndex = count;
            }
        }

        // Aplicar sprite + volteo
        int idx = Mathf.Min(frameIndex, count - 1);
        Sprite s = rows[currentRow][idx];
        if (s != null && sr.sprite != s) sr.sprite = s;

        float desired = baseScale * (flipX ? -1f : 1f);
        Vector3 ls = transform.localScale;
        if (Mathf.Sign(ls.x) != Mathf.Sign(desired))
            transform.localScale = new Vector3(desired, Mathf.Abs(ls.y), ls.z);
    }
}