namespace ZPG
{
    /// <summary>
    /// Reprezentuje barvu v RGB formátu s komponentami v rozsahu 0–1.
    /// </summary>
    public class ColorRGB
    {
        /// <summary>
        /// Červená složka barvy (0.0 až 1.0).
        /// </summary>
        public double R { get; set; }

        /// <summary>
        /// Zelená složka barvy (0.0 až 1.0).
        /// </summary>
        public double G { get; set; }

        /// <summary>
        /// Modrá složka barvy (0.0 až 1.0).
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Inicializuje barvu podle zadaných složek R, G a B.
        /// </summary>
        /// <param name="r">Červená složka (0–1).</param>
        /// <param name="g">Zelená složka (0–1).</param>
        /// <param name="b">Modrá složka (0–1).</param>
        public ColorRGB(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Vrací hexadecimální reprezentaci barvy ve formátu #RRGGBB.
        /// </summary>
        public override string ToString()
        {
            return $"#{(int)(R * 255):X}{(int)(G * 255):X}{(int)(B * 255):X}";
        }
    }
}
