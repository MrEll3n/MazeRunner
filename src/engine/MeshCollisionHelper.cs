using OpenTK.Mathematics;

namespace ZPG
{
    public static class MeshCollisionHelper
    {
        public static bool SphereIntersectsTriangle(Vector3 center, float radius, Vector3 a, Vector3 b, Vector3 c, out Vector3 pushOut)
        {
            pushOut = Vector3.Zero;

            // Najdeme nejbližší bod na trojúhelníku
            Vector3 closest = ClosestPointOnTriangle(center, a, b, c);
            Vector3 diff = center - closest;
            float distSq = diff.LengthSquared;

            if (distSq < radius * radius)
            {
                float dist = MathF.Sqrt(distSq);
                Vector3 normal = dist > 0 ? diff / dist : Vector3.UnitY;
                float penetration = radius - dist;
                pushOut = normal * penetration;
                return true;
            }

            return false;
        }

        private static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            // Möller–Trumbore styl výpočtu
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0 && d2 <= 0) return a;

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0 && d4 <= d3) return b;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                float v = d1 / (d1 - d3);
                return a + ab * v;
            }

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0 && d5 <= d6) return c;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                float w = d2 / (d2 - d6);
                return a + ac * w;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + (c - b) * w;
            }

            Vector3 n = Vector3.Cross(ab, ac);
            float denom = Vector3.Dot(n, n);
            float u = Vector3.Dot(Vector3.Cross(ap, ac), n) / denom;
            float v2 = Vector3.Dot(Vector3.Cross(ab, ap), n) / denom;

            return a + ab * u + ac * v2;
        }
    }
}
