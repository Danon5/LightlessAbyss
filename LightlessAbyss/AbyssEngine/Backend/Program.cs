using System;

namespace AbyssEngine.Backend
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using Engine game = new Engine();
            game.Run();
        }
    }
}
