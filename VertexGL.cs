using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace ZPG
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexGL
    {
        public Vector3 position;
        public Vector2 texCoord;

        public VertexGL(Vector3 pos, Vector2 uv)
        {
            position = pos;
            texCoord = uv;
        }

        public static int SizeOf() => Marshal.SizeOf<VertexGL>();

        public float[] ToArray()
        {
            return new float[] { position.X, position.Y, position.Z, texCoord.X, texCoord.Y };
        }
    }
}
