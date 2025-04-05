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

        private bool isJumping = false;

        private int wallTexture; // textura pro stěny

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

            if (e.Key == Keys.Space)
            {
                isJumping = true;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Keys.Space)
            {
                isJumping = false;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.LoadBindings(new GLFWBindingsContext());

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            for (int i = 0; i < _args.Length; ++i)
            {
                if (_args[i] == "--fullscreen")
                {
                    SetFullscreenMode();
                    CursorState = CursorState.Grabbed;
                }

                if (_args[i] == "--mac") { vpScale = 2.0f; }
            }

            viewport = new Viewport()
            {
                Top = 0,
                Left = 0,
                Width = 1 * vpScale,
                Height = 1 * vpScale,
                Window = this
            };

            // Shader
            Shader shader = new Shader("shaders/basic.vert", "shaders/basic.frag");

            // Načti mapu
            mapReader = new MapReader(shader);
            mapReader.ReadFile("map.txt");

            if (mapReader.GetWalls().Count == 0)
            {
                Console.WriteLine("No walls found in map, creating test wall");
                Wall testWall = new Wall()
                {
                    Shader = shader,
                    Position = new Vector3(0, 0, -5), // 5 units in front of camera
                    TextureID = wallTexture
                };
                mapReader.GetWalls().Add(testWall);
            }

            this.wallTexture = TextureLoader.LoadTexture("textures/wall.png");

            foreach (var wall in mapReader.GetWalls())
            {
                wall.Shader = shader;
                wall.TextureID = wallTexture;
            }

            // Hráč
            Vector3 startPosition = mapReader.GetPlayerStartPosition();
            player = new Player(startPosition, this);

            Console.WriteLine($"Wall texture path exists: {File.Exists("textures/wall.png")}");
            Console.WriteLine($"Map file exists: {File.Exists("map.txt")}");
            Console.WriteLine($"Loaded {mapReader.GetWalls().Count} walls from map");

            // After creating shader in Window.OnLoad
            Console.WriteLine($"Shader ID: {shader.ID}");
            shader.Use();
            shader.SetUniform("model", Matrix4.Identity);
            shader.SetUniform("view", Matrix4.Identity);
            shader.SetUniform("projection", Matrix4.Identity);
            shader.SetUniform("lightPos", player.Position);
            shader.SetUniform("lightDir", player.Camera.Front);
            shader.SetUniform("cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            shader.SetUniform("outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(17.5f)));
            shader.SetUniform("viewPos", player.Camera.Position);
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

            player.Camera.ChangeFOV(e.OffsetY < 0 ? +1.0f : -1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            viewport.Set();
            // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            viewport.Clear();

            mapReader.GetWalls().ForEach(wall => wall.Draw(player.Camera));


            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float playerSpeed = 4f;

            base.OnUpdateFrame(args);

            float dt = (float)args.Time;

            Vector3 input = Vector3.Zero;
            if (KeyboardState.IsKeyDown(Keys.W)) input.Z += 1;
            if (KeyboardState.IsKeyDown(Keys.S)) input.Z -= 1;
            if (KeyboardState.IsKeyDown(Keys.A)) input.X -= 1;
            if (KeyboardState.IsKeyDown(Keys.D)) input.X += 1;

            if (KeyboardState.IsKeyDown(Keys.LeftShift)) playerSpeed *= 1.8f;

            Vector3 camForward = player.Camera.Front;
            Vector3 flatForward = new Vector3(camForward.X, 0, camForward.Z).Normalized();
            Vector3 flatRight = Vector3.Cross(flatForward, Vector3.UnitY).Normalized();

            if (input.LengthSquared > 0)
            {
                Vector3 moveDir = (flatForward * input.Z + flatRight * input.X).Normalized();
                player.MoveToward(moveDir * playerSpeed);
            }

            if (isJumping && player.IsOnGround)
            {
                player.Jump(5f);
            }

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
