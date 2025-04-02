using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZPG
{
    public class Window : GameWindow
    {
        // Map reader and viewport for rendering
        public MapReader mapReader;
        public Viewport viewport;

        // Projection and view matrices
        public Matrix4 projection = new Matrix4();
        public Matrix4 view = new Matrix4();

        // Player object
        private Player player;

        // Window dimensions
        public int Width { get; private set; } = 800;
        public int Height { get; private set; } = 600;

        // Command-line arguments
        private string[] _args { get; set; }

        // Mouse state and viewport scale
        private bool _mouseGrabbed = true;
        private float vpScale = 1.0f;

        // Jumping state
        private bool isJumping = false;

        // Constructor
        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CursorState = CursorState.Grabbed; // Lock the cursor initially
            _args = args;
        }

        // Handle key press events
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Handle Alt + Q and Alt + Enter for quitting and toggling fullscreen
            if (e.Alt)
            {
                switch (e.Key)
                {
                    case Keys.Q:
                        Close(); // Close the window
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

            // Toggle mouse grab with Escape
            if (e.Key == Keys.Escape)
            {
                CursorState = _mouseGrabbed ? CursorState.Normal : CursorState.Grabbed;
                _mouseGrabbed = !_mouseGrabbed;
            }

            // Start jumping when Space is pressed
            if (e.Key == Keys.Space)
            {
                isJumping = true;
            }
        }

        // Handle key release events
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            // Stop jumping when Space is released
            if (e.Key == Keys.Space)
            {
                isJumping = false;
            }
        }

        // Load resources and initialize the game
        protected override void OnLoad()
        {
            base.OnLoad();

            // Load OpenGL bindings
            GL.LoadBindings(new GLFWBindingsContext());

            // Enable depth testing and backface culling
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Parse command-line arguments
            for (int i = 0; i < _args.Length; ++i)
            {
                if (_args[i] == "--fullscreen")
                {
                    SetFullscreenMode();
                    CursorState = CursorState.Grabbed;
                }

                if (_args[i] == "--mac") { vpScale = 2.0f; }
            }

            // Initialize viewport
            viewport = new Viewport()
            {
                Top = 0,
                Left = 0,
                Width = 1 * vpScale,
                Height = 1 * vpScale,
                Window = this
            };

            // Load shaders
            Shader shader = new Shader("shaders/basic.vert", "shaders/basic.frag");

            // Initialize map reader and load the map
            mapReader = new MapReader(shader);
            mapReader.ReadFile("map.txt");

            // Get the player's starting position from the map
            Vector3 startPosition = mapReader.GetPlayerStartPosition();
            player = new Player(startPosition, viewport.AspectRatio);
        }

        // Handle mouse movement for camera rotation
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_mouseGrabbed) return;

            player.Camera.RotateX(-e.DeltaY / 250f); // Rotate camera vertically
            player.Camera.RotateY(-e.DeltaX / 250f); // Rotate camera horizontally
        }

        // Handle mouse wheel for changing the field of view
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            player.Camera.ChangeFOV(e.OffsetY < 0 ? +1.0f : -1.0f);
        }

        // Render the frame
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            viewport.Set(); // Set the viewport
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Clear buffers

            //GL.PointSize(0); // Set point size for rendering
            mapReader.GetWalls().ForEach(wall => wall.Draw(player.Camera)); // Draw walls

            SwapBuffers(); // Swap the front and back buffers
        }

        // Update the game state
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float playerSpeed = 4f; // Default player speed

            base.OnUpdateFrame(args);

            float dt = (float)args.Time; // Delta time

            // Handle player movement input
            Vector3 input = Vector3.Zero;
            if (KeyboardState.IsKeyDown(Keys.W)) input.Z += 1;
            if (KeyboardState.IsKeyDown(Keys.S)) input.Z -= 1;
            if (KeyboardState.IsKeyDown(Keys.A)) input.X -= 1;
            if (KeyboardState.IsKeyDown(Keys.D)) input.X += 1;

            // Increase speed when Left Shift is held
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) playerSpeed *= 1.8f;

            // Calculate movement direction
            Vector3 camForward = player.Camera.Front;
            Vector3 flatForward = new Vector3(camForward.X, 0, camForward.Z).Normalized();
            Vector3 flatRight = Vector3.Cross(flatForward, Vector3.UnitY).Normalized();

            if (input.LengthSquared > 0)
            {
                Vector3 moveDir = (flatForward * input.Z + flatRight * input.X).Normalized();
                player.MoveToward(moveDir * playerSpeed); // Move the player
            }

            // Handle jumping
            if (isJumping && player.IsOnGround)
            {
                player.Jump(5f);
            }

            player.Update(dt); // Update the player
        }

        // Handle window resizing
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Width = e.Width;
            Height = e.Height;
        }

        // Unload resources
        protected override void OnUnload()
        {
            base.OnUnload();
        }

        // Set fullscreen mode
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

        // Set windowed mode
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
