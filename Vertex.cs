using System.ComponentModel;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Vrchol obsahující pozici a barvu
    /// </summary>
    public class Vertex
    {
        public Vector3 Position { get; set; }

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
