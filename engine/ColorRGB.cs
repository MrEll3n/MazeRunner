namespace ZPG
{
    /// <summary>
    /// Barva RGB (0-1)
    /// </summary>
    public class ColorRGB
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public ColorRGB(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return $"#{(int)(R*255):X}{(int)(G * 255):X}{(int)(B * 255):X}";
        }
    }
}
