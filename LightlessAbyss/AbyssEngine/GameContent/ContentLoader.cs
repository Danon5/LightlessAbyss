using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace AbyssEngine.GameContent
{
    public sealed class ContentLoader
    {
        private readonly ContentManager _contentManager;
        private List<IContentLoadPass> _loadPasses;
        
        public ContentLoader(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _loadPasses = new List<IContentLoadPass>();
            
            _contentManager.RootDirectory = "Content";
        }

        public void AddContentLoadPass(IContentLoadPass pass)
        {
            if (_loadPasses.Contains(pass))
                throw new ArgumentException($"Trying to add pass {pass} to ContentLoader, " +
                                            "but it is already contained within the list of passes!");
            
            _loadPasses.Add(pass);
        }
        
        public void LoadAllPasses()
        {
            foreach (IContentLoadPass pass in _loadPasses)
                pass.LoadPassContent(_contentManager);
        }
    }
}