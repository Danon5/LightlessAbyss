using System;

namespace LightlessAbyss.AbyssEngine.CustomMath
{
    public static class CRandom
    {
        private static readonly Random _random;

        static CRandom()
        {
            _random = new Random();
        }
        
        public static int Range(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}