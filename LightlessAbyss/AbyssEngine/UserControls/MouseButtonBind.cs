namespace AbyssEngine
{
    public class MouseButtonBind : IControlsBind
    {
        public MouseButton Bind => _bind;
        public bool HasBind => _hasBind;

        public bool IsPressed => _hasBind && MouseStateTracker.MouseButtonIsPressed(Bind);
        public bool IsHeld => _hasBind && MouseStateTracker.MouseButtonIsHeld(Bind);

        private MouseButton _bind;
        private bool _hasBind;
        
        public MouseButtonBind(MouseButton bind)
        {
            _bind = bind;
            _hasBind = true;
        }

        public void SetBind(MouseButton key)
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