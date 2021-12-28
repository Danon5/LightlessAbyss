using System;
using System.Collections.Generic;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbyssEngine.Backend.Rendering
{
    public sealed class SpriteRenderer
    {
        private static SpriteRenderer _singleton;

        private static SpriteBatch _sharedSpriteBatch;
        private static Queue<BatchedDraw> _queuedDraws;
        
        private static bool _batchInProgress;

        public SpriteRenderer(SpriteBatch sharedSpriteBatch)
        {
            if (_singleton != null)
                throw new Exception("Attempting to create multiple SpriteRenderers. This is not allowed.");
            
            _singleton = this;
            
            _sharedSpriteBatch = sharedSpriteBatch;
            _queuedDraws = new Queue<BatchedDraw>();
        }

        public static void BeginSpriteBatch(SpriteSortMode sortMode = SpriteSortMode.Deferred, Matrix? matrix = null)
        {
            if (_batchInProgress)
                throw new Exception("Cannot start batch with batch already started!");
            
            matrix ??= Matrix.Identity;
            
            _sharedSpriteBatch.Begin(
                sortMode,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp, 
                transformMatrix: matrix);
            
            _batchInProgress = true;
        }

        public static void DrawBatchedSprite(Texture2D tex, CVector2 position, float rotation, 
            CVector2? scale = null, Color? color = null, OriginMode originMode = OriginMode.Center, 
            SpriteEffects effect = SpriteEffects.None, float layerDepth = 0f, bool batched = false)
        {
            if (!_batchInProgress)
                throw new Exception("Cannot batch draw with no batch started!");

            scale ??= new CVector2(1f, 1f);
            color ??= Color.White;

            _sharedSpriteBatch.Draw(
                tex, 
                position, 
                null, 
                (Color)color, 
                rotation, 
                CalculateOrigin(tex, originMode), 
                CalculateScale(scale.Value), 
                effect, 
                layerDepth);
        }

        public static void EndSpriteBatch()
        {
            if (!_batchInProgress)
                throw new Exception("Cannot end batch with no batch started!");
            
            _sharedSpriteBatch.End();
            
            _batchInProgress = false;
        }

        public static void IndividualDraw(Texture2D tex, CVector2 position, float rotation, CVector2? scale = null,
            Color? color = null, OriginMode originMode = OriginMode.Center, CVector2 customOrigin = default,
            SpriteEffects effect = SpriteEffects.None, float layerDepth = 0f, Matrix? matrix = null)
        {
            if (_batchInProgress)
                throw new Exception("Cannot individual draw while batched draw already started!");

            matrix ??= Matrix.Identity;
            
            BeginSpriteBatch(SpriteSortMode.Immediate, matrix);
            
            scale ??= new CVector2(1f, 1f);
            color ??= Color.White;

            _sharedSpriteBatch.Draw(
                tex, 
                position, 
                null, 
                (Color)color, 
                rotation, 
                CalculateOrigin(tex, originMode, customOrigin), 
                CalculateScale(scale.Value), 
                effect, 
                layerDepth);
            
            EndSpriteBatch();
        }

        public static void FullscreenDraw(Texture2D tex)
        {
            BeginSpriteBatch(SpriteSortMode.Immediate, Matrix.Identity);
            
            _sharedSpriteBatch.Draw(tex, EngineRenderer.DisplayedGameRect, Color.White);
            
            EndSpriteBatch();
        }
        
        private static CVector2 CalculateScale(CVector2 scale)
        {
            return new CVector2(scale.x, -scale.y) * (1f / EngineRenderer.PPU);
        }

        private static CVector2 CalculateOrigin(Texture2D tex, OriginMode originMode, CVector2 customOrigin = default)
        {
            switch (originMode)
            {
                case OriginMode.Center:
                    return new CVector2(tex.Width, tex.Height) / 2f;
                case OriginMode.Custom:
                    return customOrigin;
                default:
                    throw new ArgumentOutOfRangeException(nameof(originMode), originMode, null);
            }
        }
        
        private struct BatchedDraw
        {
            public Texture2D texture;
            public CVector2 position;
            public float rotation;
            public CVector2 scale;
            public Color color;
            public OriginMode originMode;
            public SpriteEffects effect;
            public float layerDepth;
        }
    }
}