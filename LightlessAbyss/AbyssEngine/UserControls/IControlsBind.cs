namespace LightlessAbyss.AbyssEngine
{
    public interface IControlsBind
    {
        bool IsPressed { get; }
        bool IsHeld { get; }
    }
}