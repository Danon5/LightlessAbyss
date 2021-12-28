using Microsoft.Xna.Framework.Input;

namespace AbyssEngine
{
    public class KeyBind : IControlsBind
    {
        public Keys Bind => _bind;
        public bool HasBind => _hasBind;

        public bool IsPressed => _hasBind && KeyStateTracker.KeyIsPressed(Bind);
        public bool IsHeld => _hasBind && KeyStateTracker.KeyIsHeld(Bind);

        private Keys _bind;
        private bool _hasBind;
        
        public KeyBind(Keys bind)
        {
            _bind = bind;
            _hasBind = true;
        }

        public void SetBind(Keys key)
        {
            _bind = key;
            _hasBind = true;
        }

        public void RemoveBind()
        {
            _bind = default;
            _hasBind = false;
        }
    }
}