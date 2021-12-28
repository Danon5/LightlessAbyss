using System;
using Microsoft.Xna.Framework;

namespace LightlessAbyss.AbyssEngine.CustomMath
{
    public struct CVector2Int
    {
        public int x;
        public int y;

        public CVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public float Magnitude => MathF.Sqrt(x * x + y * y);

        public static readonly CVector2Int Zero = new CVector2Int(0, 0);
        public static readonly CVector2Int One = new CVector2Int(1, 1);
        public static readonly CVector2Int Up = new CVector2Int(0, 1);
        public static readonly CVector2Int Down = new CVector2Int(0, -1);
        public static readonly CVector2Int Left = new CVector2Int(-1, 0);
        public static readonly CVector2Int Right = new CVector2Int(1, 0);
        
        public static CVector2Int operator +(CVector2Int v1, CVector2Int v2) => new CVector2Int(v1.x + v2.x, v1.y + v2.y);
        public static CVector2Int operator -(CVector2Int v1, CVector2Int v2) => new CVector2Int(v1.x - v2.x, v1.y - v2.y);
        public static CVector2Int operator *(CVector2Int v1, CVector2Int v2) => new CVector2Int(v1.x * v2.x, v1.y * v2.y);
        public static CVector2Int operator /(CVector2Int v1, CVector2Int v2) => new CVector2Int(v1.x / v2.x, v1.y / v2.y);
        
        public static CVector2Int operator +(CVector2Int v1, int f1) => new CVector2Int(v1.x + f1, v1.y + f1);
        public static CVector2Int operator -(CVector2Int v1, int f1) => new CVector2Int(v1.x - f1, v1.y - f1);
        public static CVector2Int operator *(CVector2Int v1, int f1) => new CVector2Int(v1.x * f1, v1.y * f1);
        public static CVector2Int operator /(CVector2Int v1, int f1) => new CVector2Int(v1.x / f1, v1.y / f1);
        public static explicit operator CVector2Int(CVector2 vec) => new CVector2Int((int)vec.x, (int)vec.y);
        public static implicit operator CVector2Int(Point point) => new CVector2Int(point.X, point.Y);

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}