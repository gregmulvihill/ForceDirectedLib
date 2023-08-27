using System;

namespace Lattice
{
	public static class PseudoRandom
	{
		private static readonly Random _rng = new Random();

		public static double Double(double a, double b = 0.0)
		{
			return a + (_rng.NextDouble() * (b - a));
		}

		public static int Int32(int a = int.MaxValue)
		{
			return _rng.Next(a);
		}

		public static Vector Vector(double maximumMagnitude = 1.0)
		{
			return Double(maximumMagnitude) * DirectionVector();
		}

		public static Vector DirectionVector(double magnitude = 1.0)
		{
			Vector vector;

			do
			{
				vector = new Vector(Double(-1.0, 1.0), Double(-1.0, 1.0), Double(-1.0, 1.0));
			}
			while (vector.Magnitude() == 0.0);

			return magnitude / vector.Magnitude() * vector;
		}
	}
}
