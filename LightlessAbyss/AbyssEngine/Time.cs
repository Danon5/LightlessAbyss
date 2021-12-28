using Microsoft.Xna.Framework;

namespace LightlessAbyss.AbyssEngine
{
    public static class Time
    {
        public static float TotalTime { get; private set; }
        public static float DeltaTime { get; private set; }
        
        public static void EngineUpdateGameTime(GameTime gameTime)
        {
            float oldTotalTime = TotalTime;
            TotalTime = (float)gameTime.TotalGameTime.TotalSeconds;
            DeltaTime = TotalTime - oldTotalTime;
        }
    }
}