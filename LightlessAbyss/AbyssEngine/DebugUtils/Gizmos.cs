using System;
using AbyssEngine.Backend.Rendering;
using AbyssEngine.CustomColor;
using AbyssEngine.CustomMath;
using Microsoft.Xna.Framework;

namespace AbyssEngine.DebugUtils
{
    public static class Gizmos
    {
        public static Matrix matrix;
        public static CColor color;

        static Gizmos()
        {
            matrix = Matrix.Identity;
            color = CColor.White;
        }

        public static void DrawLine(CVector2 p1, CVector2 p2, float lineWidth = .01f, bool batched = false)
        {
            if (!batched)
                PolygonRenderer.BeginBatch(matrix);
            
            PolygonRenderer.BatchDrawLine(
                p1, 
                p2, 
                color,
                lineWidth);
            
            if (!batched)
                PolygonRenderer.EndBatch();
        }
        
        public static void DrawWireGrid(CVector2 pos, int width, int height, bool batched = false)
        {
            if (!batched)
                PolygonRenderer.BeginBatch(matrix);
            
            CVector2 offset = new CVector2(width / 2f, height / 2f);
            pos -= offset;
            float lineWidth = .01f * EngineRenderer.CameraOrthoSizeScaler;

            for (int y = 0; y <= height; y++)
            {
                PolygonRenderer.BatchDrawLine(
                    new CVector2(pos.x, pos.y + y), 
                    new CVector2(pos.x + width, pos.y + y),
                    color,
                    lineWidth);
            }
            
            for (int x = 0; x <= width; x++)
            {
                PolygonRenderer.BatchDrawLine(
                    new CVector2(pos.x + x, pos.y), 
                    new CVector2(pos.x + x, pos.y + height),
                    color,
                    lineWidth);
            }

            if (!batched)
                PolygonRenderer.EndBatch();
        }

        public static void DrawWireRectangle(CVector2 position, CVector2 size, bool batched = false)
        {
            if (!batched)
                PolygonRenderer.BeginBatch(matrix);

            float lineWidth = .01f * EngineRenderer.CameraOrthoSizeScaler;
            PolygonRenderer.BatchDrawWireRectangle(
                position, 
                size, 
                color, 
                lineWidth);

            if (!batched)
                PolygonRenderer.EndBatch();
        }

        public static void DrawRectangle(CVector2 position, CVector2 size, bool batched = false)
        {
            if (!batched)
                PolygonRenderer.BeginBatch(matrix);

            PolygonRenderer.BatchDrawRectangle(
                position, 
                size, 
                color);

            if (!batched)
                PolygonRenderer.EndBatch();
        }

        public static void StartGizmoBatch()
        {
            if (PolygonRenderer.BatchingInProgress)
                throw new Exception("Cannot start gizmo batch with polygon batching already in progress!");
            
            PolygonRenderer.BeginBatch(matrix);
        }

        public static void EndGizmoBatch()
        {
            if (!PolygonRenderer.BatchingInProgress)
                throw new Exception("Cannot end gizmo batch with no polygon batch in progress!");
            
            PolygonRenderer.EndBatch();
        }
    }
}