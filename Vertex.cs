using OpenTK.Mathematics;

namespace ZPG
{
    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector2 TexCoord { get; set; }

        public Vertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public float[] ToArray()
        {
            return new float[] { Position.X, Position.Y, Position.Z, TexCoord.X, TexCoord.Y };
        }
    }
}
