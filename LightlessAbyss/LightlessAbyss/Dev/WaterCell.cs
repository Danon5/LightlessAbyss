using System;
using AbyssEngine.CustomMath;

namespace LightlessAbyss.Dev
{
    public sealed class WaterCell
    {
        public readonly CVector2Int cellPos;
        
        public float Value
        {
            get => _value;
            set => _value = MathF.Max(value, 0f);
        }

        public float MaxValue
        {
            get => _maxValue;
            set => _maxValue = MathF.Max(value, 0f);
        }

        public bool IsWall { get; set; }
            
        private float _value;
        private float _maxValue = WaterSimulation.MAX_WATER_PER_CELL;

        public WaterCell(CVector2Int cellPos)
        {
            this.cellPos = cellPos;
        }
        
        public void TransferTo(WaterCell dest, float amount)
        {
            Value -= amount;
            dest.Value += amount;
        }
    }
}