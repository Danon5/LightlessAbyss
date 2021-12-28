using System;
using Microsoft.Xna.Framework;

namespace LightlessAbyss.AbyssEngine.CustomMath
{
    public static class CMathUtils
    {
        public const float EPSILON = 1.175494E-38f;
        public const float RAD2DEG = 180f / MathF.PI;
        public const float DEG2RAD = MathF.PI / 180f;

        public static CVector2 ToVec2(this CVector2Int vec)
        {
            return new CVector2(vec.x, vec.y);
        }
        
        public static CVector2Int ToVec2Int(this CVector2 vec)
        {
            return new CVector2Int((int)vec.x, (int)vec.y);
        }
        
        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            float fromAbs = from - fromMin;
            float fromMaxAbs = fromMax - fromMin;

            float normal = fromAbs / fromMaxAbs;

            float toMaxAbs = toMax - toMin;
            float toAbs = toMaxAbs * normal;

            float to = toAbs + toMin;

            return to;
        }
        
        public static float Remap01(this float from, float fromMin, float fromMax)
        {
            return Remap(from, fromMin, fromMax, 0f, 1f);
        }

        public static float Lerp(float a, float b, float t)
        {
            return (1f - t) * a + t * b;
        }

        public static float InverseLerp(float a, float b, float t)
        {
            return (t - a) * (b - a);
        }
        
        public static float RoundToNumber(float value, float number)
        {
            return RoundToInt(value / number) * number;
        }
        
        public static CVector2 SnapToPPU(CVector2 unsnappedPos, int ppu)
        {
            float ppuSnap = 1f / ppu;
            
            CVector2 pos = unsnappedPos;
            pos.x = MathF.Round(pos.x / ppuSnap) * ppuSnap;
            pos.y = MathF.Round(pos.y / ppuSnap) * ppuSnap;
            
            return pos;
        }
        
        public static float RoundToDecimal(float value, int decimalPlace)
        {
            float multiplier = MathF.Pow(10, decimalPlace);

            value *= multiplier;
            value = MathF.Round(value);
            value /= multiplier;

            return value;
        }
        
        public static float CeilToDecimal(float value, int decimalPlace)
        {
            float multiplier = MathF.Pow(10, decimalPlace);

            value *= multiplier;
            value = MathF.Ceiling(value);
            value /= multiplier;

            return value;
        }

        public static float RoundTowardsZeroToDecimal(float value, int decimalPlace)
        {
            float multiplier = MathF.Pow(10, decimalPlace);

            value *= multiplier;
            value = value < 0 ? MathF.Ceiling(value) : MathF.Floor(value);
            value /= multiplier;

            return value;
        }

        public static float FloorToDecimal(float value, int decimalPlace)
        {
            float multiplier = MathF.Pow(10, decimalPlace);

            value *= multiplier;
            value = MathF.Floor(value);
            value /= multiplier;

            return value;
        }
        
        public static int RoundToInt(float val) => (int)MathF.Round(val);
        public static int FloorToInt(float val) => (int) MathF.Floor(val);

        public static CVector2Int RoundToVec2Int(this CVector2 vec)
        {
            return new CVector2Int(RoundToInt(vec.x), RoundToInt(vec.y));
        }
        
        public static CVector2Int FloorToVec2Int(this CVector2 vec)
        {
            return new CVector2Int(FloorToInt(vec.x), FloorToInt(vec.y));
        }
        
        public static float DirectionToDegAngle(CVector2 dir)
        {
            return NormalizeAngle(MathF.Atan2(dir.y, dir.x) * RAD2DEG);
        }
        
        public static float DirectionToRadAngle(CVector2 dir)
        {
            return NormalizeAngle(MathF.Atan2(dir.y, dir.x));
        }
        
        public static CVector2 AngleToDirection(float angle)
        {
            return new CVector2(MathF.Cos(angle * RAD2DEG), MathF.Sin(angle * DEG2RAD));
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static CVector2 Clamp(CVector2 val, CVector2 min, CVector2 max)
        {
            return new CVector2(Clamp(val.x, min.x, max.x), Clamp(val.y, min.y, max.y));
        }

        public static float Clamp01(float val)
        {
            return Clamp(val, 0f, 1f);
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle < 0f)
                angle += 360f;

            return angle % 360f;
        }

        public static bool Approximately(float a, float b)
        {
            return (double) Math.Abs(b - a) < Math.Max(1E-06f * Math.Max(Math.Abs(a), Math.Abs(b)), EPSILON * 8f);
        }

        public static Matrix MatrixTRS(CVector2 translation, float rotation, CVector2 scale)
        {
            Matrix translationMatrix = Matrix.CreateTranslation(translation);
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotation * DEG2RAD);
            Matrix scaleMatrix = Matrix.CreateScale(new Vector3(scale.x, scale.y, 1f));
            
            return translationMatrix * rotationMatrix * scaleMatrix;
        }
        
        public static Matrix MatrixTRS(CVector2 translation, float rotation, float scale)
        {
            return MatrixTRS(translation, rotation, new CVector2(scale, scale));
        }
        
        public static float GetZDegrees(this Matrix matrix)
        {
            return (float)Math.Atan2(matrix.M21, matrix.M11);
        }

        public static Matrix MatrixFromDegreesZ(float z)
        {
            return Matrix.CreateRotationZ(z * DEG2RAD);
        }

        public static void Decompose2D(this Matrix matrix, out CVector2 pos, out float rot, out CVector2 scale)
        {
            matrix.Decompose(out Vector3 mScale, out Quaternion mRot, out Vector3 mPos);
            pos = mPos;
            rot = matrix.GetZDegrees();
            scale = new CVector2(mScale.X, mScale.Y);
        }
        
        public static CVector2 MultiplyPoint(this Matrix m, CVector2 v)
        {
            return Microsoft.Xna.Framework.Vector2.Transform(v, m);
        }
        
        public static CVector2 InverseMultiplyPoint(this Matrix m, CVector2 v)
        {
            return Microsoft.Xna.Framework.Vector2.Transform(v, Matrix.Invert(m));
        }
    }
}