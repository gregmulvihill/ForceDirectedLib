using ForceDirectedLib.Tools;
using System;
using System.Collections.Generic;

namespace Lattice
{
    public class Renderer
	{
		private readonly double lightingMultiplier = 0.5;
		private double lightingBrightness;
		private double lightingMaximum = 0.4;
		private double lightingThreshold = 0.2;
		private double lightingRatio = 0.5;
		private double ideal;
		private double fov;
		public Point Origin = new Point();
		public Vector Camera = new Vector(0.0, 0.0, 1000.0);
		public Vector Light = new Vector(0.0, 1000.0, 0.0);
		public bool Lighting = true;

		public Renderer() => FOV = 1000.0;

		public double FOV
		{
			get => fov;
			set => ideal = Camera.Z * Camera.Z / (fov = value);
		}

		public double LightingOptimalRange
		{
			get => Math.Sqrt(lightingBrightness / lightingMultiplier);
			set => lightingBrightness = value * value * lightingMultiplier;
		}

		public double ShadowMaximum
		{
			get => lightingMaximum;
			set
			{
				lightingMaximum = value;
				lightingThreshold = value * lightingRatio;
			}
		}

		public double ShadowContrast
		{
			get => lightingRatio;
			set
			{
				lightingRatio = value;
				ShadowMaximum = lightingMaximum;
			}
		}

		public bool DrawPolygon(IGraphics g, Color colour, IList<Vector> vertices)
		{
			if (vertices[0].Z >= Camera.Z)
			{
				return false;
			}

			g.DrawPolygon(Color.FromArgb(colour.ToArgb()), ComputePoints(vertices));

			return true;
		}

		public bool FillPolygon(IGraphics g, Color colour, IList<Vector> vertices)
		{
			if (vertices[0].Z >= Camera.Z)
			{
				return false;
			}

			g.FillPolygon(new Color(Lighting ? ComputeLighting(colour, vertices) : colour), ComputePoints(vertices));

			return true;
		}

		public Point[] ComputePoints(IList<Vector> vectors)
		{
			var points = new Point[vectors.Count];

			for (int index = 0; index < points.Length; ++index)
			{
				points[index] = ComputePoint(vectors[index]);
			}

			return points;
		}

		public Point ComputePoint(Vector location)
		{
			double scale = ComputeScale(location);

			return new Point((int)Math.Round(((location.X - Camera.X) * scale) + Origin.X), (int)Math.Round(((-location.Y + Camera.Y) * scale) + Origin.Y));
		}

		public double ComputeScale(Vector vector)
		{
			return ideal / Vector.Distance(vector, Camera);
		}

		public Color ComputeLighting(Color colour, IList<Vector> vertices)
		{
			Vector a1 = Light.To(Vector.Average(vertices));
			Vector a2 = Camera.To(Vector.Average(vertices));
			var b = Vector.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);
			double num1 = Vector.Angle(a1, b);
			double num2 = Vector.Angle(a2, b);
			double num3 = lightingBrightness * 0.001 / a1.Magnitude();
			double num4 = (num1 < Math.PI / 2.0 && num2 > Math.PI / 2.0) || (num1 > Math.PI / 2.0 && num2 < Math.PI / 2.0) ? num3 * ((Math.Abs(Math.Sin(num1)) * lightingMaximum) + 1.0 - lightingMaximum - lightingThreshold) : num3 * ((Math.Abs(Math.Cos(num1)) * lightingThreshold) + 1.0 - lightingThreshold);
			double[] hsl = ColorConverter.ColorToHSL(colour);
			hsl[2] = num4 > 1.0 ? 1.0 : (num4 < 0.0 ? 0.0 : num4);

			return ColorConverter.HSLToColor(hsl);
		}

		public bool DrawPoint(IGraphics g, Color brush, Vector location)
		{
			if (location.Z >= Camera.Z)
			{
				return false;
			}

			double scale = ComputeScale(location);
			int x = (int)Math.Round(((location.X - Camera.X) * scale) + Origin.X);
			int y = (int)Math.Round(((-location.Y + Camera.Y) * scale) + Origin.Y);
			g.FillRectangle(brush, x, y, 1, 1);

			return true;
		}

		public bool DrawSquare2D(IGraphics g, Color pen, Vector location, double width)
		{
			if (location.Z >= Camera.Z)
			{
				return false;
			}

			double scale = ComputeScale(location);
			float x = (float)((location.X - Camera.X - (width * 0.5)) * scale) + Origin.X;
			float y = (float)((-location.Y + Camera.Y - (width * 0.5)) * scale) + Origin.Y;
			float num = (float)(width * scale);
			g.DrawRectangle(pen, x, y, num, num);

			return true;
		}

		public bool FillSquare2D(IGraphics g, Color brush, Vector location, double width)
		{
			if (location.Z >= Camera.Z)
			{
				return false;
			}

			double scale = ComputeScale(location);
			float x = (float)((location.X - Camera.X - (width * 0.5)) * scale) + Origin.X;
			float y = (float)((-location.Y + Camera.Y - (width * 0.5)) * scale) + Origin.Y;
			float num = (float)(width * scale);
			g.FillRectangle(brush, x, y, num, num);

			return true;
		}

		public bool DrawCircle2D(IGraphics g, Color pen, Vector location, double radius)
		{
			if (location.Z >= Camera.Z)
			{
				return false;
			}

			double scale = ComputeScale(location);
			float x = (float)((location.X - Camera.X - radius) * scale) + Origin.X;
			float y = (float)((-location.Y + Camera.Y - radius) * scale) + Origin.Y;
			float num = (float)(radius * scale * 2.0);
			g.DrawEllipse(pen, x, y, num, num);

			return true;
		}

		public bool FillCircle2D(IGraphics g, Color brush, Vector location, double radius)
		{
			if (location.Z >= Camera.Z)
			{
				return false;
			}

			double scale = ComputeScale(location);
			float x = (float)((location.X - Camera.X - radius) * scale) + Origin.X;
			float y = (float)((-location.Y + Camera.Y - radius) * scale) + Origin.Y;
			float num = (float)(radius * scale * 2.0);
			g.FillEllipse(brush, x, y, num, num);

			return true;
		}
	}
}
