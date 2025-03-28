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
        public Cube cube1, cube2, cube3;
        public Viewport viewport; 
        public Camera camera;
        public Matrix4 projection = new Matrix4();
        public Matrix4 view = new Matrix4();

		public int Width {get; private set;}

		public int Height {get; private set;}

        bool keyMoveLeft, keyMoveRight;
        bool keyMoveUp, keyMoveDown;
        bool keyRotateLeft, keyRotateRight;
        bool keyRotateUp, keyRotateDown;

        private string[] _args {set; get; }

        private bool _mouseGrabbed = true;

        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CursorState = CursorState.Grabbed;
            _args = args;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Keys.A: camera.Move(-0.1f, 0); break;
                case Keys.D: camera.Move(0.1f, 0); break;
                case Keys.S: camera.Move(0, -0.1f); break;
                case Keys.W: camera.Move(0, 0.1f); break;
                case Keys.D4: camera.RotateY(-0.1f); break;
                case Keys.D8: camera.RotateX(-0.1f); break;
                case Keys.Escape: CursorState = _mouseGrabbed ? CursorState.Normal : CursorState.Grabbed; _mouseGrabbed = !_mouseGrabbed; break;
            }

            if (e.Alt)
            {
                switch (e.Key)
                {
                    case Keys.Q: // Alt+Q
                        Close();
                        break;
                    case Keys.Enter: // Alt+Enter
                        if (WindowState == WindowState.Fullscreen)
                        {
                            WindowState = WindowState.Normal;
                        }
                        else
                        {
                            WindowState = WindowState.Fullscreen;
                        }
                        break;
                }
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            float vpScale = 1.0f;
			GL.LoadBindings(new GLFWBindingsContext());

            for (int i = 0; i < _args.Length; ++i)
            {
                if (_args[i] == "--fullscreen")
                {
                    WindowState = WindowState.Fullscreen;
                    CursorState = CursorState.Grabbed;
                }

                if (_args[i] == "--mac") {vpScale = 2.0f;}
            }

            viewport = new Viewport()
            {
                Top = 0,
                Left = 0,
                Width = 1*vpScale,
                Height = 1*vpScale,
                Window = this
            };

            camera = new Camera(viewport);
            
            Shader shader = new Shader("shaders/basic.vert", "shaders/basic.frag");
            
            cube1 = new Cube();
            cube1.Shader = shader;
            cube2 = new Cube();
            cube2.Shader = shader;
            cube2.position.X = -0.1;
            cube2.position.Z = -2;
            cube2.position.Y = -2;
            cube3 = new Cube();
            cube3.Shader = shader;
            cube3.position.Z = +2;
            cube3.position.Y = +2;

            model = new Model();
            model.Vertices.Add(new Vertex(new Vector3(-0.9, -0.9, 0), new ColorRGB(1, 0, 0)));
            model.Vertices.Add(new Vertex(new Vector3(0.9, 0.9, 0), new ColorRGB(1, 0, 0)));
            model.Vertices.Add(new Vertex(new Vector3(-0.9, 0.8, 0), new ColorRGB(1, 1, 0)));
            model.Vertices.Add(new Vertex(new Vector3(0.9, -0.8, 0), new ColorRGB(1, 1, 0)));
            model.Triangles.Add(new Triangle(0, 1, 2));
            model.Triangles.Add(new Triangle(1, 3, 0));

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_mouseGrabbed) return;

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
            //camera.SetProjection();
            //camera.SetView();

            GL.PointSize(10);
            cube1.Draw(camera);
            cube2.Draw(camera);
            cube3.Draw(camera);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
			Width = e.Width;
			Height = e.Height;
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
