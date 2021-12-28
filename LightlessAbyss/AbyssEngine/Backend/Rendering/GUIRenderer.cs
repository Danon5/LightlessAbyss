using System;
using LightlessAbyss.AbyssEngine.Content;
using LightlessAbyss.AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightlessAbyss.AbyssEngine.Backend.Rendering
{
    public sealed class GUIRenderer
    {
        private static GUIRenderer _singleton;

        private static SpriteBatch _sharedSpriteBatch;

        public GUIRenderer(SpriteBatch sharedSpriteBatch)
        {
            if (_singleton != null)
                throw new Exception("Attempting to create multiple GUIRenderers. This is not allowed.");
            
            _singleton = this;
            
            _sharedSpriteBatch = sharedSpriteBatch;
        }

        // TODO: GUI batching, rename DrawText to IndividualDrawText
        
        public static void DrawText(string text, CVector2 position, Color? color = null, FontId font = FontId.Default)
        {
            color ??= Color.White;
            
            _sharedSpriteBatch.Begin(SpriteSortMode.Immediate);

            Rectangle viewport = EngineRenderer.DisplayedGameRect;
            CVector2 viewportOffset = new CVector2(viewport.X, viewport.Y);
            
            SpriteFont spriteFont = ContentDatabase.GetFont(font);
            _sharedSpriteBatch.DrawString(spriteFont, text, position + viewportOffset, (Color)color);
            
            _sharedSpriteBatch.End();
        }
    }
}