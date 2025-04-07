using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje 3D kameru s možností pohybu, otáčení a změny zorného pole (FOV).
    /// </summary>
    public class Camera
    {
        private float fovDegrees = 90f; // Zorné pole ve stupních
        private float ncp = 0.1f;       // Near clipping plane
        private float fcp = 100f;       // Far clipping plane

        /// <summary>
        /// Pozice kamery ve světovém prostoru.
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>
        /// Rotace kamery kolem osy X (pitch, nahoru/dolů) v radiánech.
        /// </summary>
        public float rx = 0;

        /// <summary>
        /// Rotace kamery kolem osy Y (yaw, vlevo/vpravo) v radiánech.
        /// </summary>
        public float ry = 0;

        /// <summary>
        /// Odkaz na okno pro výpočet poměru stran při tvorbě projekční matice.
        /// </summary>
        public Window Window;

        /// <summary>
        /// Inicializuje kameru s danou počáteční pozicí.
        /// </summary>
        /// <param name="startPosition">Počáteční pozice kamery ve světě.</param>
        public Camera(Vector3 startPosition)
        {
            Position = startPosition;
        }

        /// <summary>
        /// Vrací směrový vektor kamery (dopředu), spočtený z rx a ry.
        /// </summary>
        public Vector3 Front =>
            new Vector3(
                (float)(Math.Cos(rx) * Math.Sin(ry)),
                (float)(Math.Sin(rx)),
                (float)(Math.Cos(rx) * Math.Cos(ry))
            ).Normalized();

        /// <summary>
        /// Vrací pravý vektor kamery (směr doprava), spočtený jako kolmý na Front a Y.
        /// </summary>
        public Vector3 Right => Vector3.Cross(Front, Vector3.UnitY).Normalized();

        /// <summary>
        /// Nastaví nové zorné pole (FOV) ve stupních.
        /// </summary>
        /// <param name="degrees">FOV ve stupních, omezený mezi 30° a 120°.</param>
        public void SetFOV(float degrees)
        {
            fovDegrees = Math.Clamp(degrees, 30f, 120f); // rozumný rozsah
        }

        /// <summary>
        /// Změní aktuální zorné pole o zadaný rozdíl ve stupních.
        /// </summary>
        public void ChangeFOV(float deltaDegrees)
        {
            SetFOV(fovDegrees + deltaDegrees);
        }

        /// <summary>
        /// Otočí kameru kolem osy X (pitch) a omezí naklonění na přirozený rozsah.
        /// </summary>
        /// <param name="a">Úhel v radiánech.</param>
        public void RotateX(float a)
        {
            float oneRad = MathF.PI / 180; // 1 stupeň v radiánech
            rx += a;
            rx = Math.Clamp(rx, (-(MathF.PI / 2) + oneRad), ((MathF.PI / 2) - oneRad));
        }

        /// <summary>
        /// Otočí kameru kolem osy Y (yaw).
        /// </summary>
        /// <param name="a">Úhel v radiánech.</param>
        public void RotateY(float a)
        {
            ry += a;
        }

        /// <summary>
        /// Vrací projekční matici kamery na základě FOV, poměru stran a near/far plane.
        /// </summary>
        public Matrix4 Projection
        {
            get
            {
                float ratio = Window.Width / (float)Window.Height;
                float fovRadians = MathHelper.DegreesToRadians(fovDegrees);
                return Matrix4.CreatePerspectiveFieldOfView(fovRadians, ratio, ncp, fcp);
            }
        }

        /// <summary>
        /// Vrací pohledovou matici kamery (view matrix) směřující ve směru Front.
        /// </summary>
        public Matrix4 View =>
            Matrix4.LookAt(Position, Position + Front, Vector3.UnitY);
    }
}
