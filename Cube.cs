using OpenTK.Mathematics;

namespace ZPG
{
    public class Cube : Model
    {
        public Cube()
        {
            // Vrcholy – každá stěna má vlastní set 4 vrcholů kvůli UVGL.Enable(EnableCap.Texture2D);
            // Přední stěna (+Z)
            Vertices.Add(new Vertex(new Vector3(-1, -1, 1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(1, -1, 1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(1, 1, 1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, 1), new Vector2(0, 1)));

            // Zadní stěna (-Z)
            Vertices.Add(new Vertex(new Vector3(1, -1, -1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(-1, -1, -1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, -1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(1, 1, -1), new Vector2(0, 1)));

            // Levá stěna (-X)
            Vertices.Add(new Vertex(new Vector3(-1, -1, -1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(-1, -1, 1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, 1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, -1), new Vector2(0, 1)));

            // Pravá stěna (+X)
            Vertices.Add(new Vertex(new Vector3(1, -1, 1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(1, -1, -1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(1, 1, -1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(1, 1, 1), new Vector2(0, 1)));

            // Horní stěna (+Y)
            Vertices.Add(new Vertex(new Vector3(-1, 1, 1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(1, 1, 1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(1, 1, -1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, -1), new Vector2(0, 1)));

            // Spodní stěna (-Y)
            Vertices.Add(new Vertex(new Vector3(-1, -1, -1), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(1, -1, -1), new Vector2(1, 0)));
            Vertices.Add(new Vertex(new Vector3(1, -1, 1), new Vector2(1, 1)));
            Vertices.Add(new Vertex(new Vector3(-1, -1, 1), new Vector2(0, 1)));

            // Každá stěna = 2 trojúhelníky (6 vrcholových indexů)

            // Front
            Triangles.Add(new Triangle(0, 1, 2));
            Triangles.Add(new Triangle(0, 2, 3));

            // Back
            Triangles.Add(new Triangle(4, 5, 6));
            Triangles.Add(new Triangle(4, 6, 7));

            // Left
            Triangles.Add(new Triangle(8, 9, 10));
            Triangles.Add(new Triangle(8, 10, 11));

            // Right
            Triangles.Add(new Triangle(12, 13, 14));
            Triangles.Add(new Triangle(12, 14, 15));

            // Top
            Triangles.Add(new Triangle(16, 17, 18));
            Triangles.Add(new Triangle(16, 18, 19));

            // Bottom
            Triangles.Add(new Triangle(20, 21, 22));
            Triangles.Add(new Triangle(20, 22, 23));

            Construct();
        }
    }
}
