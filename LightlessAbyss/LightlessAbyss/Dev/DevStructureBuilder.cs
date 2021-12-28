using System.Collections.Generic;
using AbyssEngine;
using AbyssEngine.CustomMath;
using AbyssEngine.DebugUtils;
using Microsoft.Xna.Framework;

namespace LightlessAbyss.Dev
{
    public sealed class DevStructureBuilder : Behaviour
    {
        private Structure _structure;
        private IRoomDetector _roomDetector;
        private List<Room> _rooms;

        public override void Initialize()
        {
            base.Initialize();
            
            _structure = new Structure();
            _roomDetector = new FloodFillRoomDetector();
            _rooms = new List<Room>();
        }

        public override void Tick()
        {
            base.Tick();
            
            if (Controls.UseItem.IsHeld)
                PlaceTileAtMouse();
            else if (Controls.UseItemAltAbility.IsHeld)
                RemoveTileAtMouse();
            
            //_structure.Position = new CVector2(CMath.Sin(Time.TotalTime) * 4f, CMath.Cos(Time.TotalTime) * 4f);
            _structure.Rotation = CMath.Sin(Time.TotalTime / 4f) * 180f;
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            
            if (_structure == null) return;

            Gizmos.color = Color.Red;
            Gizmos.matrix = _structure.TranslationMatrix;
            Gizmos.StartGizmoBatch();
            
            CVector2 offset = new CVector2(.5f, .5f);
            
            foreach (StructureTile tile in _structure.GetTiles())
                Gizmos.DrawRectangle(tile.TilePosition + offset, CVector2.One, true);
            
            Gizmos.color = Color.Yellow;

            Gizmos.DrawWireRectangle(_structure.Bounds.Center, _structure.Bounds.Size, true);
            
            Gizmos.color = new Color(.5f, 1f, .5f, .25f);
            
            foreach (Room room in _rooms)
            {
                foreach (CVector2Int tilePos in room.TilePositions)
                    Gizmos.DrawRectangle(tilePos + offset, CVector2.One, true);
            }
            
            Gizmos.EndGizmoBatch();
        }
        
        private void PlaceTileAtMouse()
        {
            if (_structure.TryPlaceTileAtWorldPos(Controls.MouseWorldPosition))
                RecalculateRooms();
        }

        private void RemoveTileAtMouse()
        {
            if (_structure.TryRemoveTileAtWorldPos(Controls.MouseWorldPosition))
                RecalculateRooms();
        }

        private async void RecalculateRooms()
        {
            _rooms = await _roomDetector.FindRooms(_structure);
        }
    }
}