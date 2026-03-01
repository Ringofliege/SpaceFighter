using UnityEngine;

namespace SpaceFighter
{
    public static class ProceduralSpriteGenerator
    {
        private static readonly Color BlueTeam = new Color(0.2f, 0.4f, 1f);
        private static readonly Color RedTeam = new Color(1f, 0.3f, 0.2f);

        public static Sprite GenerateShipSprite(ShipClass cls, Team team)
        {
            Color baseColor = team == Team.Blue ? BlueTeam : RedTeam;
            Color outline = baseColor * 0.5f;
            outline.a = 1f;

            switch (cls)
            {
                case ShipClass.Vanguard: return GenerateVanguard(baseColor, outline);
                case ShipClass.Striker:  return GenerateStriker(baseColor, outline);
                case ShipClass.Disruptor: return GenerateDisruptor(baseColor, outline);
                case ShipClass.Flanker:  return GenerateFlanker(baseColor, outline);
                default: return GenerateStriker(baseColor, outline);
            }
        }

        private static Sprite GenerateVanguard(Color fill, Color outline)
        {
            int s = 32;
            var tex = CreateTexture(s, s);
            float cx = s / 2f, cy = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = Mathf.Abs(x - cx + 0.5f);
                float dy = Mathf.Abs(y - cy + 0.5f);
                float manhattan = dx / cx + dy / cy;
                if (manhattan <= 1f)
                {
                    tex.SetPixel(x, y, manhattan > 0.85f ? outline : fill);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        private static Sprite GenerateStriker(Color fill, Color outline)
        {
            int s = 24;
            var tex = CreateTexture(s, s);

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x - s / 2f) / (s / 2f);
                float ny = (float)y / s;
                float halfWidth = ny * 0.9f;

                if (Mathf.Abs(nx) <= halfWidth && ny > 0.05f)
                {
                    bool border = Mathf.Abs(nx) > halfWidth - 0.15f || ny < 0.15f || ny > 0.92f;
                    tex.SetPixel(x, y, border ? outline : fill);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        private static Sprite GenerateDisruptor(Color fill, Color outline)
        {
            int s = 24;
            var tex = CreateTexture(s, s);
            float cx = s / 2f, cy = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = Mathf.Abs(x - cx + 0.5f);
                float dy = Mathf.Abs(y - cy + 0.5f);
                float manhattan = dx + dy;
                float limit = s * 0.4f;

                if (manhattan <= limit)
                {
                    bool border = manhattan > limit - 2f;
                    bool cross = (dx < 1.5f || dy < 1.5f) && manhattan < limit - 2f;
                    tex.SetPixel(x, y, border || cross ? outline : fill);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        private static Sprite GenerateFlanker(Color fill, Color outline)
        {
            int s = 20;
            var tex = CreateTexture(s, s);

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x - s / 2f) / (s / 2f);
                float ny = (float)y / s;
                float halfWidth = ny * 0.55f;

                if (Mathf.Abs(nx) <= halfWidth && ny > 0.08f)
                {
                    bool border = Mathf.Abs(nx) > halfWidth - 0.18f || ny < 0.18f || ny > 0.9f;
                    tex.SetPixel(x, y, border ? outline : fill);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateProjectileSprite(Team team)
        {
            int s = 8;
            var tex = CreateTexture(s, s);
            Color c = team == Team.Blue ? new Color(0.5f, 0.7f, 1f) : new Color(1f, 0.6f, 0.5f);
            float cx = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));
                if (d < cx)
                {
                    Color col = c;
                    col.a = 1f - (d / cx) * 0.4f;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateRicochetSprite(Team team)
        {
            int s = 10;
            var tex = CreateTexture(s, s);
            Color c = team == Team.Blue ? new Color(0.6f, 0.8f, 1f) : new Color(1f, 0.7f, 0.4f);
            float cx = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = Mathf.Abs(x - cx + 0.5f);
                float dy = Mathf.Abs(y - cx + 0.5f);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));

                bool star = d < cx * 0.5f || dx < 1f || dy < 1f;
                if (star && d < cx)
                    tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GeneratePierceSprite()
        {
            int w = 12, h = 6;
            var tex = CreateTexture(w, h);
            Color c = new Color(0.7f, 1f, 1f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float ny = Mathf.Abs(y - h / 2f + 0.5f) / (h / 2f);
                float nx = (float)x / w;
                float taper = 1f - nx * 0.4f;

                if (ny < taper)
                {
                    Color col = c;
                    col.a = 1f - ny * 0.3f;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateMineSprite(Team team)
        {
            int s = 12;
            var tex = CreateTexture(s, s);
            Color fill = team == Team.Blue ? new Color(0.3f, 0.5f, 1f) : new Color(1f, 0.4f, 0.3f);
            Color ring = Color.white * 0.8f;
            ring.a = 1f;
            float cx = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));
                if (d < cx && d > cx - 2f)
                    tex.SetPixel(x, y, ring);
                else if (d < cx - 2f)
                    tex.SetPixel(x, y, fill);
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateCoreSprite()
        {
            int s = 32;
            var tex = CreateTexture(s, s);
            Color ring = new Color(1f, 0.9f, 0.3f);
            float cx = s / 2f;

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));
                if (d < cx && d > cx - 3f)
                    tex.SetPixel(x, y, ring);
            }

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateWallSprite()
        {
            int s = 4;
            var tex = CreateTexture(s, s);
            Color c = new Color(0.5f, 0.5f, 0.5f);

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, c);

            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite GenerateAsteroidSprite()
        {
            int s = 16;
            var tex = CreateTexture(s, s);
            Color c = new Color(0.35f, 0.32f, 0.3f);
            float cx = s / 2f;

            // Irregular circle using fixed offsets per angle
            float[] radii = { 0.85f, 0.9f, 0.75f, 0.95f, 0.8f, 0.88f, 0.72f, 0.92f };

            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx + 0.5f;
                float dy = y - cx + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                if (angle < 0) angle += Mathf.PI * 2f;

                float sector = angle / (Mathf.PI * 2f) * radii.Length;
                int i0 = Mathf.FloorToInt(sector) % radii.Length;
                int i1 = (i0 + 1) % radii.Length;
                float t = sector - Mathf.Floor(sector);
                float r = Mathf.Lerp(radii[i0], radii[i1], t) * cx;

                if (d < r)
                {
                    float shade = 0.9f + 0.1f * (d / r);
                    tex.SetPixel(x, y, c * shade);
                    Color px = tex.GetPixel(x, y);
                    px.a = 1f;
                    tex.SetPixel(x, y, px);
                }
            }

            tex.Apply();
            return ToSprite(tex);
        }

        private static Texture2D CreateTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);
            return tex;
        }

        private static Sprite ToSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
        }
    }
}
