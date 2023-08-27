namespace ForceDirectedLib.Tools
{
    public class Font
    {
        public Font(string name, double size)
        {
            Name = name;
            Size = size;
        }

        public string Name { get; }
        public double Size { get; }
    }
}