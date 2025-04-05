using OpenTK.Mathematics;

namespace ZPG
{
    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector2 TexCoord { get; set; }
        public Vector3 Normal { get; set; } = Vector3.Zero;

        public Vertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = Vector3.Zero;
        }

        public float[] ToArray()
        {
            return new float[]
            {
                Position.X, Position.Y, Position.Z,
                TexCoord.X, TexCoord.Y,
                Normal.X, Normal.Y, Normal.Z
            };
        }
    }
}
