using System;
using AbyssEngine.CustomColor;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbyssEngine.Backend.Rendering
{
    public sealed class PolygonRenderer : IDisposable
    {
        public static bool BatchingInProgress => _batchingInProgress;
        
        private const int MAX_VERTEX_BUFFER_SIZE = 512;
        private const int MAX_INDEX_BUFFER_SIZE = MAX_VERTEX_BUFFER_SIZE * 3;
        
        private static PolygonRenderer _singleton;

        private static GraphicsDevice _graphicsDevice;
        private static VertexPositionColor[] _vertexBuffer;
        private static int[] _indexBuffer;
        private static int _vertCount;
        private static int _indexCount;
        private static BasicEffect _shader;
        private static bool _batchingInProgress;
        private static bool _isScreenSpaceBatch;
        
        public PolygonRenderer(GraphicsDevice graphicsDevice)
        {
            if (_singleton != null)
                throw new Exception("Attempting to create multiple PolygonRenderers. This is not allowed.");
            
            _singleton = this;

            _graphicsDevice = graphicsDevice;
            _vertexBuffer = new VertexPositionColor[MAX_VERTEX_BUFFER_SIZE];
            _indexBuffer = new int[MAX_INDEX_BUFFER_SIZE];
            
            _shader = new BasicEffect(graphicsDevice)
            {
                FogEnabled = false,
                LightingEnabled = false,
                TextureEnabled = false,
                VertexColorEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = Matrix.Identity
            };
        }

        public static void BeginBatch(Matrix? matrix = null, bool screenSpace = false)
        {
            if (_batchingInProgress)
                throw new Exception("Cannot start batch with batch already started!");

            _vertCount = 0;
            _indexCount = 0;

            matrix ??= Matrix.Identity;
            
            Matrix renderMatrix = screenSpace ? EngineRenderer.PolygonScreenMatrix : EngineRenderer.CameraMatrix;
            
            InitializeViewProjectionMatrix(matrix * renderMatrix);
            _isScreenSpaceBatch = screenSpace;
            
            _batchingInProgress = true;
        }

        public static void BatchDrawLine(CVector2 point1, CVector2 point2, CColor? color = null, float lineWidth = .01f)
        {
            if (!_batchingInProgress)
                throw new Exception("Cannot batch draw with no batch started!");

            EnsureBufferSpaceForPolygon(4, 6);

            if (_isScreenSpaceBatch)
            {
                point1 = ToScreenVector(point1);
                point2 = ToScreenVector(point2);
            }
            
            color ??= CColor.White;

            CVector2 dir = point1 - point2;
            float xExtents = dir.Magnitude / 2f;
            float yExtents = lineWidth / 2f;
            dir = dir.Normalized;
            CVector2 normal = new CVector2(-dir.y, dir.x);
            CVector2 center = (point1 + point2) / 2f;

            CVector2 botLeft = center - dir * xExtents - normal * yExtents;
            CVector2 topLeft = center - dir * xExtents + normal * yExtents;
            CVector2 topRight = center + dir * xExtents + normal * yExtents;
            CVector2 botRight = center + dir * xExtents - normal * yExtents;
            
            _indexBuffer[_indexCount++] = _vertCount + 0;
            _indexBuffer[_indexCount++] = _vertCount + 1;
            _indexBuffer[_indexCount++] = _vertCount + 3;
            
            _indexBuffer[_indexCount++] = _vertCount + 1;
            _indexBuffer[_indexCount++] = _vertCount + 2;
            _indexBuffer[_indexCount++] = _vertCount + 3;
            
            _vertexBuffer[_vertCount++] = new VertexPositionColor(botLeft, (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(topLeft, (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(topRight, (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(botRight, (CColor)color);
        }

        public static void BatchDrawWireRectangle(CVector2 pos, CVector2 size, CColor? color = null, float lineWidth = .01f)
        {
            if (!_batchingInProgress)
                throw new Exception("Cannot batch draw with no batch started!");
            
            if (_isScreenSpaceBatch)
            {
                pos = ToScreenVector(pos);
            }
            
            color ??= CColor.White;

            pos -= size / 2f;

            CVector2 botLeft = pos;
            CVector2 topLeft = new CVector2(pos.x, pos.y + size.y);
            CVector2 topRight = new CVector2(pos.x + size.x, pos.y + size.y);
            CVector2 botRight = new CVector2(pos.x + size.x, pos.y);
            
            BatchDrawLine(botLeft, topLeft, (CColor)color, lineWidth);
            BatchDrawLine(topLeft, topRight, (CColor)color, lineWidth);
            BatchDrawLine(topRight, botRight, (CColor)color, lineWidth);
            BatchDrawLine(botRight, botLeft, (CColor)color, lineWidth);
        }

        public static void BatchDrawRectangle(CVector2 pos, CVector2 size, CColor? color = null)
        {
            if (!_batchingInProgress)
                throw new Exception("Cannot batch draw with no batch started!");

            EnsureBufferSpaceForPolygon(4, 6);

            if (_isScreenSpaceBatch)
            {
                pos = ToScreenVector(pos);
            }
            
            color ??= CColor.White;
            
            float xExtents = size.x / 2f;
            float yExtents = size.y / 2f;
            
            float botLeftX = pos.x - xExtents;
            float botLeftY = pos.y - yExtents;
            float topLeftX = pos.x - xExtents;
            float topLeftY = pos.y + yExtents;
            float topRightX = pos.x + xExtents;
            float topRightY = pos.y + yExtents;
            float botRightX = pos.x + xExtents;
            float botRightY = pos.y - yExtents;
            
            _indexBuffer[_indexCount++] = _vertCount + 0;
            _indexBuffer[_indexCount++] = _vertCount + 1;
            _indexBuffer[_indexCount++] = _vertCount + 3;
            
            _indexBuffer[_indexCount++] = _vertCount + 1;
            _indexBuffer[_indexCount++] = _vertCount + 2;
            _indexBuffer[_indexCount++] = _vertCount + 3;
            
            _vertexBuffer[_vertCount++] = new VertexPositionColor(new CVector2(botLeftX, botLeftY), (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(new CVector2(topLeftX, topLeftY), (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(new CVector2(topRightX, topRightY), (CColor)color);
            _vertexBuffer[_vertCount++] = new VertexPositionColor(new CVector2(botRightX, botRightY), (CColor)color);
        }

        public static void EndBatch()
        {
            if (!_batchingInProgress)
                throw new Exception("Cannot end batch with no batch started!");
            
            Flush();

            _batchingInProgress = false;
        }

        public void Dispose()
        {
            _shader?.Dispose();
        }

        private static CVector2 ToScreenVector(CVector2 vec)
        {
            return new CVector2(vec.x, EngineRenderer.RawGameRect.Height - vec.y);
        }

        private static void EnsureBufferSpaceForPolygon(int polyVertCount, int polyIndexCount)
        {
            if (polyVertCount > MAX_VERTEX_BUFFER_SIZE || polyIndexCount > MAX_INDEX_BUFFER_SIZE)
                throw new Exception(
                    $"Cannot batch polygon with {polyVertCount} vertices and {polyIndexCount} indices to buffers" +
                    $"with {MAX_VERTEX_BUFFER_SIZE} max vertices and {MAX_INDEX_BUFFER_SIZE} max indices.");
            
            if (_vertCount + polyVertCount > MAX_VERTEX_BUFFER_SIZE || _indexCount + polyIndexCount > MAX_INDEX_BUFFER_SIZE)
                Flush();
        }
        
        private static void InitializeViewProjectionMatrix(Matrix? viewMatrix)
        {
            viewMatrix ??= EngineRenderer.CameraMatrix;
            
            Viewport viewport = EngineRenderer.RawGameViewport;
            _shader.Projection = Matrix.CreateOrthographicOffCenter(
                0f, viewport.Width, viewport.Height, 0f, -1, 1f);
            _shader.View = (Matrix)viewMatrix;
        }

        private static void Flush()
        {
            foreach (EffectPass pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, 
                    _vertexBuffer, 0, _vertCount,
                    _indexBuffer, 0, _indexCount / 3);
            }

            _vertCount = 0;
            _indexCount = 0;
        }
    }
}