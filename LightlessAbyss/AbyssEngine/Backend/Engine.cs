using System;
using System.Collections.Generic;
using LightlessAbyss.AbyssEngine.Backend.Rendering;
using LightlessAbyss.AbyssEngine.Content;
using LightlessAbyss.AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;

namespace LightlessAbyss.AbyssEngine.Backend
{
    public sealed class Engine : Game
    {
        public GraphicsDeviceManager Graphics { get; }

        private static Engine _singleton;

        private static List<Behaviour> _behaviours;
        
        private ContentLoader _contentLoader;
        private EngineRenderer _engineRenderer;
        private InputPoller _inputPoller;
        private IGameEntryPoint _gameEntryPoint;

        public Engine()
        {
            if (_singleton != null)
                throw new Exception("Attempting to create multiple Engines. This is not allowed.");

            _singleton = this;
            
            Graphics = new GraphicsDeviceManager(this);

            IsMouseVisible = true;
        }

        public static void RegisterBehaviour(Behaviour behaviour)
        {
            if (_behaviours.Contains(behaviour))
                throw new Exception("Attempting to register a Behaviour multiple times.");
            
            _behaviours.Add(behaviour);
        }

        public static void UnregisterBehaviour(Behaviour behaviour)
        {
            if (!_behaviours.Contains(behaviour))
                throw new Exception("Attempting to unregister Drawable that is not currently registered.");

            _behaviours.Remove(behaviour);
        }

        protected override void Initialize()
        {
            Window.Title = "Lightless Abyss";
            Window.AllowUserResizing = true;

            _contentLoader = new ContentLoader(Content);
            _engineRenderer = new EngineRenderer(this);
            _inputPoller = new InputPoller();
            _behaviours = new List<Behaviour>();

            IsFixedTimeStep = false;

            EngineRenderer.SetVSync(false);
            EngineRenderer.SetFramerate(60f);
            EngineRenderer.SetFullscreen(false);
            EngineRenderer.SetResolution(1280, 720);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _contentLoader.AddContentLoadPass(new TextureLoadPass());
            _contentLoader.AddContentLoadPass(new FontLoadPass());

            _contentLoader.LoadAllPasses();
            
            StartGame();
        }

        private void StartGame()
        {
            _gameEntryPoint = new LightlessAbyssEntryPoint();
            _gameEntryPoint.StartGame();
            InitializeBehaviours();
        }

        private void InitializeBehaviours()
        {
            foreach (Behaviour behaviour in _behaviours)
                behaviour.Initialize();
        }

        private float _desiredOrthographicSize = 1f;
        private CVector2 _camVel = CVector2.Zero;
        protected override void Update(GameTime gameTime)
        {
            Time.EngineUpdateGameTime(gameTime);
            
            _inputPoller.Poll();

            _desiredOrthographicSize -= Controls.MouseScrollDelta * .1f * _desiredOrthographicSize;
            _desiredOrthographicSize = Math.Clamp(_desiredOrthographicSize, .01f, 25f);
            Camera.Main.OrthographicSize = 
                CMathUtils.Lerp(Camera.Main.OrthographicSize, _desiredOrthographicSize, 15f * Time.DeltaTime);

            CVector2 axis = new CVector2();

            if (Controls.Up.IsHeld)
                axis.y += 1;
            if (Controls.Down.IsHeld)
                axis.y -= 1;
            if (Controls.Right.IsHeld)
                axis.x += 1;
            if (Controls.Left.IsHeld)
                axis.x -= 1;

            axis = axis.Normalized;

            CVector2 desiredVel = CVector2.Zero;

            if (axis.Magnitude > 0f)
                desiredVel = axis * (Controls.Sprint.IsHeld ? 5f : 2.5f);
            
            _camVel = CVector2.Lerp(_camVel, desiredVel, 15f * Time.DeltaTime);

            Camera.Main.Position += _camVel * (Camera.Main.OrthographicSize * Time.DeltaTime);

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("EarlyUpdate being called on Behaviour, but it has already been destroyed.");
                behaviour.EarlyUpdate();
            }

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("Update being called on Behaviour, but it has already been destroyed.");
                behaviour.Update();
            }

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("LateUpdate being called on Behaviour, but it has already been destroyed.");
                behaviour.LateUpdate();
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _engineRenderer.RenderAll();

            base.Draw(gameTime);
        }
    }
}
