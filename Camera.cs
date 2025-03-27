using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ZPG
{
    public class Camera
    {
        private float zoom = 1;
        public float z = 0;
        public float x = 0;
        public float y = 0;
        public float rx = 0;
        public float ry = 0;

        public Viewport Viewport;
        public Camera(Viewport Viewport)
        {
            this.Viewport = Viewport;
        }

        public virtual Matrix4 Projection
        {
            get
            {
                float ratio = (float)((Viewport.Width * Viewport.Window.Width) / (Viewport.Height * Viewport.Window.Height));
                return Matrix4.CreatePerspectiveFieldOfView(zoom, ratio, 1f, 10);
            }
        }

        public void Zoom(float coef)
        {
            //zoom *= coef;
            zoom += coef / 10.0f;
            zoom = Math.Max(0.2f, Math.Min(2, zoom));
            Console.WriteLine(zoom);
        }

        public void Move(float x, float y)
        {
            this.x += (float)(x * Math.Cos(ry) + y * Math.Sin(ry));
            this.z += (float)(x * Math.Sin(ry) - y * Math.Cos(ry));
        }

        public void RotateX(float a)
        {
            rx += a;
            rx = (float)Math.Max(-Math.PI / 2, Math.Min(Math.PI / 2, rx));
        }

        public void RotateY(float a)
        {
            ry += a;
        }


        public virtual Matrix4 View
        {
            get
            {
                Matrix4 view;
                view = Matrix4.Identity;
                view *= Matrix4.CreateTranslation(-x, -y, -z);
                view *= Matrix4.CreateRotationY(ry);
                view *= Matrix4.CreateRotationX(rx);
                return view;
            }

        }
    }
}
