using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Helper class for detecting collisions between spherical objects and triangle meshes.
    /// </summary>
    public static class MeshCollisionHelper
    {
        /// <summary>
        /// Detects a collision between a sphere and a triangle in space.
        /// </summary>
        /// <param name="center">Center of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="a">Vertex A of the triangle.</param>
        /// <param name="b">Vertex B of the triangle.</param>
        /// <param name="c">Vertex C of the triangle.</param>
        /// <param name="pushOut">Output vector to push the sphere out of collision.</param>
        /// <returns>True if a collision occurred.</returns>
        public static bool SphereIntersectsTriangle(Vector3 center, float radius, Vector3 a, Vector3 b, Vector3 c, out Vector3 pushOut)
        {
            pushOut = Vector3.Zero;

            // Najdi nejbližší bod na trojúhelníku k centru koule
            Vector3 closest = ClosestPointOnTriangle(center, a, b, c);
            Vector3 diff = center - closest;
            float distSq = diff.LengthSquared;

            // Pokud je vzdálenost menší než poloměr, nastala kolize
            if (distSq < radius * radius)
            {
                float dist = MathF.Sqrt(distSq);
                Vector3 normal = dist > 0 ? diff / dist : Vector3.UnitY; // pokud je střed přímo na trojúhelníku
                float penetration = radius - dist;
                pushOut = normal * penetration; // vektor, kterým se koule vytlačí z kolize
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the closest point on a triangle to a given point in space.
        /// </summary>
        /// <param name="p">The point to which the closest point on the triangle is sought.</param>
        /// <param name="a">Vertex A of the triangle.</param>
        /// <param name="b">Vertex B of the triangle.</param>
        /// <param name="c">Vertex C of the triangle.</param>
        /// <returns>The closest point on the triangle to the point <paramref name="p"/>.</returns>
        private static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            // Vektory stran
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;

            // Test oblasti okolo vrcholu A
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0 && d2 <= 0) return a;

            // Test oblasti okolo vrcholu B
            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0 && d4 <= d3) return b;

            // Test oblasti na hraně AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                float v = d1 / (d1 - d3);
                return a + ab * v;
            }

            // Test oblasti okolo vrcholu C
            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0 && d5 <= d6) return c;

            // Test oblasti na hraně AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                float w = d2 / (d2 - d6);
                return a + ac * w;
            }

            // Test oblasti na hraně BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + (c - b) * w;
            }

            // Bod je uvnitř trojúhelníku – použij barycentrickou interpolaci
            Vector3 n = Vector3.Cross(ab, ac);
            float denom = Vector3.Dot(n, n);
            float u = Vector3.Dot(Vector3.Cross(ap, ac), n) / denom;
            float v2 = Vector3.Dot(Vector3.Cross(ab, ap), n) / denom;

            return a + ab * u + ac * v2;
        }
    }
}
