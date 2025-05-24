namespace ZPG
{
    /// <summary>
    /// Represents a color in RGB format with components in the range 0–1.
    /// </summary>
    public class ColorRGB
    {
        /// <summary>
        /// Red color component (0.0 to 1.0).
        /// </summary>
        public double R { get; set; }

        /// <summary>
        /// Green color component (0.0 to 1.0).
        /// </summary>
        public double G { get; set; }

        /// <summary>
        /// Blue color component (0.0 to 1.0).
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Initializes the color using the specified R, G, and B components.
        /// </summary>
        /// <param name="r">Red component (0–1).</param>
        /// <param name="g">Green component (0–1).</param>
        /// <param name="b">Blue component (0–1).</param>
        public ColorRGB(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Returns the hexadecimal representation of the color in the format #RRGGBB.
        /// </summary>
        public override string ToString()
        {
            return $"#{(int)(R * 255):X}{(int)(G * 255):X}{(int)(B * 255):X}";
        }
    }
}
