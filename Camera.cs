using OpenTK.Mathematics;

namespace ZPG
{
    public class Camera
    {
        private float fovDegrees = 90f; // ✅ FOV stored in degrees now
        private float ncp = 0.1f;
        private float fcp = 100f;

        public Vector3 Position { get; set; } = Vector3.Zero;
        public float rx = 0;
        public float ry = 0;

        public Window Window;

        public Camera(Vector3 startPosition)
        {
            Position = startPosition;
        }

        public Vector3 Front =>
            new Vector3(
                (float)(Math.Cos(rx) * Math.Sin(ry)),
                (float)(Math.Sin(rx)),
                (float)(Math.Cos(rx) * Math.Cos(ry))
            ).Normalized();

        public Vector3 Right => Vector3.Cross(Front, Vector3.UnitY).Normalized();

        public void SetFOV(float degrees)
        {
            fovDegrees = Math.Clamp(degrees, 30f, 120f); // ⬅ reasonable range
        }

        public void ChangeFOV(float deltaDegrees)
        {
            SetFOV(fovDegrees + deltaDegrees);
        }

        public void RotateX(float a)
        {   
            float oneRad = MathF.PI/180; // 1 degree in radians
            rx += a;
            rx = Math.Clamp(rx, (-(MathF.PI / 2) +  oneRad), ((MathF.PI / 2) - oneRad));
        }

        public void RotateY(float a)
        {
            ry += a;
        }

        public Matrix4 Projection
        {
            get
            {
                float ratio = Window.Width / (float)Window.Height;
                float fovRadians = MathHelper.DegreesToRadians(fovDegrees);
                return Matrix4.CreatePerspectiveFieldOfView(fovRadians, ratio, ncp, fcp);
            }
        }

        public Matrix4 View =>
            Matrix4.LookAt(Position, Position + Front, Vector3.UnitY);
    }
}
