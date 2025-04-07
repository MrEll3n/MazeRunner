using OpenTK.Mathematics;

namespace ZPG
{
    public class Quad : Model
    {
        public Quad(float width, float depth, float y, bool flip = false, float tileSize = 3.0f)
        {
            float w = width / 2f;
            float d = depth / 2f;

            float uRepeat = width / tileSize;
            float vRepeat = depth / tileSize;

            Vertices.Add(new Vertex(new Vector3(-w, y, -d), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(w, y, -d), new Vector2(uRepeat, 0)));
            Vertices.Add(new Vertex(new Vector3(w, y, d), new Vector2(uRepeat, vRepeat)));
            Vertices.Add(new Vertex(new Vector3(-w, y, d), new Vector2(0, vRepeat)));

            if (flip)
            {
                Triangles.Add(new Triangle(2, 1, 0));
                Triangles.Add(new Triangle(3, 2, 0));
                foreach (var v in Vertices)
                    v.Normal = -Vector3.UnitY;
            }
            else
            {
                Triangles.Add(new Triangle(0, 1, 2));
                Triangles.Add(new Triangle(0, 2, 3));
                foreach (var v in Vertices)
                    v.Normal = Vector3.UnitY;
            }

            Construct();
        }
    }
}
