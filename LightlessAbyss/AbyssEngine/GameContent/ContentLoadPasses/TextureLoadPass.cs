using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AbyssEngine.GameContent
{
    public sealed class TextureLoadPass : IContentLoadPass
    {
        public void LoadPassContent(ContentManager contentManager)
        {
            LoadDevTextures(contentManager);
        }

        private void LoadDevTextures(ContentManager c)
        {
            ContentDatabase.RegisterTexture(TextureId.Turret, 
                c.Load<Texture2D>("DevContent/Turret"));
        }
    }
}