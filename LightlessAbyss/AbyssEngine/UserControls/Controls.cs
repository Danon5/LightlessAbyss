using AbyssEngine.CustomMath;
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
        public static readonly IControlsBind UseItem = new MouseButtonBind(MouseButton.Left);
        public static readonly IControlsBind UseItemAltAbility = new MouseButtonBind(MouseButton.Right);

        public static int MouseScrollDelta => MouseStateTracker.ScrollDelta;
        public static CVector2 MouseScreenPosition => MouseStateTracker.ScreenPosition;
        public static CVector2 MouseWorldPosition => MouseStateTracker.WorldPosition;
    }
}