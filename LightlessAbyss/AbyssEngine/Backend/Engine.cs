using System;
using System.Collections.Generic;
using AbyssEngine.Backend.Rendering;
using AbyssEngine.GameContent;
using LightlessAbyss;
using Microsoft.Xna.Framework;

namespace AbyssEngine.Backend
{
    public sealed class Engine : Game
    {
        public GraphicsDeviceManager Graphics { get; }

        private static Engine _singleton;

        public List<Behaviour> Behaviours => _behaviours;
        
        private List<Behaviour> _behaviours;
        
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
            if (_singleton._behaviours.Contains(behaviour))
                throw new Exception("Attempting to register a Behaviour multiple times.");
            
            _singleton._behaviours.Add(behaviour);
            behaviour.Initialize();
        }

        public static void UnregisterBehaviour(Behaviour behaviour)
        {
            if (!_singleton._behaviours.Contains(behaviour))
                throw new Exception("Attempting to unregister Drawable that is not currently registered.");

            _singleton._behaviours.Remove(behaviour);
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
        }

        protected override void Update(GameTime gameTime)
        {
            Time.EngineUpdateGameTime(gameTime);
            
            _inputPoller.Poll();

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("EarlyUpdate being called on Behaviour, but it has already been destroyed.");
                behaviour.EarlyTick();
            }

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("Update being called on Behaviour, but it has already been destroyed.");
                behaviour.Tick();
            }

            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour.IsDestroyed)
                    throw new Exception("LateUpdate being called on Behaviour, but it has already been destroyed.");
                behaviour.LateTick();
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _engineRenderer.RenderNewFrame();

            base.Draw(gameTime);
        }
    }
}
