using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje čtvercovou nebo obdélníkovou plochu (quad) v rovině XZ, vhodnou např. pro podlahy nebo stropy.
    /// </summary>
    public class Quad : Model
    {
        /// <summary>
        /// Vytvoří novou instanci třídy <see cref="Quad"/>.
        /// </summary>
        /// <param name="width">Šířka quadu (směrem po ose X).</param>
        /// <param name="depth">Hloubka quadu (směrem po ose Z).</param>
        /// <param name="y">Výška quadu (souřadnice Y všech vrcholů).</param>
        /// <param name="flip">Pokud je true, otočí normály směrem dolů a převrátí trojúhelníky.</param>
        /// <param name="tileSize">Velikost dlaždice pro opakování textury (defaultně 3 jednotky).</param>
        public Quad(float width, float depth, float y, bool flip = false, float tileSize = 3.0f)
        {
            float w = width / 2f;
            float d = depth / 2f;

            // Výpočet počtu opakování textury podle velikosti dlaždice
            float uRepeat = width / tileSize;
            float vRepeat = depth / tileSize;

            // Definice vrcholů s pozicemi a texturovacími souřadnicemi
            Vertices.Add(new Vertex(new Vector3(-w, y, -d), new Vector2(0, 0)));
            Vertices.Add(new Vertex(new Vector3(w, y, -d), new Vector2(uRepeat, 0)));
            Vertices.Add(new Vertex(new Vector3(w, y, d), new Vector2(uRepeat, vRepeat)));
            Vertices.Add(new Vertex(new Vector3(-w, y, d), new Vector2(0, vRepeat)));

            if (flip)
            {
                // Trojúhelníky jsou definovány tak, aby mířily dolů
                Triangles.Add(new Triangle(2, 1, 0));
                Triangles.Add(new Triangle(3, 2, 0));

                // Normály míří dolů (pro strop nebo spodní část objektu)
                foreach (var v in Vertices)
                    v.Normal = -Vector3.UnitY;
            }
            else
            {
                // Trojúhelníky směřují nahoru
                Triangles.Add(new Triangle(0, 1, 2));
                Triangles.Add(new Triangle(0, 2, 3));

                // Normály míří nahoru (pro podlahu nebo horní část)
                foreach (var v in Vertices)
                    v.Normal = Vector3.UnitY;
            }

            // Vytvoření grafické reprezentace modelu (např. nahrání do GPU)
            Construct();
        }
    }
}
