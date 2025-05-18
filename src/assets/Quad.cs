using OpenTK.Mathematics;
using System.Collections.Generic;

namespace ZPG
{
    /// <summary>
    /// Obecný 2D quad v prostoru – definovaný centrem, směry a rozměry.
    /// </summary>
    public class Quad : Model
    {
        /// <summary>
        /// Vytvoří nový obecný quad v prostoru.
        /// </summary>
        /// <param name="center">Střed quadu.</param>
        /// <param name="rightDir">Směr "doprava" (šířka).</param>
        /// <param name="upDir">Směr "nahoru" (výška).</param>
        /// <param name="width">Šířka quadu (ve směru rightDir).</param>
        /// <param name="height">Výška quadu (ve směru upDir).</param>
        /// <param name="flip">Otočí normálu a trojúhelníky.</param>
        /// <param name="tileSize">Velikost 1 dlaždice pro opakování textury (v metrech).</param>
        public Quad(Vector3 center, Vector3 rightDir, Vector3 upDir, float width, float height, bool flip = false, float tileSize = 3f, bool uvRepeat = true)
        {
            Vector3 right = rightDir.Normalized() * (width / 2f);
            Vector3 up = upDir.Normalized() * (height / 2f);

            // Lokální osy
            Vector3 p0 = center - right - up; // levý dolní
            Vector3 p1 = center + right - up; // pravý dolní
            Vector3 p2 = center + right + up; // pravý horní
            Vector3 p3 = center - right + up; // levý horní

            if (uvRepeat) {
                float uRepeat = width / tileSize;
                float vRepeat = height / tileSize;

                // UV mapování
                Vertices.Add(new Vertex(p0, new Vector2(0, 0)));
                Vertices.Add(new Vertex(p1, new Vector2(uRepeat, 0)));
                Vertices.Add(new Vertex(p2, new Vector2(uRepeat, vRepeat)));
                Vertices.Add(new Vertex(p3, new Vector2(0, vRepeat)));
            }
            else {
                Vertices.Add(new Vertex(p0, new Vector2(0, 0)));
                Vertices.Add(new Vertex(p1, new Vector2(1, 0)));
                Vertices.Add(new Vertex(p2, new Vector2(1, 1)));
                Vertices.Add(new Vertex(p3, new Vector2(0, 1)));
            }


            if (flip)
            {
                Triangles.Add(new Triangle(2, 1, 0));
                Triangles.Add(new Triangle(3, 2, 0));
            }
            else
            {
                Triangles.Add(new Triangle(0, 1, 2));
                Triangles.Add(new Triangle(0, 2, 3));
            }

            // Normála (kolmá na plochu)
            Vector3 normal = Vector3.Cross(right, up).Normalized();
            if (flip) normal = -normal;

            foreach (var v in Vertices)
                v.Normal = normal;

            Construct(); // Upload na GPU
        }
    }
}