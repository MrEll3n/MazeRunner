using System.Runtime.InteropServices;

namespace ZPG
{
    /// <summary>
    /// Struktura pro vrchol skládající se z pozice a barvy. Využívá třídu OpenTK.Mathematics.Vector3
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 64)]
    public struct VertexGL
    {
        public OpenTK.Mathematics.Vector3 position;
        public OpenTK.Mathematics.Vector3 color;

        public VertexGL(Vector3 position, ColorRGB color)
        {
            this.position = new OpenTK.Mathematics.Vector3((float)position.X, (float)position.Y, (float)position.Z);
            this.color = new OpenTK.Mathematics.Vector3((float)color.R, (float)color.G, (float)color.B);
        }

        /// <summary>
        /// Vrátí velikost struktury v bytech
        /// </summary>
        /// <returns></returns>
        public static int SizeOf()
        {
            return Marshal.SizeOf(typeof(VertexGL));
        }
    }
}
