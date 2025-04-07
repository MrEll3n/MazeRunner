using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje jeden vrchol 3D modelu – obsahuje pozici, UV souřadnice a normálu.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// Pozice vrcholu ve 3D prostoru.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Texturovací souřadnice (UV).
        /// </summary>
        public Vector2 TexCoord { get; set; }

        /// <summary>
        /// Normálový vektor (používá se pro nasvícení).
        /// </summary>
        public Vector3 Normal { get; set; } = Vector3.Zero;

        /// <summary>
        /// Vytvoří nový vrchol se zadanou pozicí a UV souřadnicemi.
        /// Normála je nastavena na (0,0,0) – doporučuje se později spočítat.
        /// </summary>
        public Vertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = Vector3.Zero;
        }

        /// <summary>
        /// Převede vrchol na pole `float[]` – pozice, UV, normála – v tomto pořadí.
        /// Používá se při odesílání dat do OpenGL (VBO).
        /// </summary>
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
