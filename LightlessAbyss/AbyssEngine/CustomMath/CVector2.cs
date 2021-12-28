using System;
using Microsoft.Xna.Framework;

namespace AbyssEngine.CustomMath
{
    public struct CVector2
    {
        public float x;
        public float y;

        public CVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public CVector2(CVector2Int vec)
        {
            x = vec.x;
            y = vec.y;
        }

        public float Magnitude => MathF.Sqrt(x * x + y * y);
        public CVector2 Normalized => CMath.Approximately(Magnitude, 0f) ? Zero : new CVector2(x, y) / Magnitude;
        
        public static readonly CVector2 Zero = new CVector2(0f, 0f);
        public static readonly CVector2 One = new CVector2(1f, 1f);
        public static readonly CVector2 Up = new CVector2(0f, 1f);
        public static readonly CVector2 Down = new CVector2(0f, -1f);
        public static readonly CVector2 Left = new CVector2(-1f, 0f);
        public static readonly CVector2 Right = new CVector2(1f, 0f);

        public static float Dot(CVector2 a, CVector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static CVector2 Lerp(CVector2 a, CVector2 b, float t)
        {
            return new CVector2(CMath.Lerp(a.x, b.x, t), CMath.Lerp(a.y, b.y, t));
        }

        public static CVector2 operator -(CVector2 v1) => new CVector2(-v1.x, -v1.y);
        
        public static CVector2 operator +(CVector2 v1, CVector2 v2) => new CVector2(v1.x + v2.x, v1.y + v2.y);
        public static CVector2 operator -(CVector2 v1, CVector2 v2) => new CVector2(v1.x - v2.x, v1.y - v2.y);
        public static CVector2 operator *(CVector2 v1, CVector2 v2) => new CVector2(v1.x * v2.x, v1.y * v2.y);
        public static CVector2 operator /(CVector2 v1, CVector2 v2) => new CVector2(v1.x / v2.x, v1.y / v2.y);
        
        public static CVector2 operator +(CVector2 v1, float f1) => new CVector2(v1.x + f1, v1.y + f1);
        public static CVector2 operator -(CVector2 v1, float f1) => new CVector2(v1.x - f1, v1.y - f1);
        public static CVector2 operator *(CVector2 v1, float f1) => new CVector2(v1.x * f1, v1.y * f1);
        public static CVector2 operator /(CVector2 v1, float f1) => new CVector2(v1.x / f1, v1.y / f1);
        
        public static CVector2 operator +(CVector2 v1, CVector2Int v2) => new CVector2(v1.x + v2.x, v1.y + v2.y);
        public static CVector2 operator -(CVector2 v1, CVector2Int v2) => new CVector2(v1.x - v2.x, v1.y - v2.y);
        public static CVector2 operator *(CVector2 v1, CVector2Int v2) => new CVector2(v1.x * v2.x, v1.y * v2.y);
        public static CVector2 operator /(CVector2 v1, CVector2Int v2) => new CVector2(v1.x / v2.x, v1.y / v2.y);

        public static implicit operator Vector2(CVector2 vec) => new Vector2(vec.x, vec.y);
        public static implicit operator CVector2(Vector2 vec) => new CVector2(vec.X, vec.Y);
        public static implicit operator Vector3(CVector2 vec) => new Vector3(vec.x, vec.y, 0f);
        public static implicit operator CVector2(Vector3 vec) => new CVector2(vec.X, vec.Y);
        public static implicit operator CVector2(CVector2Int vec) => new CVector2(vec.x, vec.y);
        public static implicit operator CVector2(Point point) => new CVector2(point.X, point.Y);

        public override string ToString()
        {
            return $"({CMath.RoundToDecimal(x, 2)}, {CMath.RoundToDecimal(y, 2)})";
        }
    }
}