//using System.Drawing;

namespace ForceDirectedLib.Tools
{
    public interface IGraphics
    {
        float Width { get; }
        float Height { get; }

        void DrawRectangle(Color pen, float x, float y, float w, float h);

        void DrawLine(Color edgePen, Point point1, Point point2);

        void FillRectangle(Color brush, float x, float y, float v1, float v2);

        void TranslateTransform(float x, float y);

        void ResetTransform();

        void DrawString(string msg, Font font, Color brush, float x, float y);

        void DrawPolygon(Color pen, Point[] points);

        void FillPolygon(Color solidBrush, Point[] points);

        void DrawEllipse(Color pen, float x, float y, float xr, float yr);

        void FillEllipse(Color brush, float x, float y, float xr, float yr);

        void DrawString(string label, Font font, Color brush, Point point);
    }
}