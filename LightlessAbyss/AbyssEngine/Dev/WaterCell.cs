using System;

namespace LightlessAbyss.AbyssEngine.Dev
{
    public sealed class WaterCell
    {
        public readonly int x;
        public readonly int y;
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

        public WaterCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public void TransferTo(WaterCell dest, float amount)
        {
            Value -= amount;
            dest.Value += amount;
        }
    }
}