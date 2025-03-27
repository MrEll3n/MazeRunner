using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPG
{
    public  class Cube : Model
    {
        public Cube() { 
            Vertices.Add(new Vertex(new Vector3(-1,-1,-1), new ColorRGB(0.1,0.1,0.1)));
            Vertices.Add(new Vertex(new Vector3(1, -1, -1), new ColorRGB(1, 0, 0)));
            Vertices.Add(new Vertex(new Vector3(1, 1, -1), new ColorRGB(1, 1, 0)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, -1), new ColorRGB(0, 1, 0)));

            Vertices.Add(new Vertex(new Vector3(-1, -1, 1), new ColorRGB(0, 0, 1)));
            Vertices.Add(new Vertex(new Vector3(1, -1, 1), new ColorRGB(1, 0, 1)));
            Vertices.Add(new Vertex(new Vector3(1, 1, 1), new ColorRGB(1, 1, 1)));
            Vertices.Add(new Vertex(new Vector3(-1, 1, 1), new ColorRGB(0, 1, 1)));

            Triangles.Add(new Triangle(0, 1, 2));
            Triangles.Add(new Triangle(0, 2, 3));

            Triangles.Add(new Triangle(1, 5, 6));
            Triangles.Add(new Triangle(1, 6, 2));

            Triangles.Add(new Triangle(5, 4, 7));
            Triangles.Add(new Triangle(5, 7, 6));

            Triangles.Add(new Triangle(4, 0, 3));
            Triangles.Add(new Triangle(4, 3, 7));

        }
    }
}
