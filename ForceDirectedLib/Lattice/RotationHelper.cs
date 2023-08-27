namespace Lattice
{
	public static class RotationHelper
	{
		private const double mouseDragMultiplier = 0.005;

		public static void MouseDrag(RotationMethod rotationMethod, double deltaX, double deltaY)
		{
			var vector = new Vector(deltaX, deltaY, 0.0);

			if (vector.Magnitude() <= 0.0)
			{
				return;
			}

			rotationMethod(Vector.Zero, new Vector(vector.Y, vector.X, 0.0), vector.Magnitude() * mouseDragMultiplier);
		}

		public delegate void RotationMethod(Vector point, Vector direction, double angle);
	}
}
