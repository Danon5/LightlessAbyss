namespace AbyssEngine
{
    public interface IDestroyable
    {
        bool IsDestroyed { get; }
        void Destroy();
    }
}