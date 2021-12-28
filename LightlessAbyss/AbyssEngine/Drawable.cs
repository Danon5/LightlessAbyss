using LightlessAbyss.AbyssEngine.Backend.Rendering;

namespace LightlessAbyss.AbyssEngine
{
    public abstract class Drawable : IEntityComponent, IDestroyable
    {
        public bool IsDestroyed { get; private set; }

        private Drawable()
        {
            EngineRenderer.RegisterDrawable(this);
        }
        
        public virtual void Draw() { }

        
        public void Destroy()
        {
            EngineRenderer.UnregisterDrawable(this);
            IsDestroyed = true;
        }
        
        public override bool Equals(object? obj) => obj is null ? IsDestroyed : ReferenceEquals(this, obj);

        public static bool operator ==(Drawable left, Drawable right)
        {
            if (left is null || left.IsDestroyed)
                return right is null || right.IsDestroyed;
            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Drawable left, Drawable right) => !(left == right);
    }
}