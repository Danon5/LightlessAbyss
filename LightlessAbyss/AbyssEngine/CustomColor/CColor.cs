using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;

namespace AbyssEngine.CustomColor
{
    public struct CColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public CColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        
        public CColor(double r, double g, double b, double a)
        {
            this.r = (float)r;
            this.g = (float)g;
            this.b = (float)b;
            this.a = (float)a;
        }
        
        public CColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            a = 1f;
        }
        
        public static readonly CColor White = new CColor(1f, 1f, 1f, 1f);
        public static readonly CColor Black = new CColor(0f, 0f, 0f, 1f);
        public static readonly CColor Clear = new CColor(0f, 0f, 0f, 0f);
        public static readonly CColor Red = new CColor(1f, 0f, 0f, 1f);
        public static readonly CColor Orange = new CColor(1f, .5f, 0f, 1f);
        public static readonly CColor Yellow = new CColor(1f, 1f, 0f, 1f);
        public static readonly CColor Green = new CColor(0f, 1f, 0f, 1f);
        public static readonly CColor Blue = new CColor(0f, 0f, 1f, 1f);
        public static readonly CColor Purple = new CColor(.5f, 0f, 1f, 1f);
        public static readonly CColor Pink = new CColor(1f, 0f, 1f, 1f);

        public static void RGBtoHSV(CColor rgb, out float h, out float s, out float v)
        {
            float r = rgb.r, g = rgb.g, b = rgb.b;
            float max = CMath.Max(r, g, b);
            float min = CMath.Min(r, g, b);
            float delta = max - min;

            h = 0f;
            
            if (max != min)
            {
                if (max == r)
                    h = (g - b) / delta + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / delta + 2f;
                else if (max == b)
                    h = (r - g) / delta + 4f;
            }

            h /= 6f;
            s = max == 0f ? 0f : delta / max;
            v = max;
        }

        public static CColor HSVtoRGB(float h, float s, float v, float a = 1f)
        {
            double r = 0f, g = 0f, b = 0f;

            int i = (int)(h * 6f);
            double f = h * 6f - i;
            double p = v * (1f - s);
            double q = v * (1f - f * s);
            double t = v * (1f - (1f - f) * s);

            switch (i % 6)
            {
                case 0: r = v; g = t; b = p;
                    break;
                case 1: r = q; g = v; b = p;
                    break;
                case 2: r = p; g = v; b = t;
                    break;
                case 3: r = p; g = q; b = v;
                    break;
                case 4: r = t; g = p; b = v;
                    break;
                case 5: r = v; g = p; b = q;
                    break;
            }
            
            return new CColor(r, g, b, a);
        }

        public override string ToString()
        {
            return $"[{r}, {g}, {b}, {a}]";
        }
        
        public static CColor operator +(CColor c1, CColor c2) => 
            new CColor(c1.r + c2.r, c1.g + c2.g, c1.b + c2.b, c1.a + c2.a);
        public static CColor operator -(CColor c1, CColor c2) => 
            new CColor(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b, c1.a - c2.a);
        public static CColor operator *(CColor c1, CColor c2) => 
            new CColor(c1.r * c2.r, c1.g * c2.g, c1.b * c2.b, c1.a * c2.a);
        public static CColor operator /(CColor c1, CColor c2) => 
            new CColor(c1.r / c2.r, c1.g / c2.g, c1.b / c2.b, c1.a / c2.a);
        
        public static CColor operator +(CColor c1, float f1) => 
            new CColor(c1.r + f1, c1.g + f1, c1.b + f1, c1.a + f1);
        public static CColor operator -(CColor c1, float f1) => 
            new CColor(c1.r - f1, c1.g - f1, c1.b - f1, c1.a - f1);
        public static CColor operator *(CColor c1, float f1) => 
            new CColor(c1.r * f1, c1.g * f1, c1.b * f1, c1.a * f1);
        public static CColor operator /(CColor c1, float f1) => 
            new CColor(c1.r / f1, c1.g / f1, c1.b / f1, c1.a / f1);
        
        public static implicit operator Color(CColor col) => new Color(col.r, col.g, col.b, col.a);
        public static implicit operator CColor(Color col) => RoundColor(new CColor(col.R, col.G, col.B, col.A) / 255f);

        private static CColor RoundColor(CColor col)
        {
            return new CColor(
                CMath.RoundToDecimal(col.r, 3),
                CMath.RoundToDecimal(col.g, 3),
                CMath.RoundToDecimal(col.b, 3),
                CMath.RoundToDecimal(col.a, 3));
        }
    }
} 