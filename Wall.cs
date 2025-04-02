using OpenTK.Mathematics;

namespace ZPG
{
    public class Wall : Model
    {
        public Wall() : this(2.0f, 3.0f, 2.0f) // výchozí rozměry: šířka 2m, výška 3m, hloubka 2m
        {
        }

        public Wall(float width, float height, float depth)
        {
            float w = width / 2f;
            float h = height;
            float d = depth / 2f;

            // Vrcholy: výška od 0 (země) do h
            Vertices.Add(new Vertex(new Vector3(-w, 0, -d), new ColorRGB(1, 0, 0))); // 0
            Vertices.Add(new Vertex(new Vector3(w, 0, -d), new ColorRGB(1, 0, 0)));  // 1
            Vertices.Add(new Vertex(new Vector3(w, h, -d), new ColorRGB(1, 0, 0)));  // 2
            Vertices.Add(new Vertex(new Vector3(-w, h, -d), new ColorRGB(1, 0, 0))); // 3

            Vertices.Add(new Vertex(new Vector3(-w, 0, d), new ColorRGB(1, 0, 0)));  // 4
            Vertices.Add(new Vertex(new Vector3(w, 0, d), new ColorRGB(1, 0, 0)));   // 5
            Vertices.Add(new Vertex(new Vector3(w, h, d), new ColorRGB(1, 0, 0)));   // 6
            Vertices.Add(new Vertex(new Vector3(-w, h, d), new ColorRGB(1, 0, 0)));  // 7

            // Zadní stěna (-Z)
            Triangles.Add(new Triangle(0, 2, 1));
            Triangles.Add(new Triangle(0, 3, 2));

            // Přední stěna (+Z)
            Triangles.Add(new Triangle(5, 6, 7));
            Triangles.Add(new Triangle(5, 7, 4));

            // Pravá stěna (+X)
            Triangles.Add(new Triangle(1, 6, 5));
            Triangles.Add(new Triangle(1, 2, 6));

            // Levá stěna (-X)
            Triangles.Add(new Triangle(4, 7, 3));
            Triangles.Add(new Triangle(4, 3, 0));

            // Horní stěna (+Y)
            Triangles.Add(new Triangle(3, 7, 6));
            Triangles.Add(new Triangle(3, 6, 2));

            // Spodní stěna (-Y)
            Triangles.Add(new Triangle(4, 0, 1));
            Triangles.Add(new Triangle(4, 1, 5));
        }
    }
}
