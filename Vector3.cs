namespace ZPG
{
    /// <summary>
    /// Dvourozměrný vektor
    /// </summary>
    public class Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3(double x, double y, double z) {  X = x; Y = y; Z = z; }

        public override string ToString()
        {
            return $"[{X:F2}; {Y:F2}; {Z:F2}]";
        }
    }
}
