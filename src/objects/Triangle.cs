namespace ZPG
{
    /// <summary>
    /// Reprezentuje jeden trojúhelník pomocí tří indexů do seznamu vrcholů.
    /// Používá se při vykreslování modelů a výpočtu normál.
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// Index prvního vrcholu v seznamu vrcholů.
        /// </summary>
        public int I1 { get; set; }

        /// <summary>
        /// Index druhého vrcholu v seznamu vrcholů.
        /// </summary>
        public int I2 { get; set; }

        /// <summary>
        /// Index třetího vrcholu v seznamu vrcholů.
        /// </summary>
        public int I3 { get; set; }

        /// <summary>
        /// Vytvoří trojúhelník pomocí tří zadaných indexů.
        /// </summary>
        /// <param name="i1">Index prvního vrcholu.</param>
        /// <param name="i2">Index druhého vrcholu.</param>
        /// <param name="i3">Index třetího vrcholu.</param>
        public Triangle(int i1, int i2, int i3)
        {
            I1 = i1;
            I2 = i2;
            I3 = i3;
        }

        /// <summary>
        /// Vrací textovou reprezentaci trojúhelníku.
        /// </summary>
        /// <returns>Např. "[0 1 2]"</returns>
        public override string ToString()
        {
            return $"[{I1} {I2} {I3}]";
        }
    }
}
