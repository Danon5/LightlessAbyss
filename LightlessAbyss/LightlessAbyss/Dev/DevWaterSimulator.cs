using AbyssEngine;
using AbyssEngine.CustomMath;
using AbyssEngine.DebugUtils;
using Microsoft.Xna.Framework;

namespace LightlessAbyss.Dev
{
    public sealed class DevWaterSimulator : Behaviour
    {
        private bool _doMovement;
        private bool _doRotation = true;
        private bool _cameraFollowMovement;
        private bool _cameraFollowRotation;
        private bool _drawTilemapGizmos = true;
        private bool _drawDirectionGizmos;
        
        private WaterSimulation _waterSim;
        private Structure _structure;

        private const int SIM_FRAMERATE = 30;
        private const float SIM_FRAME_DURATION = 1f / SIM_FRAMERATE;
        private float _lastSimTotalTime;
        
        private Camera _cam;

        public override void Initialize()
        {
            base.Initialize();
            
            _cam = Camera.Main;
            
            _structure = new Structure();
            _structure.Position = new CVector2(-25f, -25f);
            _waterSim = new WaterSimulation(_structure);
        }

        public override void Tick()
        {
            base.Tick();
            
            if (_doMovement)
                _structure.Position = new CVector2(
                    CMath.Sin(Time.TotalTime / 2f) * 8f, CMath.Cos(Time.TotalTime / 2f) * 4f);
            if (_doRotation)
                _structure.Rotation = CMath.Sin(Time.TotalTime / 4f) * 180f;

            if (Controls.UseItem.IsHeld)
            {
                CVector2 worldMousePos = Controls.MouseWorldPosition;
                _waterSim.ModifyWaterAtWorldPos(worldMousePos, 50f * Time.DeltaTime);
            }
            else if (Controls.UseItemAltAbility.IsHeld)
            {
                CVector2 worldMousePos = Controls.MouseWorldPosition;
                _structure.TryPlaceTileAtWorldPos(worldMousePos);
                _waterSim.ResizeToStructureBounds();
            }
            
            if (Time.TotalTime > _lastSimTotalTime + SIM_FRAME_DURATION)
            {
                _lastSimTotalTime += SIM_FRAME_DURATION;
                _waterSim.Step(3);
            }
        }
        
        public override void DrawGizmos()
        {
            base.DrawGizmos();
            
            if (_drawTilemapGizmos)
                DrawTilemapGizmos();
            if (_drawDirectionGizmos)
                DrawDirectionTestGizmos();
        }
        
        private int[,] _testArr = new int[3, 3];
        private float _eulerZTest;
        private CVector2Int GetIndexInWorldDir(CVector2 worldDir, int originX, int originY)
        {
            float worldAngle = CMath.NormalizeAngle(CMath.DirectionToDegAngle(worldDir) - _eulerZTest);
            CVector2 correctedDir = CMath.DegAngleToDirection(worldAngle);
            CVector2Int indexOffset = new CVector2Int(
                CMath.RoundToInt(correctedDir.x), 
                CMath.RoundToInt(correctedDir.y));
            CVector2Int origin = new CVector2Int(originX, originY);

            return origin + indexOffset;
        }
        
        private WorldCellIndex[] GetIndicesInWorldDir(CVector2 worldDir, int originX, int originY)
        {
            float tilemapAngle = _eulerZTest;
            CVector2Int origin = new CVector2Int(originX, originY);
            
            float worldAngle = CMath.DirectionToDegAngle(worldDir) - tilemapAngle;
            CVector2 unroundedOffset = CMath.DegAngleToDirection(worldAngle);
            float UpperOffsetAngle = worldAngle + 45f;
            float lowerOffsetAngle = worldAngle - 45f;
            
            CalculateIndexOffset(worldAngle, unroundedOffset, out CVector2Int intOffset, out float dot);
            CalculateIndexOffset(UpperOffsetAngle, unroundedOffset, out CVector2Int UpperIntOffset, out float UpperDot);
            CalculateIndexOffset(lowerOffsetAngle, unroundedOffset, out CVector2Int lowerIntOffset, out float lowerDot);

            CVector2Int closestOffset = UpperDot > lowerDot ? UpperIntOffset : lowerIntOffset;
            float closestDot = UpperDot > lowerDot ? UpperDot : lowerDot;

            float sum = CMath.Clamp01(dot) + CMath.Clamp01(closestDot);

            dot /= sum;
            closestDot /= sum;

            return new[]
            {
                new WorldCellIndex(origin + intOffset, dot),
                new WorldCellIndex(origin + closestOffset, closestDot)
            };
        }

        private float _remapScale = .666f;
        private void CalculateIndexOffset(float angle, CVector2 unroundedOffset, out CVector2Int intOffset, out float dot)
        {
            CVector2 offset = CMath.DegAngleToDirection(CMath.RoundToNumber(angle, 45f));
            intOffset = new CVector2Int(
                CMath.RoundToInt(offset.x),
                CMath.RoundToInt(offset.y));
            dot = CVector2.Dot(offset, unroundedOffset).Remap01(_remapScale, 1f);
        }

        public readonly struct WorldCellIndex
        {
            public readonly CVector2Int index;
            public readonly float transferMultiplier;
            
            public WorldCellIndex(CVector2Int index, float transferMultiplier)
            {
                this.index = index;
                this.transferMultiplier = transferMultiplier;
            }
        }

        private void DrawDirectionTestGizmos()
        {
            CVector2 offset = new CVector2(-1f, -1f);
            
            CVector2 worldDir = CVector2.Up;
            CVector2Int dirIndex = GetIndexInWorldDir(worldDir, 1, 1);
            
            Gizmos.color = Color.Green;
            Gizmos.DrawLine(CVector2.Zero, worldDir * 2f);

            Gizmos.matrix = Matrix.CreateRotationZ(_eulerZTest);

            WorldCellIndex[] cellIndices = GetIndicesInWorldDir(worldDir, 1, 1);
            
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Gizmos.color = new Color(1f, 0f, 0f,.5f);
                    Gizmos.DrawRectangle(new CVector2(x, y) + offset, CVector2.One);
                    
                    for (int i = 0; i < cellIndices.Length; i++)
                    {
                        if (x == cellIndices[i].index.x && y == cellIndices[i].index.y)
                        {
                            Gizmos.color = new Color(0f, 0f, 1f, .5f * cellIndices[i].transferMultiplier);
                            Gizmos.DrawRectangle(new CVector2(x, y) + offset, CVector2.One);
                            break;
                        }
                    }
                }
            }
        }

        private void DrawTilemapGizmos()
        {
            if (_structure == null) return;

            Gizmos.matrix = _structure.TranslationMatrix;

            if (_waterSim == null) return;
            
            Gizmos.StartGizmoBatch();
            
            Gizmos.color = Color.Yellow;
            Gizmos.DrawWireRectangle(_waterSim.Bounds.Center, _waterSim.Bounds.Size, true);

            CVector2 simOrigin = _waterSim.Bounds.Center - _waterSim.Bounds.Size / 2f + new CVector2(.5f, .5f);

            for (int x = 0; x < _waterSim.Size.x; x++)
            {
                for (int y = 0; y < _waterSim.Size.y; y++)
                {
                    CVector2 offset = new CVector2(x, y);

                    if (!_waterSim.TryGetCellAtIndex(x, y, out WaterCell cell)) continue;

                    if (cell.IsWall)
                    {
                        Color wallCol = new Color(1f, 0f, 0f, .5f);

                        Gizmos.color = wallCol;
                        Gizmos.DrawRectangle(simOrigin + offset, CVector2.One, true);
                        continue;
                    }

                    Color col = new Color(0f, 0f, 0f, 0f);

                    if (cell.Value >= WaterSimulation.MIN_WATER_PER_CELL)
                    {
                        col = new Color(0f, .25f, 1f, cell.Value / cell.MaxValue);

                        if (cell.Value > cell.MaxValue)
                            col = Color.Lerp(col, new Color(0f, .2f, .4f, col.A / 255f),
                                (cell.Value - cell.MaxValue) / 2f);
                    }

                    Gizmos.color = col;
                    Gizmos.DrawRectangle(simOrigin + offset, CVector2.One, true);
                }
            }
            
            Gizmos.EndGizmoBatch();
        }
    }
}