using System.Collections.Generic;
using LightlessAbyss.AbyssEngine.CustomMath;

namespace LightlessAbyss.AbyssEngine
{
    public static class Utils
    {
        public static T[,] ResizeArray2D<T>(this T[,] arr, int newWidth, int newHeight)
        {
            T[,] resized = new T[newWidth, newHeight];

            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    resized[x, y] = arr[x, y];
                }
            }

            return resized;
        }
        
        public static T GetRandomElementOfList<T>(this List<T> list)
        {
            return list[CRandom.Range(0, list.Count)];
        }
    }
}