using OpenTK.Mathematics;
using System.Collections.Generic;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje čtvercovou nebo obdélníkovou plochu (quad)
    /// </summary>
    public class Quad : Model
    {
        /// <summary>
        /// Vytvoří nový quad v rovině XZ, zarovnaný podle Y.
        /// </summary>
        /// <param name="width">Šířka quadu (osa X).</param>
        /// <param name="depth">Hloubka quadu (osa Z).</param>
        /// <param name="y">Výška quadu (osa Y).</param>
        /// <param name="flip">Otočí normály směrem dolů a obrátí trojúhelníky.</param>
        /// <param name="tileSize">Opakování textury v jednotkách.</param>
        public Quad(float width, float depth, float y = 0f, bool flip = false, float tileSize = 3f)
        {
            float halfW = width / 2f;
            float halfD = depth / 2f;

            float uRepeat = width / tileSize;
            float vRepeat = depth / tileSize;

            // Vrcholy (pozice + UV)
            Vertices.Add(new Vertex(new Vector3(-halfW, y, -halfD), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3( halfW, y, -halfD), new Vector2(uRepeat, 0)));
            Vertices.Add(new Vertex(new Vector3( halfW, y,  halfD), new Vector2(uRepeat, vRepeat)));
            Vertices.Add(new Vertex(new Vector3(-halfW, y,  halfD), new Vector2(0, vRepeat)));

            // Trojúhelníky
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

            Construct(); // Nahraje do GPU
        }
    }
}