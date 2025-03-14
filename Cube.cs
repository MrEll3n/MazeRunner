namespace ZPG
{
    public class Cube : Model
    {
        public Cube() : base()
        {
            vertices.Add(new Vertex(new Vector3(-1, -1, -1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3( 1, -1, -1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3( 1,  1, -1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3(-1,  1, -1), new ColorRGB(1, 1, 1)));

            vertices.Add(new Vertex(new Vector3(-1, -1,  1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3( 1, -1,  1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3( 1,  1,  1), new ColorRGB(1, 1, 1)));
            vertices.Add(new Vertex(new Vector3(-1,  1,  1), new ColorRGB(1, 1, 1)));

            triangles.Add(new Triangle(0, 1, 1));
            triangles.Add(new Triangle(1, 2, 2));
            triangles.Add(new Triangle(2, 3, 3));
            triangles.Add(new Triangle(3, 0, 0));

            triangles.Add(new Triangle(4, 5, 5));
            triangles.Add(new Triangle(5, 6, 6));
            triangles.Add(new Triangle(6, 7, 7));
            triangles.Add(new Triangle(7, 4, 4));

            triangles.Add(new Triangle(0, 4, 4));
            triangles.Add(new Triangle(1, 5, 5));
            triangles.Add(new Triangle(2, 6, 6));
            triangles.Add(new Triangle(3, 7, 7));
        }
    }
}
