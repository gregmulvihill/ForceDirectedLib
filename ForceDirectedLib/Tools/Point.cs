namespace ForceDirectedLib.Tools
{
    public class Point
    {
        public Point()
        {
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public void Offset(int xd, int yd)
        {
            X += xd;
            Y += yd;
        }
    }
}