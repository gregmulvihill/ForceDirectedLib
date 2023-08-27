using ForceDirectedLib.Tools;

using System.Windows.Media.Imaging;

namespace ForceDirectedLibDemo.ViewModel
{
    public class Graphics : IGraphics
	{
		public WriteableBitmap RenderSurface { get; set; }

		public float Width => RenderSurface.PixelWidth;

		public float Height => RenderSurface.PixelHeight;

		private float _translateTransformX;
		private float _translateTransformY;

		public Graphics(WriteableBitmap renderSurface)
		{
			RenderSurface = renderSurface;
		}

		public void DrawEllipse(Color pen, float x, float y, float xr, float yr)
		{
			throw new System.NotImplementedException();
		}

		public void DrawLine(Color edgePen, Point point1, Point point2)
		{
			RenderSurface.DrawLine(
				(int)(point1.X + _translateTransformX),
				(int)(point1.Y + _translateTransformY),
				(int)(point2.X + _translateTransformX),
				(int)(point2.Y + _translateTransformY),
				edgePen.ToArgb());
		}

		public void DrawPolygon(Color pen, Point[] points)
		{
			throw new System.NotImplementedException();
		}

		public void DrawRectangle(Color pen, float x, float y, float num1, float num2)
		{
			throw new System.NotImplementedException();
		}

		public void DrawString(string msg, Font font, Color color, float x, float y)
		{
			RenderSurface.DrawString((int)(x + _translateTransformX), (int)(y + _translateTransformY), System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B), new FontDetails(font.Name, font.Size), msg);
		}

		public void DrawString(string msg, Font font, Color brush, Point point)
		{
			RenderSurface.DrawString(point.X, point.Y, System.Windows.Media.Color.FromArgb(brush.A, brush.R, brush.G, brush.B), new FontDetails(font.Name, font.Size), msg);
		}

		public void FillEllipse(Color brush, float x, float y, float xr, float yr)
		{
			float xx = x + _translateTransformX;
			float yy = y + _translateTransformY;
			RenderSurface.FillEllipse((int)(xx - xr), (int)(yy - yr), (int)(xx + xr), (int)(yy + yr), brush.ToArgb());
		}

		public void FillPolygon(Color solidBrush, Point[] points)
		{
			throw new System.NotImplementedException();
		}

		public void FillRectangle(Color brush, float x, float y, float v1, float v2)
		{
			throw new System.NotImplementedException();
		}

		public void ResetTransform()
		{
			_translateTransformX = 0;
			_translateTransformY = 0;
		}

		public void TranslateTransform(float x, float y)
		{
			_translateTransformX = x;
			_translateTransformY = y;
		}
	}
}