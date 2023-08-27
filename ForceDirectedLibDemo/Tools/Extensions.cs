using ForceDirectedLib.Tools;

namespace ForceDirectedLibDemo.Tools
{
    public static class Extensions
	{
		public static System.Windows.Point ToSWPoint(this Point point)
		{
			return new System.Windows.Point(point.X, point.Y);
		}

		public static Point ToSDPoint(this System.Windows.Point point)
		{
			return new Point((int)point.X, (int)point.Y);
		}
	}
}
