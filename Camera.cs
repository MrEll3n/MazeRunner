using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ZPG
{
    public class Camera
    {
        private float scale = 1;         
        float x = 0;
        float y = 0;
        float z = 0;
        float rx = 0;
        float ry = 0;
        float fov = 0.9f;

        public Viewport viewport;
        public Camera(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void SetProjection()
        {
            float ratio = (float)((viewport.Width * viewport.Window.Width) / (viewport.Height * viewport.Window.Height));
            //Matrix4 projection = Matrix4.CreateOrthographic(scale*2, scale * 2 / ratio, -10, 10);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        public void Zoom(float coef)
        {
            fov *= coef;
            fov = (float)Math.Max(Math.Min(fov, Math.PI * 0.9), 0.1);
        }

        public void Move(float x, float y)
        {
            this.x -= (float)(x * Math.Cos(ry) + y * Math.Sin(ry));
            this.y -= (float)(-y * Math.Cos(ry) + x * Math.Sin(ry)); ;
        }

        public void RotateX(float a)
        {
            rx += a;
            rx = (float)Math.Max(Math.Min(rx, Math.PI / 2), -Math.PI / 2);
        }

        public void RotateY(float a)
        {
            ry += a;
        }

        public void SetView()
        {
            Matrix4 view;
            view = Matrix4.Identity;
            view *= Matrix4.CreateTranslation(-x, -y, -z);
            view *= Matrix4.CreateRotationY(ry);
            view *= Matrix4.CreateRotationX(rx);
            GL.MatrixMode(MatrixMode.Modelview);
            
            GL.LoadMatrix(ref view);
        }

    }
}
