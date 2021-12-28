using System;
using System.Collections.Generic;
using System.Linq;
using AbyssEngine;
using AbyssEngine.CustomMath;

namespace LightlessAbyss.Dev
{
    public sealed class WaterSimulation
    {
        public const float MAX_WATER_PER_CELL = 1f;
        public const float MIN_WATER_PER_CELL = .001f;
        private const float MAX_TRANSFER_AMOUNT = 50f;
        private const float MAX_COMPRESSION = .01f;

        public BoundsInt Bounds => _bounds;
        public CVector2Int Size => _bounds.IntSize;

        private BoundsInt _bounds;
        private WaterCell[,] _cells;
        private Structure _attachedStructure;
        
        public WaterSimulation(Structure attachedStructure)
        {
            _attachedStructure = attachedStructure;
            
            _bounds = _attachedStructure.Bounds;
            
            _cells = new WaterCell[Size.x, Size.y];
            
            InitializeNullCells();
            UpdateWallsFromStructureData();
        }
        
        public void Step(int numTimes = 1)
        {
            for (int i = 0; i < numTimes; i++)
                UpdateCells();
        }
        
        public void ResizeToStructureBounds()
        {
            _bounds = _attachedStructure.Bounds;
            _cells = _cells.ResizeArray2D(_bounds.IntSize.x, _bounds.IntSize.y);
            InitializeNullCells();
            UpdateWallsFromStructureData();
        }

        public void ModifyWaterAtWorldPos(CVector2 worldPos, float modification)
        {
            if (!TryGetValidCellIndexFromWorld(worldPos, out int x, out int y)) return;

            _cells[x, y].Value += modification;
        }

        public bool TryGetCellAtIndex(int x, int y, out WaterCell cell)
        {
            if (!IsIndexInBounds(x, y))
            {
                cell = null;
                return false;
            }

            cell = _cells[x, y];
            return true;
        }

        public void UpdateWallsFromStructureData()
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    CVector2Int cellPos = new CVector2Int(x, y);
                    CVector2Int tilePos = CellToTile(cellPos);

                    _cells[x, y].IsWall = 
                        _attachedStructure.TryGetTileAtTilePos(tilePos, out StructureTile tile) && 
                        tile.IsWaterWall;
                }
            }
        }

        public CVector2Int WorldToCell(CVector2 worldPos)
        {
            return _attachedStructure.WorldToTile(worldPos) - Bounds.Min;
        }

        public CVector2 CellToWorld(CVector2Int cellPos)
        {
            return _attachedStructure.TileToWorld(cellPos) + Bounds.Min;
        }

        public CVector2 CellToWorld(int cellX, int cellY)
        {
            return CellToWorld(new CVector2Int(cellX, cellY));
        }
        
        public CVector2Int CellToTile(CVector2Int cellPos)
        {
            return cellPos + _attachedStructure.Bounds.Min;
        }

        public CVector2Int CellToTile(int cellX, int cellY)
        {
            return CellToTile(new CVector2Int(cellX, cellY));
        }

        private bool TryGetValidCellIndexFromWorld(CVector2 worldPos, out int x, out int y)
        {
            CVector2Int index = WorldToCell(worldPos);

            if (!IsIndexInBounds(index.x, index.y) || _cells[index.x, index.y].IsWall)
            {
                x = 0;
                y = 0;
                return false;
            }

            x = index.x;
            y = index.y;
            return true;
        }
        
        private void UpdateCells()
        {
            foreach (WaterCell cell in GetOrderedCellsForUpdate())
            {
                if (!ValidateCellValue(cell)) continue;
                DistributeDown(cell, cell.x, cell.y);
                if (!ValidateCellValue(cell)) continue;
                DistributeRight(cell, cell.x, cell.y );
                if (!ValidateCellValue(cell)) continue;
                DistributeLeft(cell, cell.x, cell.y);
                if (!ValidateCellValue(cell)) continue;
                DistributeUp(cell, cell.x, cell.y);
            }
        }

        private List<WaterCell> GetOrderedCellsForUpdate()
        {
            List<WaterCell> waterCells = new List<WaterCell>();

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    WaterCell cell = _cells[x, y];
                    
                    if (cell.Value > MIN_WATER_PER_CELL)
                        waterCells.Add(cell);
                }
            }

            return waterCells.OrderBy(WorldYOfCell).ThenBy(WorldXOfCell).ToList();
        }

        private float WorldYOfCell(WaterCell cell)
        {
            CVector2 worldPos = CellToWorld(cell.x, cell.y);
            return worldPos.y;
        }
        
        private float WorldXOfCell(WaterCell cell)
        {
            CVector2 worldPos = CellToWorld(cell.x, cell.y);
            return worldPos.x;
        }
        
        private WorldCellIndex[] GetIndicesInWorldDir(CVector2 worldDir, int originX, int originY)
        {
            float structureAngle = _attachedStructure.Rotation;
            CVector2Int origin = new CVector2Int(originX, originY);
            
            float worldAngle = CMath.DirectionToDegAngle(worldDir) - structureAngle;
            CVector2 unroundedOffset = CMath.DegAngleToDirection(worldAngle);
            float upperOffsetAngle = worldAngle + 45f;
            float lowerOffsetAngle = worldAngle - 45f;
            
            CalculateIndexOffset(worldAngle, unroundedOffset, out CVector2Int intOffset, out float dot);
            CalculateIndexOffset(upperOffsetAngle, unroundedOffset, out CVector2Int upperIntOffset, out float upperDot);
            CalculateIndexOffset(lowerOffsetAngle, unroundedOffset, out CVector2Int lowerIntOffset, out float lowerDot);

            CVector2Int closestOffset = upperDot > lowerDot ? upperIntOffset : lowerIntOffset;
            float closestDot = upperDot > lowerDot ? upperDot : lowerDot;

            float sum = CMath.Clamp01(dot) + CMath.Clamp01(closestDot);

            dot /= sum;
            closestDot /= sum;

            return new[]
            {
                new WorldCellIndex(origin + intOffset, dot),
                new WorldCellIndex(origin + closestOffset, closestDot)
            };
        }

        private static void CalculateIndexOffset(float angle, CVector2 unroundedOffset, out CVector2Int intOffset, out float dot)
        {
            CVector2 offset = CMath.DegAngleToDirection(CMath.RoundToNumber(angle, 45f));
            intOffset = new CVector2Int(
                CMath.RoundToInt(offset.x),
                CMath.RoundToInt(offset.y));
            const float remap_scale = .71f;
            dot = CVector2.Dot(offset, unroundedOffset).Remap01(remap_scale, 1f);
        }

        private void DistributeDown(WaterCell cell, int cellX, int cellY)
        {
            WorldCellIndex[] downIndices = GetIndicesInWorldDir(CVector2.Down, cellX, cellY);
            
            for (int i = 0; i < downIndices.Length; i++)
            {
                CVector2Int downIndex = downIndices[i].index;

                if (!IsIndexInBounds(downIndex.x, downIndex.y)) continue;

                WaterCell downCell = _cells[downIndex.x, downIndex.y];

                if (!CellCanHoldWater(downCell)) continue;
            
                if (!IsDiagonalAndCanTransfer(cellX, cellY, downIndex)) continue;
                
                float transferAmount = CalculateVerticalTransferAmount(cell, downCell) - downCell.Value;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
                
                if (transferAmount <= 0f) continue;
                
                cell.TransferTo(downCell, transferAmount * downIndices[i].transferMultiplier);
            }
        }

        private void DistributeRight(WaterCell cell, int cellX, int cellY)
        {
            WorldCellIndex[] rightIndices = GetIndicesInWorldDir(CVector2.Right, cellX, cellY);
            
            for (int i = 0; i < rightIndices.Length; i++)
            {
                CVector2Int rightIndex = rightIndices[i].index;

                if (!IsIndexInBounds(rightIndex.x, rightIndex.y)) continue;

                WaterCell rightCell = _cells[rightIndex.x, rightIndex.y];

                if (!CellCanHoldWater(rightCell)) continue;
                
                if (!IsDiagonalAndCanTransfer(cellX, cellY, rightIndex)) continue;

                float transferAmount = (cell.Value - rightCell.Value) / 2f;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
            
                if (transferAmount <= 0f) continue;

                cell.TransferTo(rightCell, transferAmount * rightIndices[i].transferMultiplier);
            }
        }

        private void DistributeLeft(WaterCell cell, int cellX, int cellY)
        {
            WorldCellIndex[] leftIndices = GetIndicesInWorldDir(CVector2.Left, cellX, cellY);
            
            for (int i = 0; i < leftIndices.Length; i++)
            {
                CVector2Int leftIndex = leftIndices[i].index;

                if (!IsIndexInBounds(leftIndex.x, leftIndex.y)) continue;

                WaterCell leftCell = _cells[leftIndex.x, leftIndex.y];

                if (!CellCanHoldWater(leftCell)) continue;
                
                if (!IsDiagonalAndCanTransfer(cellX, cellY, leftIndex)) continue;
                
                float transferAmount = (cell.Value - leftCell.Value) / 2f;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
            
                if (transferAmount <= 0f) continue;
                
                cell.TransferTo(leftCell, transferAmount * leftIndices[i].transferMultiplier);
            }
        }

        private void DistributeUp(WaterCell cell, int cellX, int cellY)
        {
            if (cell.Value <= cell.MaxValue) return;

            WorldCellIndex[] upIndices = GetIndicesInWorldDir(CVector2.Up, cellX, cellY);
            
            for (int i = 0; i < upIndices.Length; i++)
            {
                CVector2Int upIndex = upIndices[i].index;

                if (!IsIndexInBounds(upIndex.x, upIndex.y)) continue;

                WaterCell upCell = _cells[upIndex.x, upIndex.y];

                if (!CellCanHoldWater(upCell)) continue;

                if (!IsDiagonalAndCanTransfer(cellX, cellY, upIndex)) continue;

                float transferAmount = cell.Value - CalculateVerticalTransferAmount(cell, upCell);
                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
                
                if (transferAmount <= 0f) continue;

                cell.TransferTo(upCell, transferAmount * upIndices[i].transferMultiplier);
            }
        }

        private bool IsDiagonalAndCanTransfer(int cellX, int cellY, CVector2Int destIndex)
        {
            CVector2Int localIndexOffset = OffsetFromOrigin(cellX, cellY, destIndex);

            return !IsDiagonalIndexOffset(localIndexOffset) || IsValidDiagonal(cellX, cellY, localIndexOffset);
        }

        private CVector2Int OffsetFromOrigin(int originX, int originY, CVector2Int index)
        {
            return new CVector2Int(index.x - originX, index.y - originY);
        }

        private bool IsDiagonalIndexOffset(CVector2Int offset)
        {
            return Math.Abs(offset.x) == 1 && Math.Abs(offset.y) == 1;
        }

        private bool IsValidDiagonal(int originX, int originY, CVector2Int diagonalOffset)
        {
            CVector2Int xCardinal = new CVector2Int(originX + diagonalOffset.x, originY);
            CVector2Int yCardinal = new CVector2Int(originX, originY + diagonalOffset.y);

            return IndexCanHoldWater(xCardinal) && IndexCanHoldWater(yCardinal);
        }

        private float CalculateVerticalTransferAmount(WaterCell src, WaterCell dest)
        {
            float sum = src.Value + dest.Value;
            float value;
            
            if (sum <= dest.MaxValue)
                value = dest.MaxValue;
            else if (sum < dest.MaxValue * 2f + MAX_COMPRESSION)
                value = (dest.MaxValue * dest.MaxValue + sum * MAX_COMPRESSION) / (dest.MaxValue + MAX_COMPRESSION);
            else
                value = (sum + MAX_COMPRESSION) / 2f;

            return value;
        }

        private bool ValidateCellValue(WaterCell cell)
        {
            if (!CellHasWater(cell))
            {
                cell.Value = 0f;
                return false;
            }

            return true;
        }

        private bool IndexCanHoldWater(CVector2Int index)
        {
            return IsIndexInBounds(index) && CellCanHoldWater(_cells[index.x, index.y]);
        }

        private bool CellCanHoldWater(WaterCell cell)
        {
            return !cell.IsWall;
        }

        private bool IsIndexInBounds(CVector2Int index)
        {
            return IsIndexInBounds(index.x, index.y);
        }

        private bool IsIndexInBounds(int x, int y)
        {
            return x >= 0 && x < Size.x && y >= 0 && y < Size.y;
        }

        private bool CellHasWater(WaterCell cell)
        {
            return cell.Value >= MIN_WATER_PER_CELL;
        }

        private void InitializeNullCells()
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    _cells[x, y] ??= new WaterCell(x, y);
                }
            }
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
    }
}