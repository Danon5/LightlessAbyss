using System;
using System.Collections.Generic;

namespace LightlessAbyss.AbyssEngine
{
    public class Entity : IDestroyable
    {
        public bool IsDestroyed { get; private set; }
        
        private readonly List<IEntityComponent> _components;

        public Entity()
        {
            _components = new List<IEntityComponent>();
        }

        public T AddComponent<T>() where T : IEntityComponent, new()
        {
            T comp = new T();

            _components.Add(comp);
            
            return comp;
        }

        public T GetComponent<T>() where T : IEntityComponent
        {
            foreach (IEntityComponent component in _components)
            {
                if (component is T desiredComponent)
                    return desiredComponent;
            }

            return default;
        }

        public void Destroy()
        {
            foreach (IEntityComponent component in _components)
            {
                if (component is IDestroyable destroyable)
                    destroyable.Destroy();
            }

            IsDestroyed = true;
        }

        public override bool Equals(object? obj) => obj is null ? IsDestroyed : ReferenceEquals(this, obj);

        public static bool operator ==(Entity left, Entity right)
        {
            if (left is null || left.IsDestroyed)
                return right is null || right.IsDestroyed;
            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Entity left, Entity right) => !(left == right);
    }
}