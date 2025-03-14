using System.ComponentModel;

namespace ZPG
{
    /// <summary>
    /// Vrchol obsahující pozici a barvu
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// TypeConverter umožňuje zobrazit vnitřní strukturu v PropertyGridu
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Vector3 Position { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ColorRGB Color { get; set; }

        public Vertex(Vector3 position, ColorRGB color)
        {
            Position = position;
            Color = color;
        }

        public override string ToString()
        {
            return $"{Position} {Color}";
        }

    }
}
