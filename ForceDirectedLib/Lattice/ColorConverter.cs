using ForceDirectedLib.Tools;
using System;

namespace Lattice
{
    public static class ColorConverter
	{
		public static int[] HSLToRGB(double h, double s, double l)
		{
			byte r;
			byte g;
			byte b;

			if (s == 0.0)
			{
				r = (byte)Math.Round(l * byte.MaxValue);
				g = (byte)Math.Round(l * byte.MaxValue);
				b = (byte)Math.Round(l * byte.MaxValue);
			}
			else
			{
				double t2 = l >= 0.5 ? l + s - (l * s) : l * (1.0 + s);
				double t1 = (2.0 * l) - t2;
				double c1 = h + (1.0 / 3.0);
				double c2 = h;
				double c3 = h - (1.0 / 3.0);
				double rgbCalc1 = HSLToRGBCalc(c1, t1, t2);
				double rgbCalc2 = HSLToRGBCalc(c2, t1, t2);
				double rgbCalc3 = HSLToRGBCalc(c3, t1, t2);
				r = (byte)Math.Round(rgbCalc1 * byte.MaxValue);
				g = (byte)Math.Round(rgbCalc2 * byte.MaxValue);
				b = (byte)Math.Round(rgbCalc3 * byte.MaxValue);
			}

			return new int[3] { r, g, b };
		}

		private static double HSLToRGBCalc(double h, double s, double l)
		{
			h = h < 0.0 ? h + 1.0 : (h > 1.0 ? h - 1.0 : h);

			if (6.0 * h < 1.0)
			{
				return s + ((l - s) * 6.0 * h);
			}

			if (2.0 * h < 1.0)
			{
				return l;
			}

			return 3.0 * h < 2.0 ? s + ((l - s) * (4.0 - (h * 6.0))) : s;
		}

		public static Color HSLToColor(params double[] hsl)
		{
			int[] rgb = HSLToRGB(hsl[0], hsl[1], hsl[2]);

			return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
		}

		public static double[] RGBToHSL(int r, int g, int b)
		{
			double val1 = r / (double)byte.MaxValue;
			double val2_1 = g / (double)byte.MaxValue;
			double val2_2 = b / (double)byte.MaxValue;
			double num1 = Math.Min(Math.Min(val1, val2_1), val2_2);
			double num2 = Math.Max(Math.Max(val1, val2_1), val2_2);
			double num3 = num2 - num1;
			double num4 = 0.0;
			double num5 = 0.0;
			double num6 = (num2 + num1) * 0.5;

			if (num3 != 0.0)
			{
				num5 = num6 >= 0.5 ? num3 / (2.0 - num2 - num1) : num3 / (num2 + num1);

				if (val1 == num2)
				{
					num4 = (val2_1 - val2_2) / num3;
				}
				else if (val2_1 == num2)
				{
					num4 = 2.0 + ((val2_2 - val1) / num3);
				}
				else if (val2_2 == num2)
				{
					num4 = 4.0 + ((val1 - val2_1) / num3);
				}
			}

			double num7 = num4 / 6.0;

			return new double[3] { num7 < 0.0 ? num7 + 1.0 : num7, num5, num6 };
		}

		public static double[] ColorToHSL(Color c)
		{
			return RGBToHSL(c.R, c.G, c.B);
		}
	}
}
