using AbyssEngine.CustomMath;

namespace AbyssEngine.Dev
{
    public class StructureTile
    {
        public CVector2 WorldPosition => AttachedStructure.TileToWorld(TilePosition);
        public CVector2Int TilePosition { get; private set; }
        public Structure AttachedStructure { get; private set; }
        public bool IsCollisionWall { get; private set; }
        public bool IsWaterWall { get; private set; }

        public StructureTile(Structure attachedStructure, CVector2Int tilePos)
        {
            SetAttachedStructure(attachedStructure, tilePos);
            IsCollisionWall = true;
            IsWaterWall = true;
        }
        
        public void SetAttachedStructure(Structure attachedStructure, CVector2Int tilePos)
        {
            AttachedStructure = attachedStructure;
            TilePosition = tilePos;
        }
    }
}