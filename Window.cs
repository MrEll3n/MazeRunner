using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZPG
{
    public class Window : GameWindow
    {
        public Model model;
        public Cube cube;
        public Viewport viewport = new Viewport();
        public Camera camera;
        public Matrix4 projection = new Matrix4();
        public Matrix4 view = new Matrix4();

		private int _width = 0;
		public int Width => _width;

		private int _height = 0;
		public int Height => _height;

        bool keyMoveLeft, keyMoveRight;
        bool keyMoveUp, keyMoveDown;
        bool keyRotateLeft, keyRotateRight;
        bool keyRotateUp, keyRotateDown;

        bool editmode = true;

        public Window() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Keys.F1:
                    {
                        CursorState = editmode ? CursorState.Grabbed : CursorState.Normal;
                        editmode = !editmode;
                        break;
                    }
                case Keys.A: camera.Move(-0.1f, 0); break;
                case Keys.D: camera.Move(0.1f, 0); break;
                case Keys.S: camera.Move(0, -0.1f); break;
                case Keys.W: camera.Move(0, 0.1f); break;
                case Keys.D4: camera.RotateY(-0.1f); break;
                case Keys.D8: camera.RotateX(-0.1f); break;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

			GL.LoadBindings(new GLFWBindingsContext());

            viewport = new Viewport()
            {
                Top = 0,
                Left = 0,
                Width = 1,
                Height = 1,
                Window = this
            };

            camera = new Camera(viewport);

            model = new Model();
            model.vertices.Add(new Vertex(new Vector3(-0.9, -0.9, 0), new ColorRGB(1, 0, 0)));
            model.vertices.Add(new Vertex(new Vector3(0.9, 0.9, 0), new ColorRGB(1, 0, 0)));
            model.vertices.Add(new Vertex(new Vector3(-0.9, 0.8, 0), new ColorRGB(1, 1, 0)));
            model.vertices.Add(new Vertex(new Vector3(0.9, -0.8, 0), new ColorRGB(1, 1, 0)));
            model.triangles.Add(new Triangle(0, 1, 2));
            model.triangles.Add(new Triangle(1, 3, 0));

            cube = new Cube();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (editmode) return;

			camera.RotateX(e.DeltaY / 250f);
			camera.RotateY(e.DeltaX / 250f);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

			camera.Zoom(e.OffsetY < 0 ? 0.9f : 1.1f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            viewport.Set();
            viewport.Clear();
            camera.SetProjection();
            camera.SetView();

            GL.PointSize(10);
            cube.Draw();

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
			_width = e.Width;
			_height = e.Height;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }
    }
}
