namespace ForceDirectedLib.Tools
{
    public class Color
    {
        public uint Value { get; set; }

        public Color(Color color) => Value = color.Value;
        public Color(uint value) => Value = value;
        public Color(int value) => Value = (uint)value;

        public byte A { get => (byte)(int)((Value & 0xff000000) >> 24); set => Value = Value & 0x00ffffff | (uint)((value & 0xff) << 24); }
        public byte R { get => (byte)(int)((Value & 0x00ff0000) >> 16); set => Value = Value & 0xff00ffff | (uint)((value & 0xff) << 16); }
        public byte G { get => (byte)(int)((Value & 0x0000ff00) >> 08); set => Value = Value & 0xffff00ff | (uint)((value & 0xff) << 08); }
        public byte B { get => (byte)(int)((Value & 0x000000ff) >> 00); set => Value = Value & 0xffffff00 | (uint)((value & 0xff) << 00); }

        public static Color FromArgb(int color)
        {
            return new Color(color);
        }

        public static Color FromArgb(int alpha, Color color)
        {
            uint a = (uint)((alpha & 0xff) << 24);
            uint rgb = color.Value & 0x00ffffff;
            return new Color(a | rgb);
        }

        public static Color FromArgb(int r, int g, int b)
        {
            return new Color(r << 16 | g << 8 | b);
        }

        public int ToArgb() => (int)Value;

        public static implicit operator int(Color color) => (int)color.Value;
    }
}