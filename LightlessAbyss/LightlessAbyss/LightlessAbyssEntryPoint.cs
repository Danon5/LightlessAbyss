using AbyssEngine;

namespace LightlessAbyss
{
    public sealed class LightlessAbyssEntryPoint : IGameEntryPoint
    {
        private GameManager _gameManager;
        
        public void StartGame()
        {
            _gameManager = new GameManager();
        }
    }
}