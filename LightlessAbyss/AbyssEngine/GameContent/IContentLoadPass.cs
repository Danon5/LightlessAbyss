using Microsoft.Xna.Framework.Content;

namespace AbyssEngine.GameContent
{
    public interface IContentLoadPass
    {
        void LoadPassContent(ContentManager contentManager);
    }
}