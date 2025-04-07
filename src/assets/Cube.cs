using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje jednotkovou texturovanou krychli (cube), tvořenou 6 stěnami a 12 trojúhelníky.
    /// Každá stěna má samostatné vrcholy kvůli odlišným UV souřadnicím.
    /// </summary>
    public class Cube : Model
    {
        /// <summary>
        /// Vytvoří novou instanci krychle s centrem v počátku a rozměry 2x2x2 (od -1 do +1).
        /// </summary>
        public Cube()
        {
            // Každá stěna má svůj vlastní set 4 vrcholů kvůli texturovacím souřadnicím.

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

            // Každá stěna je tvořena 2 trojúhelníky (6 indexů)

            // Přední stěna
            Triangles.Add(new Triangle(0, 1, 2));
            Triangles.Add(new Triangle(0, 2, 3));

            // Zadní stěna
            Triangles.Add(new Triangle(4, 5, 6));
            Triangles.Add(new Triangle(4, 6, 7));

            // Levá stěna
            Triangles.Add(new Triangle(8, 9, 10));
            Triangles.Add(new Triangle(8, 10, 11));

            // Pravá stěna
            Triangles.Add(new Triangle(12, 13, 14));
            Triangles.Add(new Triangle(12, 14, 15));

            // Horní stěna
            Triangles.Add(new Triangle(16, 17, 18));
            Triangles.Add(new Triangle(16, 18, 19));

            // Spodní stěna
            Triangles.Add(new Triangle(20, 21, 22));
            Triangles.Add(new Triangle(20, 22, 23));

            // Inicializace a vytvoření objektu
            Construct();
        }
    }
}
