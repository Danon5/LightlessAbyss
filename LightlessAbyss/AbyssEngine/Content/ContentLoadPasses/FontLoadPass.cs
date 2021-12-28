using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LightlessAbyss.AbyssEngine.Content
{
    public class FontLoadPass : IContentLoadPass
    {
        public void LoadPassContent(ContentManager contentManager)
        {
            LoadFonts(contentManager);
        }

        private void LoadFonts(ContentManager c)
        {
            ContentDatabase.RegisterFont(FontId.Default, c.Load<SpriteFont>("Fonts/DefaultFont"));
        }
    }
}