using Microsoft.Xna.Framework.Input;

namespace AbyssEngine
{
    public static class Controls
    {
        public static readonly IControlsBind Up = new KeyBind(Keys.W);
        public static readonly IControlsBind Down = new KeyBind(Keys.S);
        public static readonly IControlsBind Left = new KeyBind(Keys.A);
        public static readonly IControlsBind Right = new KeyBind(Keys.D);
        public static readonly IControlsBind Sprint = new KeyBind(Keys.LeftShift);

        public static int MouseScrollDelta => MouseStateTracker.ScrollDelta;
    }
}