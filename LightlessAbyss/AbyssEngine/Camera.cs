using System.Drawing;
using AbyssEngine.Backend.Rendering;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;

namespace AbyssEngine
{
    public class Camera
    {
        public static Camera Main { get; private set; }
        
        public CVector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                _isDirty = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _isDirty = true;
            }
        }

        public float OrthographicSize
        {
            get => _orthographicSize;
            set
            {
                _orthographicSize = value;
                _isDirty = true;
            }
        }

        public Matrix Matrix
        {
            get
            {
                if (_isDirty)
                    UpdateMatrix();
                return _matrix;
            }
        }

        public RectangleF ViewRect
        {
            get
            {
                float height = _orthographicSize * 2f;
                float width = height * EngineRenderer.RawGameViewport.AspectRatio;
                return new RectangleF(_position.x, _position.y, width, height);
            }
        }

        public bool SnapToPPU { get; set; }

        private CVector2 ScreenCenterOffset => EngineRenderer.RawGameViewport.Bounds.Size.ToVector2() / 2f;
        private CVector2 WorldCenterOffset => EngineRenderer.PixelToWorld(ScreenCenterOffset);

        private CVector2 _position;
        private float _rotation;
        private float _orthographicSize;
        private Matrix _matrix;
        private bool _isDirty;

        public Camera(bool newMainCamera = true)
        {
            Position = CVector2.Zero;
            Rotation = 0f;
            OrthographicSize = 1f;
            _isDirty = true;

            if (newMainCamera)
                Main = this;
        }

        public CVector2 ScreenToWorld(CVector2 screenPos)
        {
            return EngineRenderer.RenderMatrix.InverseMultiplyPoint(ClampInScreen(RemapDisplayPosToRaw(screenPos)));
        }
        
        public CVector2 WorldToScreen(CVector2 worldPos)
        {
            return ClampInScreen(RemapDisplayPosToRaw(EngineRenderer.RenderMatrix.MultiplyPoint(worldPos)));
        }

        private CVector2 RemapDisplayPosToRaw(CVector2 screenPos)
        {
            CVector2 size = EngineRenderer.WindowRect.Size;
            CVector2 offset = EngineRenderer.DisplayedGameOffset;
            CVector2 uv = (screenPos - offset) / (size - offset * 2f);

            return new CVector2(EngineRenderer.RawGameRect.Size) * uv;
        }

        private CVector2 ClampInScreen(CVector2 screenPos)
        {
            return CMathUtils.Clamp(screenPos, CVector2.Zero, EngineRenderer.RawGameViewport.Bounds.Size);
        }

        private void UpdateMatrix()
        {
            CVector2 pos = _position;
            CVector2 centerOffset = WorldCenterOffset;

            float viewScale = GetViewScale();

            if (SnapToPPU)
            {
                pos = CMathUtils.SnapToPPU(pos, EngineRenderer.PPU);
                centerOffset = CMathUtils.SnapToPPU(centerOffset, EngineRenderer.PPU);
            }

            _matrix = Matrix.CreateTranslation(-pos.x, -pos.y, 0f) *
                      Matrix.CreateRotationZ(_rotation * CMathUtils.DEG2RAD) *
                      Matrix.CreateScale(viewScale, viewScale, 1f) *
                      Matrix.CreateTranslation(centerOffset.x, centerOffset.y, 0f);

            _isDirty = false;
        }

        private float GetViewScale()
        {
            float desiredVerticalSizeInPixels = _orthographicSize * EngineRenderer.PPU;
            float verticalExtents = EngineRenderer.RawGameViewport.Height / 2f;
            float scaleFactor = verticalExtents / desiredVerticalSizeInPixels;

            return scaleFactor;
        }
    }
}