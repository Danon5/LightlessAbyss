using System;
using AbyssEngine.CustomColor;
using AbyssEngine.CustomMath;
using AbyssEngine.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace AbyssEngine.Backend.Rendering
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
        
        public static void DrawText(string text, CVector2 position, CColor? color = null, FontId font = FontId.Default)
        {
            color ??= CColor.White;
            
            _sharedSpriteBatch.Begin(SpriteSortMode.Immediate);

            SpriteFont spriteFont = ContentDatabase.GetFont(font);
            _sharedSpriteBatch.DrawString(spriteFont, text, position, (CColor)color);
            
            _sharedSpriteBatch.End();
        }
    }
}