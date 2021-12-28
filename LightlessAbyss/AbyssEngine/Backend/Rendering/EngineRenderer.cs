using System;
using System.Collections.Generic;
using LightlessAbyss.AbyssEngine.Content;
using LightlessAbyss.AbyssEngine.CustomMath;
using LightlessAbyss.AbyssEngine.DebugUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightlessAbyss.AbyssEngine.Backend.Rendering
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
        
        public static Matrix RenderMatrix => _camera != null ? _camera.Matrix * _unitConversionMatrix : Matrix.Identity;
        
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
        private static List<Drawable> _drawables;
        
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
            
            _drawables = new List<Drawable>();
            
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

        public static void RegisterDrawable(Drawable drawable)
        {
            if (_drawables.Contains(drawable))
                throw new Exception("Attempting to register a Drawable multiple times.");

            _drawables.Add(drawable);
        }

        public static void UnregisterDrawable(Drawable drawable)
        {
            if (!_drawables.Contains(drawable))
                throw new Exception("Attempting to unregister Drawable that is not currently registered.");

            _drawables.Remove(drawable);
        }
        
        public void Clear(Color? color = null)
        {
            color ??= Color.Black;
            _graphicsDevice.Clear((Color)color);
        }
        
        public void RenderAll()
        {
            DrawGame();
            DrawGUI();
        }

        private void DrawGame()
        {
            _gameScreen.SetAsRenderTarget();
            Clear(new Color(.2f, .2f, .2f, 1f));
            
            // TODO: call renderableEntity.Draw() here for all renderable entities

            Texture2D tex = ContentDatabase.GetTexture(TextureId.Turret);
            SpriteRenderer.IndividualDraw(tex, CVector2.Zero, 0f, matrix: RenderMatrix);

            Gizmos.color = new Color(1f, 1f, 1f, .05f);
            CVector2 pos = Camera.Main.Position;
            Gizmos.DrawWireGrid(
                new CVector2((int)pos.x, (int)pos.y), 100, 75);

            foreach (Drawable drawable in _drawables)
            {
                if (drawable.IsDestroyed)
                    throw new Exception("Draw being called on Drawable, but it has already been destroyed.");
                drawable.Draw();
            }

            _gameScreen.RemoveAsRenderTarget();
            Clear();
            _gameScreen.Draw();
        }
        
        private const float FPS_DURATION = 1f;
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
            GUIRenderer.DrawText($"Orthographic Size: {CMathUtils.RoundToDecimal(_camera.OrthographicSize, 1)}", 
                new CVector2(5f, 80f), Color.Yellow);
            GUIRenderer.DrawText($"FPS: {CMathUtils.RoundToInt(_fps)}", 
                new CVector2(5f, 105f), Color.Yellow);
        }
    }
}