using System.Collections.Generic;
using System.Linq;

namespace AbyssEngine.CustomMath
{
    public struct BoundsInt
    {
        public CVector2Int Min => _min;
        public CVector2Int Max => _max;
        public CVector2 Center => _center;
        public CVector2Int IntCenter => (CVector2Int)_center;
        public CVector2 Size => _size;
        public CVector2Int IntSize => (CVector2Int)_size;
        public CVector2 Extents => _extents;
        public CVector2Int IntExtents => (CVector2Int)_extents;
        public int Top => Max.y;
        public int Bottom => Min.y;
        public int Right => Max.x;
        public int Left => Min.x;

        private CVector2Int _min;
        private CVector2Int _max;
        private CVector2 _center;
        private CVector2 _size;
        private CVector2 _extents;

        public BoundsInt(CVector2Int min, CVector2Int max) : this()
        {
            SetMinMax(min, max);
        }
        
        public void SetMinMax(CVector2Int min, CVector2Int max)
        {
            _min = min;
            _max = max;
            _center = (min.ToVec2() + max) / 2f;
            _size = max - min;
            _extents = _size / 2f;
        }
        
        public bool Contains(CVector2Int position)
        {
            return
                position.x >= _min.x &&
                position.x < _max.x &&
                position.y >= _min.y &&
                position.y < _max.y;
        }
        
        public bool Contains(CVector2 position)
        {
            return
                position.x >= _min.x &&
                position.x < _max.x &&
                position.y >= _min.y &&
                position.y < _max.y;
        }
        
        public List<CVector2Int> GetAllPositionsWithin()
        {
            List<CVector2Int> positions = new List<CVector2Int>();

            for (int x = 0; x < IntSize.x; x++)
                for (int y = 0; y < IntSize.y; y++)
                    positions.Add(_min + new CVector2Int(x, y));

            return positions;
        }
        
        public static BoundsInt GenerateBoundsIntFromPositions(
            ICollection<CVector2Int> positions, bool addPadding = false)
        {
            BoundsInt bounds = new BoundsInt();
            
            if (positions.Count == 0)
            {
                bounds.SetMinMax(CVector2Int.Zero, CVector2Int.Zero);
                return bounds;
            }
            
            CVector2Int initialPos = positions.ElementAt(0);
            CVector2Int min = initialPos;
            CVector2Int max = initialPos;
            
            foreach (CVector2Int pos in positions)
            {
                if (pos.x < min.x) // left
                    min.x = pos.x;
                else if (pos.x > max.x) // right
                    max.x = pos.x;
                
                if (pos.y < min.y) // down
                    min.y = pos.y;
                else if (pos.y > max.y) // up
                    max.y = pos.y;
            }

            if (addPadding)
                bounds.SetMinMax(min - CVector2Int.One, max + CVector2Int.One + CVector2Int.One);
            else
                bounds.SetMinMax(min, max + CVector2Int.One);

            return bounds;
        }
    }
}