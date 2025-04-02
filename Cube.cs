using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace ZPG
{
    public class Cube : Model
    {
        public Cube()
        {
            // Vrcholy
            Vertices.Add(new Vertex(new Vector3(-1, -1, -1), new ColorRGB(0.1, 0.1, 0.1))); // 0
            Vertices.Add(new Vertex(new Vector3(1, -1, -1), new ColorRGB(1, 0, 0)));         // 1
            Vertices.Add(new Vertex(new Vector3(1, 1, -1), new ColorRGB(1, 1, 0)));         // 2
            Vertices.Add(new Vertex(new Vector3(-1, 1, -1), new ColorRGB(0, 1, 0)));        // 3

            Vertices.Add(new Vertex(new Vector3(-1, -1, 1), new ColorRGB(0, 0, 1)));        // 4
            Vertices.Add(new Vertex(new Vector3(1, -1, 1), new ColorRGB(1, 0, 1)));         // 5
            Vertices.Add(new Vertex(new Vector3(1, 1, 1), new ColorRGB(1, 1, 1)));          // 6
            Vertices.Add(new Vertex(new Vector3(-1, 1, 1), new ColorRGB(0, 1, 1)));         // 7

            // Zadní stěna (-Z)
            Triangles.Add(new Triangle(0, 2, 1));
            Triangles.Add(new Triangle(0, 3, 2));

            // Pravá stěna (+X)
            Triangles.Add(new Triangle(1, 6, 5));
            Triangles.Add(new Triangle(1, 2, 6));

            // Přední stěna (+Z)
            Triangles.Add(new Triangle(5, 6, 7));
            Triangles.Add(new Triangle(5, 7, 4));

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
