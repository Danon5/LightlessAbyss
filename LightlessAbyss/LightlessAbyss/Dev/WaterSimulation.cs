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
        private const float MAX_COMPRESSION = .1f;

        public BoundsInt Bounds => _bounds;
        public CVector2Int Size => _bounds.IntSize;
        public Dictionary<CVector2Int, WaterCell> Cells => _cells;

        private BoundsInt _bounds;
        private readonly Dictionary<CVector2Int, WaterCell> _cells;
        private readonly Structure _attachedStructure;
        
        public WaterSimulation(Structure attachedStructure)
        {
            _attachedStructure = attachedStructure;
            
            _bounds = _attachedStructure.Bounds;

            _cells = new Dictionary<CVector2Int, WaterCell>();
            
            ResizeToStructureBounds();
        }
        
        public void Step(int numTimes = 1)
        {
            for (int i = 0; i < numTimes; i++)
                UpdateCells();
        }
        
        public void ResizeToStructureBounds()
        {
            _bounds = _attachedStructure.Bounds;
            
            Stack<CVector2Int> cellsToRemove = new Stack<CVector2Int>();

            foreach ((CVector2Int cellPos, WaterCell cell) in _cells)
            {
                if (!_bounds.Contains(cellPos))
                    cellsToRemove.Push(cellPos);
            }

            while (cellsToRemove.TryPop(out CVector2Int cell))
                _cells.Remove(cell);

            for (int x = _bounds.Left; x < _bounds.Right; x++)
            {
                for (int y = _bounds.Bottom; y < _bounds.Top; y++)
                {
                    CVector2Int cellPos = new CVector2Int(x, y);
                    
                    if (!_cells.ContainsKey(cellPos))
                        _cells.Add(cellPos, new WaterCell(cellPos));
                }
            }
            
            UpdateWallsFromStructureData();
        }

        public void ModifyWaterAtWorldPos(CVector2 worldPos, float modification)
        {
            if (!TryGetValidCellPosFromWorld(worldPos, out CVector2Int cellPos)) return;

            _cells[cellPos].Value += modification;
        }

        public bool TryGetCellAtCellPos(CVector2Int cellPos, out WaterCell cell)
        {
            if (!_cells.ContainsKey(cellPos))
            {
                cell = null;
                return false;
            }

            cell = _cells[cellPos];
            return true;
        }

        public void UpdateWallsFromStructureData()
        {
            foreach ((CVector2Int cellPos, WaterCell cell) in _cells)
            {
                CVector2Int tilePos = CellToTile(cellPos);
                cell.IsWall = _attachedStructure.TryGetTileAtTilePos(tilePos, out StructureTile tile) && tile.IsWaterWall;
            }
        }

        public CVector2Int WorldToCell(CVector2 worldPos)
        {
            return _attachedStructure.WorldToTile(worldPos);
        }

        public CVector2 CellToWorld(CVector2Int cellPos)
        {
            return _attachedStructure.TileToWorld(cellPos);
        }

        public CVector2Int CellToTile(CVector2Int cellPos)
        {
            return cellPos;
        }

        private bool TryGetValidCellPosFromWorld(CVector2 worldPos, out CVector2Int cellPos)
        {
            cellPos = WorldToCell(worldPos);
            return _cells.ContainsKey(cellPos) && !_cells[cellPos].IsWall;
        }
        
        private void UpdateCells()
        {
            foreach (WaterCell cell in GetOrderedCellsForUpdate())
            {
                if (!ValidateCellValue(cell)) continue;
                DistributeDown(cell);
                if (!ValidateCellValue(cell)) continue;
                DistributeRight(cell);
                if (!ValidateCellValue(cell)) continue;
                DistributeLeft(cell);
                if (!ValidateCellValue(cell)) continue;
                DistributeUp(cell);
            }
        }

        private List<WaterCell> GetOrderedCellsForUpdate()
        {
            List<WaterCell> waterCells = new List<WaterCell>();

            foreach ((CVector2Int cellPos, WaterCell cell) in _cells)
            {
                if (cell.Value > MIN_WATER_PER_CELL)
                    waterCells.Add(cell);
            }

            return waterCells.OrderBy(WorldYOfCell).ThenBy(WorldXOfCell).ToList();
        }

        private float WorldYOfCell(WaterCell cell)
        {
            CVector2 worldPos = CellToWorld(cell.cellPos);
            return worldPos.y;
        }
        
        private float WorldXOfCell(WaterCell cell)
        {
            CVector2 worldPos = CellToWorld(cell.cellPos);
            return worldPos.x;
        }
        
        private WorldCellPos[] GetIndicesInWorldDir(CVector2 worldDir, CVector2Int origin)
        {
            float structureAngle = _attachedStructure.Rotation;

            float worldAngle = CMath.DirectionToDegAngle(worldDir) - structureAngle;
            CVector2 unroundedOffset = CMath.DegAngleToDirection(worldAngle);
            float upperOffsetAngle = worldAngle + 45f;
            float lowerOffsetAngle = worldAngle - 45f;
            
            CalculateCellPosOffset(worldAngle, unroundedOffset, out CVector2Int intOffset, out float dot);
            CalculateCellPosOffset(upperOffsetAngle, unroundedOffset, out CVector2Int upperIntOffset, out float upperDot);
            CalculateCellPosOffset(lowerOffsetAngle, unroundedOffset, out CVector2Int lowerIntOffset, out float lowerDot);

            CVector2Int closestOffset = upperDot > lowerDot ? upperIntOffset : lowerIntOffset;
            float closestDot = upperDot > lowerDot ? upperDot : lowerDot;

            float sum = CMath.Clamp01(dot) + CMath.Clamp01(closestDot);

            dot /= sum;
            closestDot /= sum;

            return new[]
            {
                new WorldCellPos(origin + intOffset, dot),
                new WorldCellPos(origin + closestOffset, closestDot)
            };
        }

        private static void CalculateCellPosOffset(float angle, CVector2 unroundedOffset, out CVector2Int intOffset, out float dot)
        {
            CVector2 offset = CMath.DegAngleToDirection(CMath.RoundToNumber(angle, 45f));
            intOffset = new CVector2Int(
                CMath.RoundToInt(offset.x),
                CMath.RoundToInt(offset.y));
            const float remap_scale = .71f;
            dot = CVector2.Dot(offset, unroundedOffset).Remap01(remap_scale, 1f);
        }

        private void DistributeDown(WaterCell cell)
        {
            WorldCellPos[] downPositions = GetIndicesInWorldDir(CVector2.Down, cell.cellPos);
            
            for (int i = 0; i < downPositions.Length; i++)
            {
                CVector2Int downPos = downPositions[i].cellPos;

                if (!_cells.ContainsKey(downPos)) continue;

                WaterCell downCell = _cells[downPos];

                if (!CellCanHoldWater(downCell)) continue;
            
                if (!IsDiagonalAndCanTransfer(cell.cellPos, downPos)) continue;
                
                float transferAmount = CalculateVerticalTransferAmount(cell, downCell) - downCell.Value;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
                
                if (transferAmount <= 0f) continue;
                
                cell.TransferTo(downCell, transferAmount * downPositions[i].transferMultiplier);
            }
        }

        private void DistributeRight(WaterCell cell)
        {
            WorldCellPos[] rightPositions = GetIndicesInWorldDir(CVector2.Right, cell.cellPos);
            
            for (int i = 0; i < rightPositions.Length; i++)
            {
                CVector2Int rightPos = rightPositions[i].cellPos;

                if (!_cells.ContainsKey(rightPos)) continue;

                WaterCell rightCell = _cells[rightPos];

                if (!CellCanHoldWater(rightCell)) continue;
                
                if (!IsDiagonalAndCanTransfer(cell.cellPos, rightPos)) continue;

                float transferAmount = (cell.Value - rightCell.Value) / 2f;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
            
                if (transferAmount <= 0f) continue;

                cell.TransferTo(rightCell, transferAmount * rightPositions[i].transferMultiplier);
            }
        }

        private void DistributeLeft(WaterCell cell)
        {
            WorldCellPos[] leftPositions = GetIndicesInWorldDir(CVector2.Left, cell.cellPos);
            
            for (int i = 0; i < leftPositions.Length; i++)
            {
                CVector2Int leftPos = leftPositions[i].cellPos;

                if (!_cells.ContainsKey(leftPos)) continue;

                WaterCell leftCell = _cells[leftPos];

                if (!CellCanHoldWater(leftCell)) continue;
                
                if (!IsDiagonalAndCanTransfer(cell.cellPos, leftPos)) continue;
                
                float transferAmount = (cell.Value - leftCell.Value) / 2f;

                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
            
                if (transferAmount <= 0f) continue;
                
                cell.TransferTo(leftCell, transferAmount * leftPositions[i].transferMultiplier);
            }
        }

        private void DistributeUp(WaterCell cell)
        {
            if (cell.Value <= cell.MaxValue) return;

            WorldCellPos[] upPositions = GetIndicesInWorldDir(CVector2.Up, cell.cellPos);
            
            for (int i = 0; i < upPositions.Length; i++)
            {
                CVector2Int upPos = upPositions[i].cellPos;

                if (!_cells.ContainsKey(upPos)) continue;

                WaterCell upCell = _cells[upPos];

                if (!CellCanHoldWater(upCell)) continue;

                if (!IsDiagonalAndCanTransfer(cell.cellPos, upPos)) continue;

                float transferAmount = cell.Value - CalculateVerticalTransferAmount(cell, upCell);
                transferAmount = MathF.Max(transferAmount, 0f);
                
                if (transferAmount > MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value))
                    transferAmount = MathF.Min(MAX_TRANSFER_AMOUNT, cell.Value);
                
                if (transferAmount <= 0f) continue;

                cell.TransferTo(upCell, transferAmount * upPositions[i].transferMultiplier);
            }
        }

        private bool IsDiagonalAndCanTransfer(CVector2Int srcCellPos, CVector2Int destCellPos)
        {
            CVector2Int localCellPosOffset = OffsetFromOrigin(srcCellPos, destCellPos);

            return !IsDiagonalCellPosOffset(localCellPosOffset) || IsValidDiagonal(srcCellPos, localCellPosOffset);
        }

        private CVector2Int OffsetFromOrigin(CVector2Int origin, CVector2Int cellPos)
        {
            return cellPos - origin;
        }

        private bool IsDiagonalCellPosOffset(CVector2Int offset)
        {
            return Math.Abs(offset.x) == 1 && Math.Abs(offset.y) == 1;
        }

        private bool IsValidDiagonal(CVector2Int origin, CVector2Int diagonalOffset)
        {
            CVector2Int xCardinal = new CVector2Int(origin.x + diagonalOffset.x, origin.y);
            CVector2Int yCardinal = new CVector2Int(origin.x, origin.y + diagonalOffset.y);

            return CellPosCanHoldWater(xCardinal) && CellPosCanHoldWater(yCardinal);
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

        private bool CellPosCanHoldWater(CVector2Int cellPos)
        {
            return _cells.ContainsKey(cellPos) && CellCanHoldWater(_cells[cellPos]);
        }

        private bool CellCanHoldWater(WaterCell cell)
        {
            return !cell.IsWall;
        }

        private bool CellHasWater(WaterCell cell)
        {
            return cell.Value >= MIN_WATER_PER_CELL;
        }

        public readonly struct WorldCellPos
        {
            public readonly CVector2Int cellPos;
            public readonly float transferMultiplier;
            
            public WorldCellPos(CVector2Int cellPos, float transferMultiplier)
            {
                this.cellPos = cellPos;
                this.transferMultiplier = transferMultiplier;
            }
        }
    }
}