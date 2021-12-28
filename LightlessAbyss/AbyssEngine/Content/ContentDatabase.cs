using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace LightlessAbyss.AbyssEngine.Content
{
    public static class ContentDatabase
    {
        private static Dictionary<TextureId, Texture2D> _textureContent;
        private static Dictionary<FontId, SpriteFont> _fontContent;

        static ContentDatabase()
        {
            _textureContent = new Dictionary<TextureId, Texture2D>();
            _fontContent = new Dictionary<FontId, SpriteFont>();
        }

        public static void RegisterTexture(TextureId id, Texture2D texture) => _textureContent.Add(id, texture);
        public static Texture2D GetTexture(TextureId id) => _textureContent[id];

        public static void RegisterFont(FontId id, SpriteFont font) => _fontContent.Add(id, font);
        public static SpriteFont GetFont(FontId id) => _fontContent[id];
    }
}