using System;
using LightlessAbyss.AbyssEngine.CustomMath;
using Microsoft.Xna.Framework.Input;

namespace LightlessAbyss.AbyssEngine
{
    public static class MouseStateTracker
    {
        private static MouseState _currentState;
        private static MouseState _previousState;

        private static int _previousScrollValue;

        public static int ScrollDelta
        {
            get
            {
                int delta = _currentState.ScrollWheelValue - _previousScrollValue;
                _previousScrollValue = _currentState.ScrollWheelValue;
                return delta / 100;
            }
        }

        public static CVector2 ScreenPosition => _currentState.Position.ToVector2();

        public static CVector2 WorldPosition =>
            Camera.Main != null ? Camera.Main.ScreenToWorld(ScreenPosition): CVector2.Zero;

        public static bool MouseButtonIsPressed(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentState.LeftButton == ButtonState.Pressed &&
                                    _previousState.LeftButton != ButtonState.Pressed,
                MouseButton.Right => _currentState.RightButton == ButtonState.Pressed &&
                                     _previousState.RightButton != ButtonState.Pressed,
                MouseButton.Middle => _currentState.MiddleButton == ButtonState.Pressed &&
                                      _previousState.MiddleButton != ButtonState.Pressed,
                MouseButton.Mouse4 => _currentState.XButton1 == ButtonState.Pressed &&
                                      _previousState.XButton1 != ButtonState.Pressed,
                MouseButton.Mouse5 => _currentState.XButton2 == ButtonState.Pressed &&
                                      _previousState.XButton2 != ButtonState.Pressed,
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
            };
        }
        
        public static bool MouseButtonIsHeld(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentState.MiddleButton == ButtonState.Pressed,
                MouseButton.Mouse4 => _currentState.XButton1 == ButtonState.Pressed,
                MouseButton.Mouse5 => _currentState.XButton2 == ButtonState.Pressed,
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
            };
        }

        public static void UpdateState()
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState();
        }
    }
}