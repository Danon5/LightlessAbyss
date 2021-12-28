using System.Collections.Generic;
using System.Linq;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;

namespace AbyssEngine.Dev
{
    public class Structure
    {
        public CVector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateTranslationMatrix();
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateTranslationMatrix();
            }
        }

        public Matrix TranslationMatrix => _translationMatrix;
        public BoundsInt Bounds => _bounds;

        private Dictionary<CVector2Int, StructureTile> _tiles;
        private Matrix _translationMatrix;
        private Vector3 _position;
        private BoundsInt _bounds;

        private float _rotation;

        public Structure()
        {
            _translationMatrix = Matrix.Identity;
            _tiles = new Dictionary<CVector2Int, StructureTile>();
            _bounds = new BoundsInt();
        }

        public Structure(CVector2 pos, float rot)
        {
            DirectSetTranslationMatrix(pos, rot);
            _tiles = new Dictionary<CVector2Int, StructureTile>();
        }

        public StructureTile[] GetTiles()
        {
            return _tiles.Values.ToArray();
        }

        public bool TryPlaceTileAtWorldPos(Vector3 worldPos)
        {
            CVector2Int tilePos = WorldToTile(worldPos);

            if (_tiles.ContainsKey(tilePos)) return false;

            AddTileAtTilePos(tilePos);
            return true;
        }

        public bool TryRemoveTileAtWorldPos(Vector3 worldPos)
        {
            CVector2Int tilePos = WorldToTile(worldPos);

            if (!_tiles.ContainsKey(tilePos)) return false;

            RemoveTileAtTilePos(tilePos);
            return true;
        }

        public bool TryGetTileAtWorldPos(CVector2 worldPos, out StructureTile tile)
        {
            CVector2Int tilePos = WorldToTile(worldPos);
            return TryGetTileAtTilePos(tilePos, out tile);
        }

        public bool TryGetTileAtTilePos(CVector2Int tilePos, out StructureTile tile)
        {
            if (!_tiles.ContainsKey(tilePos))
            {
                tile = null;
                return false;
            }

            tile = _tiles[tilePos];
            return true;
        }

        public bool HasTileAtWorldPos(CVector2 worldPos)
        {
            return _tiles.ContainsKey(WorldToTile(worldPos));
        }
        
        public bool HasTileAtTilePos(CVector2Int tilePos)
        {
            return _tiles.ContainsKey(tilePos);
        }

        public bool HasCollisionWallAtWorldPos(CVector2 worldPos)
        {
            CVector2Int tilePos = WorldToTile(worldPos);
            return HasTileAtTilePos(tilePos) && _tiles[tilePos].IsCollisionWall;
        }
        
        public bool HasCollisionWallAtTilePos(CVector2Int tilePos)
        {
            return HasTileAtTilePos(tilePos) && _tiles[tilePos].IsCollisionWall;
        }
        
        public bool HasWaterWallAtWorldPos(CVector2 worldPos)
        {
            CVector2Int tilePos = WorldToTile(worldPos);
            return HasTileAtTilePos(tilePos) && _tiles[tilePos].IsWaterWall;
        }
        
        public bool HasWaterWallAtTilePos(CVector2Int tilePos)
        {
            return HasTileAtTilePos(tilePos) && _tiles[tilePos].IsWaterWall;
        }

        public CVector2Int WorldToTile(CVector2 worldPos)
        {
            return _translationMatrix.InverseMultiplyPoint(worldPos).FloorToVec2Int();
        }

        public CVector2 TileToWorld(CVector2Int tilePos)
        {
            return _translationMatrix.MultiplyPoint(tilePos);
        }

        private void AddTileAtTilePos(CVector2Int tilePos)
        {
            _tiles.Add(tilePos, new StructureTile(this, tilePos));
            RegenerateBounds();
        }

        private void RemoveTileAtTilePos(CVector2Int tilePos)
        {
            _tiles.Remove(tilePos);
            RegenerateBounds();
        }

        private void RegenerateBounds()
        {
            _bounds = BoundsInt.GenerateBoundsIntFromPositions(_tiles.Keys.ToArray(), true);
        }

        private void DirectSetTranslationMatrix(CVector2 pos, float rot)
        {
            _position = pos;
            _rotation = rot;
            UpdateTranslationMatrix();
        }

        private void UpdateTranslationMatrix()
        {
            _translationMatrix = CMathUtils.MatrixTRS(
                _position,
                _rotation,
                CVector2.One);
        }
    }
}