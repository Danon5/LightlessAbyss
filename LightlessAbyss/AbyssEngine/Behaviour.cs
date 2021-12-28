using System;
using AbyssEngine.Backend;

namespace AbyssEngine
{
    public abstract class Behaviour : IEntityComponent, IDestroyable
    {
        public bool IsDestroyed { get; private set; }
        
        private Behaviour()
        {
            Engine.RegisterBehaviour(this);
        }
        
        public virtual void Initialize() { }
        public virtual void EarlyUpdate() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }

        public void Destroy()
        {
            Engine.UnregisterBehaviour(this);
            IsDestroyed = true;
        }
        
        protected bool Equals(Behaviour other) => ReferenceEquals(this, other);
        
        public override int GetHashCode() => HashCode.Combine(this);
        
        public override bool Equals(object obj) => obj is null ? IsDestroyed : ReferenceEquals(this, obj);

        public static bool operator ==(Behaviour left, Behaviour right)
        {
            if (left is null || left.IsDestroyed)
                return right is null || right.IsDestroyed;
            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Behaviour left, Behaviour right) => !(left == right);
    }
}