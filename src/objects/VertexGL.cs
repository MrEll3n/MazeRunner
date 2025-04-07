using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace ZPG
{
    /// <summary>
    /// Struktura vrcholu připravená pro přímý upload do GPU.
    /// Obsahuje pozici, UV souřadnice a normálu – v přesném pořadí v paměti.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexGL
    {
        /// <summary>
        /// Pozice vrcholu ve 3D prostoru.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Texturovací souřadnice (UV).
        /// </summary>
        public Vector2 texCoord;

        /// <summary>
        /// Normálový vektor pro nasvícení.
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// Vytvoří nový vrchol se všemi potřebnými daty.
        /// </summary>
        /// <param name="pos">Pozice ve světovém prostoru.</param>
        /// <param name="uv">Texturovací souřadnice.</param>
        /// <param name="norm">Normála.</param>
        public VertexGL(Vector3 pos, Vector2 uv, Vector3 norm)
        {
            position = pos;
            texCoord = uv;
            normal = norm;
        }

        /// <summary>
        /// Vrací velikost této struktury v bajtech (např. pro výpočet stride).
        /// </summary>
        public static int SizeOf() => Marshal.SizeOf<VertexGL>();

        /// <summary>
        /// Převede tento vrchol na pole floatů (pozice, texCoord, normála).
        /// </summary>
        public float[] ToArray()
        {
            return new float[]
            {
                position.X, position.Y, position.Z,
                texCoord.X, texCoord.Y,
                normal.X, normal.Y, normal.Z
            };
        }
    }
}
