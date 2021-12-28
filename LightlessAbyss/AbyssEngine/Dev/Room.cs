using System.Collections.Generic;
using AbyssEngine.CustomMath;

namespace AbyssEngine.Dev
{
    public class Room
    {
        public BoundsInt Bounds => _bounds;
        public List<CVector2Int> TilePositions => _tilePositions;
        
        private BoundsInt _bounds;
        private readonly List<CVector2Int> _tilePositions;
        
        public Room(List<CVector2Int> tilePositions)
        {
            _tilePositions = tilePositions;
            _bounds = BoundsInt.GenerateBoundsIntFromPositions(tilePositions);
        }
    }
}