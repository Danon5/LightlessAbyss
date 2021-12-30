using System;
using System.Collections.Generic;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbyssEngine.Backend.Rendering
{
    public sealed class EngineRenderer
    {
        public const int PPU = 128;

        /// <summary>
        /// The actual window's viewport, including letterboxing.
        /// </summary>
        public static Viewport WindowViewport => _gameScreen.GetWindowViewport();

        /// <summary>
        /// The Viewport of the window including letterboxing.
        /// </summary>
        public static Viewport DisplayedGameViewport => _gameScreen.GetDisplayedGameViewport();
        
        /// <summary>
        /// The Viewport of the window excluding letterboxing.
        /// </summary>
        public static Viewport RawGameViewport => _gameScreen.GetTargetTexViewport();
        
        /// <summary>
        /// The actual window's rect, including letterboxing.
        /// </summary>
        public static Rectangle WindowRect => WindowViewport.Bounds;
        
        /// <summary>
        /// The resized game display excluding letterboxing.
        /// </summary>
        public static Rectangle DisplayedGameRect => _gameScreen.GetDisplayedGameRect();
        
        /// <summary>
        /// The raw game display rect, directly from target tex size.
        /// </summary>
        public static Rectangle RawGameRect => _gameScreen.GetRawGameRect();

        /// <summary>
        /// Gets the letterboxing offset on the displayed game.
        /// </summary>
        public static CVector2 DisplayedGameOffset => _gameScreen.GetDisplayedGameOffset();
        
        public static Matrix CameraMatrix => _camera != null ? _camera.Matrix * _unitConversionMatrix : Matrix.Identity;

        public static Matrix PolygonScreenMatrix => Matrix.CreateScale(1f, -1f, 1f) *
                                                    Matrix.CreateTranslation(0f, RawGameRect.Height, 0f);
        public static float CameraOrthoSizeScaler => _camera?.OrthographicSize ?? 1f;
        public static GraphicsDevice GraphicsDevice => _graphicsDevice;

        private static EngineRenderer _singleton;

        private static Engine _engine;
        private static GraphicsDeviceManager _graphics;
        private static GraphicsDevice _graphicsDevice;
        private static Camera _camera;
        private static GameScreen _gameScreen;
        private static SpriteBatch _sharedSpriteBatch;
        private static SpriteRenderer _spriteRenderer;
        private static GUIRenderer _guiRenderer;
        private static PolygonRenderer _polygonRenderer;
        private static List<DrawableComponent> _drawables;
        private static Matrix _polygonScreenMatrix;

        private static Matrix _unitConversionMatrix;

        public EngineRenderer(Engine engine)
        {
            if (_singleton != null)
                throw new Exception("Attempting to create multiple EngineRenderers. This is not allowed.");
            
            _singleton = this;
            _engine = engine;
            
            _graphics = _engine.Graphics;
            _graphicsDevice = _graphics.GraphicsDevice;

            _camera = new Camera();
            _gameScreen = new GameScreen(engine, 1280, 720);

            _sharedSpriteBatch = new SpriteBatch(_graphicsDevice);

            _spriteRenderer = new SpriteRenderer(_sharedSpriteBatch);
            _guiRenderer = new GUIRenderer(_sharedSpriteBatch);
            _polygonRenderer = new PolygonRenderer(_graphicsDevice);
            
            _drawables = new List<DrawableComponent>();
            
            _unitConversionMatrix = Matrix.CreateScale(PPU, -PPU, 1f);
        }
        
        public static void SetFullscreen(bool state)
        {
            _graphics.IsFullScreen = state;
            _graphics.ApplyChanges();
        }
        
        public static void SetResolution(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
        }
        
        public static void SetFramerate(float framerate)
        {
            _engine.TargetElapsedTime = TimeSpan.FromSeconds(1f / framerate);
        }
        
        public static void SetVSync(bool state)
        {
            _graphics.SynchronizeWithVerticalRetrace = state;
            _graphics.ApplyChanges();
        }
        
        public static CVector2 WorldToPixel(CVector2 pixelPoint) => _unitConversionMatrix.MultiplyPoint(pixelPoint);
        
        public static CVector2 PixelToWorld(CVector2 worldPoint) => _unitConversionMatrix.InverseMultiplyPoint(worldPoint);

        public static void RegisterDrawable(DrawableComponent drawableComponent)
        {
            if (_drawables.Contains(drawableComponent))
                throw new Exception("Attempting to register a Drawable multiple times.");

            _drawables.Add(drawableComponent);
        }

        public static void UnregisterDrawable(DrawableComponent drawableComponent)
        {
            if (!_drawables.Contains(drawableComponent))
                throw new Exception("Attempting to unregister Drawable that is not currently registered.");

            _drawables.Remove(drawableComponent);
        }
        
        public void Clear(Color? color = null)
        {
            color ??= Color.Black;
            _graphicsDevice.Clear((Color)color);
        }
        
        public void RenderNewFrame()
        {
            _gameScreen.SetAsRenderTarget();
            Clear(new Color(.2f, .2f, .2f, 1f));
            
            DrawGame();

            _gameScreen.RemoveAsRenderTarget();
            Clear();
            _gameScreen.DrawToScreen();

            DrawGizmos();
            DrawGUI();
        }

        private void DrawGame()
        {
            foreach (DrawableComponent drawable in _drawables)
            {
                if (drawable.IsDestroyed)
                    throw new Exception("Draw being called on Drawable, but it has already been destroyed.");
                drawable.Draw();
            }
        }

        private void DrawGizmos()
        {
            foreach (Behaviour behaviour in _engine.Behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("GizmosUpdate being called on Behaviour, but it has already been destroyed.");
                behaviour.DrawGizmos();
            }
        }
        
        private const float FPS_DURATION = .5f;
        private float _fps;
        private float _sumFps;
        private int _fpsCount;
        private float _lastFpsTime;
        private void DrawGUI()
        {
            if (Time.TotalTime - _lastFpsTime > FPS_DURATION)
            {
                _fps = _sumFps / _fpsCount;
                _sumFps = 0f;
                _fpsCount = 0;
                _lastFpsTime += FPS_DURATION;
            }
            else
            {
                _sumFps += 1f / Time.DeltaTime;
                _fpsCount++;
            }
            
            GUIRenderer.DrawText($"Camera Position: {_camera.Position}", 
                new CVector2(5f, 5f), Color.Yellow);
            GUIRenderer.DrawText($"Mouse Screen Position: {MouseStateTracker.ScreenPosition}", 
                new CVector2(5f, 30f), Color.Yellow);
            GUIRenderer.DrawText($"Mouse World Position: {MouseStateTracker.WorldPosition}", 
                new CVector2(5f, 55f), Color.Yellow);
            GUIRenderer.DrawText($"Orthographic Size: {CMath.RoundToDecimal(_camera.OrthographicSize, 1)}", 
                new CVector2(5f, 80f), Color.Yellow);
            GUIRenderer.DrawText($"FPS: {CMath.Clamp(CMath.RoundToInt(_fps), 0, int.MaxValue)}", 
                new CVector2(5f, 105f), Color.Yellow);
        }
    }
}