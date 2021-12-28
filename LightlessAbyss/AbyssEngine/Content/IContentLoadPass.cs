using Microsoft.Xna.Framework.Content;

namespace LightlessAbyss.AbyssEngine.Content
{
    public interface IContentLoadPass
    {
        void LoadPassContent(ContentManager contentManager);
    }
}