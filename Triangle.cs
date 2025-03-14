namespace ZPG
{

    /// <summary>
    /// Indexy trojúhelníku
    /// </summary>
    public class Triangle
    {
        public int I1 { get; set; } 
        public int I2 { get; set; }
        public int I3 { get; set; }

        public Triangle(int i1, int i2, int i3) { I1 = i1; I2 = i2; I3 = i3; }

        public override string ToString()
        {
            return $"[{I1} {I2} {I3}]";
        }
    }
}
