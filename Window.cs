using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZPG
{
    public class Window : GameWindow
    {
        public MapReader mapReader;
        public Viewport viewport;
        public Matrix4 projection = new Matrix4();
        public Matrix4 view = new Matrix4();

        private Player player;

        public int Width { get; private set; } = 800;
        public int Height { get; private set; } = 600;

        private string[] _args { get; set; }

        private bool _mouseGrabbed = true;
        private float vpScale = 1.0f;

        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CursorState = CursorState.Grabbed;
            _args = args;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Alt)
            {
                switch (e.Key)
                {
                    case Keys.Q:
                        Close();
                        break;
                    case Keys.Enter:
                        if (WindowState == WindowState.Fullscreen)
                        {
                            SetWindowedMode();
                        }
                        else
                        {
                            SetFullscreenMode();
                        }
                        break;
                }
            }

            if (e.Key == Keys.Escape)
            {
                CursorState = _mouseGrabbed ? CursorState.Normal : CursorState.Grabbed;
                _mouseGrabbed = !_mouseGrabbed;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.LoadBindings(new GLFWBindingsContext());

            // Argument parsing
            for (int i = 0; i < _args.Length; ++i)
            {
                if (_args[i] == "--fullscreen")
                {
                    SetFullscreenMode();
                    CursorState = CursorState.Grabbed;
                }

                if (_args[i] == "--mac") { vpScale = 2.0f; }
            }

            // Viewport initialization
            viewport = new Viewport()
            {
                Top = 0,
                Left = 0,
                Width = 1 * vpScale,
                Height = 1 * vpScale,
                Window = this
            };

            // Shader initialization
            Shader shader = new Shader("shaders/basic.vert", "shaders/basic.frag");

            // Map initialization
            mapReader = new MapReader(shader);
            mapReader.ReadFile("map.txt");

            // Player initialization
            Vector3 startPosition = mapReader.GetPlayerStartPosition();
            player = new Player(startPosition, viewport.AspectRatio);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_mouseGrabbed) return;

            player.Camera.RotateX(-e.DeltaY / 250f);
            player.Camera.RotateY(-e.DeltaX / 250f);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            player.Camera.ChangeFOV(e.OffsetY < 0 ? -1.0f : +1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            viewport.Set();
            viewport.Clear();

            GL.PointSize(10);
            mapReader.GetWalls().ForEach(wall => wall.Draw(player.Camera));

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float dt = (float)args.Time;
            Vector3 input = Vector3.Zero;

            if (KeyboardState.IsKeyDown(Keys.W)) input.Z += 1;
            if (KeyboardState.IsKeyDown(Keys.S)) input.Z -= 1;
            if (KeyboardState.IsKeyDown(Keys.A)) input.X -= 1;
            if (KeyboardState.IsKeyDown(Keys.D)) input.X += 1;

            if (input.LengthSquared > 0)
            {
                Vector3 dir = (player.Camera.Front * input.Z + player.Camera.Right * input.X).Normalized();
                player.AddForce(dir * 100);
            }

            if (KeyboardState.IsKeyDown(Keys.Space))
                player.Jump(200);

            player.Update(dt);
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

        private void SetFullscreenMode()
        {
            WindowState = WindowState.Fullscreen;
            if (_args.Contains("--mac"))
            {
                vpScale = 1.0f;
                viewport = new Viewport()
                {
                    Top = 0,
                    Left = 0,
                    Width = 1 * vpScale,
                    Height = 1 * vpScale,
                    Window = this
                };
            }
        }

        private void SetWindowedMode()
        {
            WindowState = WindowState.Normal;
            if (_args.Contains("--mac"))
            {
                vpScale = 2.0f;
                viewport = new Viewport()
                {
                    Top = 0,
                    Left = 0,
                    Width = 1 * vpScale,
                    Height = 1 * vpScale,
                    Window = this
                };
            }
        }
    }
}
