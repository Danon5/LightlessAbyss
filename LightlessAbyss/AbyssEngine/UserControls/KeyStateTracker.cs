using Microsoft.Xna.Framework.Input;

namespace AbyssEngine
{
    public static class KeyStateTracker
    {
        private static KeyboardState _currentState;
        private static KeyboardState _previousState;

        public static bool KeyIsPressed(Keys key)
        {
            return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
        }

        public static bool KeyIsHeld(Keys key)
        {
            return _currentState.IsKeyDown(key);
        }

        public static void UpdateState()
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();
        }
    }
}