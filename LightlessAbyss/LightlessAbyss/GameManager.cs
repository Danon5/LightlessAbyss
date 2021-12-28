using AbyssEngine;
using AbyssEngine.CustomMath;
using AbyssEngine.DebugUtils;
using LightlessAbyss.Dev;
using Microsoft.Xna.Framework;

namespace LightlessAbyss
{
    public sealed class GameManager : Behaviour
    {
        private float _desiredOrthographicSize = 1f;
        private CVector2 _camVel = CVector2.Zero;

        private DevStructureBuilder _devStructureBuilder;
        private DevWaterSimulator _devWaterSimulator;
        
        public override void Initialize()
        {
            base.Initialize();
            
            //_devStructureBuilder = new DevStructureBuilder();
            _devWaterSimulator = new DevWaterSimulator();
        }

        public override void Tick()
        {
            base.Tick();
            
            _desiredOrthographicSize -= Controls.MouseScrollDelta * .1f * _desiredOrthographicSize;
            _desiredOrthographicSize = CMath.Clamp(_desiredOrthographicSize, .01f, 25f);
            Camera.Main.OrthographicSize = 
                CMath.Lerp(Camera.Main.OrthographicSize, _desiredOrthographicSize, 15f * Time.DeltaTime);

            CVector2 axis = new CVector2();

            if (Controls.Up.IsHeld)
                axis.y += 1;
            if (Controls.Down.IsHeld)
                axis.y -= 1;
            if (Controls.Right.IsHeld)
                axis.x += 1;
            if (Controls.Left.IsHeld)
                axis.x -= 1;

            axis = axis.Normalized;

            CVector2 desiredVel = CVector2.Zero;

            if (axis.Magnitude > 0f)
                desiredVel = axis * (Controls.Sprint.IsHeld ? 5f : 2.5f);
            
            _camVel = CVector2.Lerp(_camVel, desiredVel, 15f * Time.DeltaTime);

            Camera.Main.Position += _camVel * (Camera.Main.OrthographicSize * Time.DeltaTime);
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            
            Gizmos.matrix = Matrix.Identity;
            Gizmos.color = new Color(1f, 1f, 1f, .05f);
            CVector2 pos = Camera.Main.Position;
            Gizmos.DrawWireGrid(new CVector2((int)pos.x, (int)pos.y), 100, 76);
        }
    }
}