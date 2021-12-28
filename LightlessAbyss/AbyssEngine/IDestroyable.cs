namespace LightlessAbyss.AbyssEngine
{
    public interface IDestroyable
    {
        bool IsDestroyed { get; }
        void Destroy();
    }
}