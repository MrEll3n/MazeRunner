using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje 3D kvádr (stěnu) s texturovanými stěnami, vhodný např. pro kolizní nebo vizuální reprezentaci zdi.
    /// </summary>
    public class Wall : Model
    {
        /// <summary>
        /// Poloviční šířka stěny (původní šířka dělená dvěma).
        /// </summary>
        public float w;

        /// <summary>
        /// Výška stěny.
        /// </summary>
        public float h;

        /// <summary>
        /// Poloviční hloubka stěny (původní hloubka dělená dvěma).
        /// </summary>
        public float d;

        /// <summary>
        /// Vytvoří stěnu s výchozími rozměry 2x3x2 metry.
        /// </summary>
        public Wall() : this(2.0f, 3.0f, 2.0f)
        {
        }

        /// <summary>
        /// Vytvoří stěnu s danými rozměry.
        /// </summary>
        /// <param name="width">Celková šířka stěny (osa X).</param>
        /// <param name="height">Celková výška stěny (osa Y).</param>
        /// <param name="depth">Celková hloubka stěny (osa Z).</param>
        public Wall(float width, float height, float depth)
        {
            w = width / 2f;
            h = height;
            d = depth / 2f;

            // Přidání vrcholů pro každou stěnu kvádru s pozicí a UV souřadnicemi
            // Každá stěna má 4 vrcholy a tvoří 2 trojúhelníky

            // Front face (+Z)
            Vertices.Add(new Vertex(new Vector3(-w, 0, d), new Vector2(0, 0)));  // 0
            Vertices.Add(new Vertex(new Vector3(w, 0, d), new Vector2(1, 0)));   // 1
            Vertices.Add(new Vertex(new Vector3(w, h, d), new Vector2(1, 1)));   // 2
            Vertices.Add(new Vertex(new Vector3(-w, h, d), new Vector2(0, 1)));  // 3

            // Back face (-Z)
            Vertices.Add(new Vertex(new Vector3(w, 0, -d), new Vector2(0, 0)));  // 4
            Vertices.Add(new Vertex(new Vector3(-w, 0, -d), new Vector2(1, 0))); // 5
            Vertices.Add(new Vertex(new Vector3(-w, h, -d), new Vector2(1, 1))); // 6
            Vertices.Add(new Vertex(new Vector3(w, h, -d), new Vector2(0, 1)));  // 7

            // Left face (-X)
            Vertices.Add(new Vertex(new Vector3(-w, 0, -d), new Vector2(0, 0))); // 8
            Vertices.Add(new Vertex(new Vector3(-w, 0, d), new Vector2(1, 0)));  // 9
            Vertices.Add(new Vertex(new Vector3(-w, h, d), new Vector2(1, 1)));  // 10
            Vertices.Add(new Vertex(new Vector3(-w, h, -d), new Vector2(0, 1))); // 11

            // Right face (+X)
            Vertices.Add(new Vertex(new Vector3(w, 0, d), new Vector2(0, 0)));   // 12
            Vertices.Add(new Vertex(new Vector3(w, 0, -d), new Vector2(1, 0)));  // 13
            Vertices.Add(new Vertex(new Vector3(w, h, -d), new Vector2(1, 1)));  // 14
            Vertices.Add(new Vertex(new Vector3(w, h, d), new Vector2(0, 1)));   // 15

            // Top face (+Y)
            Vertices.Add(new Vertex(new Vector3(-w, h, d), new Vector2(0, 0)));  // 16
            Vertices.Add(new Vertex(new Vector3(w, h, d), new Vector2(1, 0)));   // 17
            Vertices.Add(new Vertex(new Vector3(w, h, -d), new Vector2(1, 1)));  // 18
            Vertices.Add(new Vertex(new Vector3(-w, h, -d), new Vector2(0, 1))); // 19

            // Bottom face (-Y)
            Vertices.Add(new Vertex(new Vector3(-w, 0, -d), new Vector2(0, 0))); // 20
            Vertices.Add(new Vertex(new Vector3(w, 0, -d), new Vector2(1, 0)));  // 21
            Vertices.Add(new Vertex(new Vector3(w, 0, d), new Vector2(1, 1)));   // 22
            Vertices.Add(new Vertex(new Vector3(-w, 0, d), new Vector2(0, 1)));  // 23

            // Přidání trojúhelníků pro každou stranu
            // Front (+Z)
            Triangles.Add(new Triangle(0, 1, 2));
            Triangles.Add(new Triangle(0, 2, 3));

            // Back (-Z)
            Triangles.Add(new Triangle(4, 5, 6));
            Triangles.Add(new Triangle(4, 6, 7));

            // Left (-X)
            Triangles.Add(new Triangle(8, 9, 10));
            Triangles.Add(new Triangle(8, 10, 11));

            // Right (+X)
            Triangles.Add(new Triangle(12, 13, 14));
            Triangles.Add(new Triangle(12, 14, 15));

            // Top (+Y)
            Triangles.Add(new Triangle(16, 17, 18));
            Triangles.Add(new Triangle(16, 18, 19));

            // Bottom (-Y)
            Triangles.Add(new Triangle(20, 21, 22));
            Triangles.Add(new Triangle(20, 22, 23));

            // Výpočet normál pro osvětlování
            ComputeNormals(Vertices, Triangles);

            // Vytvoření objektu (např. nahrání dat do GPU)
            Construct();
        }

        /// <summary>
        /// Vrací Axis-Aligned Bounding Box (AABB) této stěny.
        /// Slouží například ke kolizní detekci.
        /// </summary>
        /// <returns>Tuple s minimálním a maximálním bodem kvádru v prostoru.</returns>
        public (Vector3 min, Vector3 max) GetAABB()
        {
            float halfWidth = w / 2.0f;
            float halfDepth = d / 2.0f;
            float height = h / 2.0f;

            Vector3 min = Position + new Vector3(-halfWidth, 0, -halfDepth);
            Vector3 max = Position + new Vector3(halfWidth, height, halfDepth);
            return (min, max);
        }
    }
}
