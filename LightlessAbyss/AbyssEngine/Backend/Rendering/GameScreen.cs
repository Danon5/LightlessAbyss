using System;
using LightlessAbyss.AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightlessAbyss.AbyssEngine.Backend.Rendering
{
    public sealed class GameScreen : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        private const int MIN_DIMENSIONS = 64;
        private const int MAX_DIMENSIONS = 4096;

        private readonly Engine _engine;
        private readonly RenderTarget2D _renderTarget;
        
        public GameScreen(Engine engine, int width, int height)
        {
            _engine = engine;
            _renderTarget = CreateRenderTarget(width, height);
        }

        public void SetAsRenderTarget()
        {
            _engine.GraphicsDevice.SetRenderTarget(_renderTarget);
        }

        public void RemoveAsRenderTarget()
        {
            _engine.GraphicsDevice.SetRenderTarget(null);
        }

        public void Draw()
        {
            SpriteRenderer.FullscreenDraw(_renderTarget);
        }

        public void Dispose()
        {
            _renderTarget?.Dispose();
        }

        public Viewport GetDisplayedGameViewport()
        {
            return new Viewport(GetDisplayedGameRect());
        }

        public Viewport GetTargetTexViewport()
        {
            return new Viewport(0, 0, _renderTarget.Width, _renderTarget.Height);
        }

        public Viewport GetWindowViewport()
        {
            return new Viewport(_engine.GraphicsDevice.PresentationParameters.Bounds);
        }

        public Rectangle GetDisplayedGameRect()
        {
            Viewport windowViewport = GetWindowViewport();

            float targetAspectRatio = (float)Width / Height;

            float xOffset = 0f;
            float yOffset = 0f;
            float displayWidth = windowViewport.Width;
            float displayHeight = windowViewport.Height;
            
            if (targetAspectRatio < windowViewport.AspectRatio)
            {
                displayWidth = displayHeight * targetAspectRatio;
                xOffset = (windowViewport.Width - displayWidth) / 2f;
            }
            else if (targetAspectRatio > windowViewport.AspectRatio)
            {
                displayHeight = displayWidth / targetAspectRatio;
                yOffset = (windowViewport.Height - displayHeight) / 2f;
            }
            
            return new Rectangle((int)xOffset, (int)yOffset, (int)displayWidth, (int)displayHeight);
        }

        public CVector2 GetDisplayedGameOffset()
        {
            Viewport viewport = GetWindowViewport();

            float targetAspectRatio = (float)Width / Height;

            float xOffset = 0f;
            float yOffset = 0f;
            float displayWidth = viewport.Width;
            float displayHeight = viewport.Height;
            
            if (targetAspectRatio < viewport.AspectRatio)
            {
                displayWidth = displayHeight * targetAspectRatio;
                xOffset = (viewport.Width - displayWidth) / 2f;
            }
            else if (targetAspectRatio > viewport.AspectRatio)
            {
                displayHeight = displayWidth / targetAspectRatio;
                yOffset = (viewport.Height - displayHeight) / 2f;
            }
            
            return new CVector2((int)xOffset, (int)yOffset);
        }

        public Rectangle GetRawGameRect() => _renderTarget.Bounds;

        private RenderTarget2D CreateRenderTarget(int width, int height)
        {
            Width = Math.Clamp(width, MIN_DIMENSIONS, MAX_DIMENSIONS);
            Height = Math.Clamp(height, MIN_DIMENSIONS, MAX_DIMENSIONS);
            
            return new RenderTarget2D(_engine.GraphicsDevice, Width, Height);
        }
    }
}