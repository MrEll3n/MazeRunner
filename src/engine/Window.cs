using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;
using System.Linq;

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
        private bool _mouseGrabbed = true;
        private float vpScale = 1.0f;
        private bool isJumping = false;
        private int frameCount = 0;
        private double fpsTimer = 0;

        private int wallTexture, floorTexture, ceilingTexture;
        private Shader basicShader;
        private Shader fadeShader;
        private Shader transShader;
        private FadeOverlay teleportFadeOverlay;
        private string[] _args { get; set; }

        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            _args = args;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.LoadBindings(new GLFWBindingsContext());

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.FrontFace(FrontFaceDirection.Ccw);

            for (int i = 0; i < _args.Length; ++i)
            {
                if (_args[i] == "--fullscreen")
                    SetFullscreenMode();
                if (_args[i] == "--mac")
                    vpScale = 2.0f;
            }

            viewport = new Viewport() { Top = 0, Left = 0, Width = 1 * vpScale, Height = 1 * vpScale, Window = this };

            basicShader = new Shader("shaders/basic.vert", "shaders/basic.frag");
            fadeShader = new Shader("shaders/fade.vert", "shaders/fade.frag");
            transShader = new Shader("shaders/basic.vert", "shaders/transparent.frag");
            teleportFadeOverlay = new FadeOverlay { Shader = fadeShader };

            wallTexture = TextureLoader.LoadTexture("textures/wall.png");
            floorTexture = TextureLoader.LoadTexture("textures/carpet.jpg");
            ceilingTexture = TextureLoader.LoadTexture("textures/ceiling.png");
            int teleportTextureID = TextureLoader.LoadTexture("textures/teleport.jpg");
            if (teleportTextureID == 0) teleportTextureID = wallTexture;

            mapReader = new MapReader(basicShader);
            mapReader.ReadFile("map.txt");

            foreach (var wall in mapReader.GetWalls())
            {
                wall.Shader = basicShader;
                wall.TextureID = wallTexture;
            }

            foreach (var trigger in mapReader.GetTeleportTriggers())
            {
                trigger.Shader = transShader;
                trigger.TextureID = teleportTextureID;
                trigger.FadeOverlay = teleportFadeOverlay;
                trigger.Transparency = 0.65f;
            }

            mapReader.GetRenderables().Add(new Quad(128, 128, 0f, true) { Shader = basicShader, TextureID = floorTexture });
            mapReader.GetRenderables().Add(new Quad(128, 128, 3f, false) { Shader = basicShader, TextureID = ceilingTexture });

            player = new Player(mapReader.GetPlayerStartPosition(), this);
            player.TriggerModels = mapReader.GetTeleportTriggers().Cast<Model>().ToList();

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            viewport.Set();
            viewport.Clear();

            // First render opaque objects
            basicShader.Use();
            player.Flashlight.Apply(basicShader);

            // Draw all non-transparent objects
            foreach (var renderable in mapReader.GetRenderables())
            {
                if (!(renderable is TeleportTrigger)) // Skip teleport triggers
                    renderable.Draw(player.Camera);
            }

            // Now render transparent objects with proper settings
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(false); // Don't write to depth buffer for transparent objects

            transShader.Use();
            player.Flashlight.Apply(transShader);

            // Draw teleport triggers with transparency
            foreach (var trigger in mapReader.GetTeleportTriggers())
            {
                trigger.Draw(player.Camera);
            }

            // Reset depth mask
            GL.DepthMask(true);

            // Draw the fade overlay
            fadeShader.Use();
            teleportFadeOverlay.DrawFullScreenQuad(Width, Height);

            SwapBuffers();
            frameCount++;
            fpsTimer += args.Time;
            if (fpsTimer >= 1.0)
            {
                Title = $"MazeRunner - FPS: {frameCount}";
                frameCount = 0;
                fpsTimer = 0;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            float dt = (float)args.Time;

            float playerSpeed = 1.4f;
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) playerSpeed *= 1.8f;

            Vector3 input = Vector3.Zero;
            if (KeyboardState.IsKeyDown(Keys.W)) input.Z += 1;
            if (KeyboardState.IsKeyDown(Keys.S)) input.Z -= 1;
            if (KeyboardState.IsKeyDown(Keys.A)) input.X += 1;
            if (KeyboardState.IsKeyDown(Keys.D)) input.X -= 1;

            if (input.LengthSquared > 0)
            {
                input = input.Normalized();
                Vector3 camForward = player.Camera.Front;
                Vector3 flatForward = new Vector3(camForward.X, 0, camForward.Z).Normalized();
                Vector3 flatRight = Vector3.Cross(Vector3.UnitY, flatForward).Normalized();
                Vector3 moveDir = flatForward * input.Z + flatRight * input.X;
                Vector3 desiredVelocity = moveDir * playerSpeed;
                player.MoveToward(desiredVelocity);
            }

            if (isJumping && player.IsOnGround) player.Jump(5f);

            player.Controller.ApplyInputControl();
            Vector3 totalForce = player.Controller.ConsumeForces();
            player.Acceleration = totalForce / player.Mass;
            player.Velocity += player.Acceleration * dt;
            player.Controller.ClearInput();

            if (player.IsOnGround && input.LengthSquared == 0)
            {
                Vector3 h = new(player.Velocity.X, 0, player.Velocity.Z);
                h -= h * player.Controller.GroundFriction * dt;
                player.Velocity = new Vector3(h.X, player.Velocity.Y, h.Z);
            }

            if (!player.IsOnGround)
            {
                Vector3 h = new(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 drag = h * 2.0f;
                player.Velocity -= new Vector3(drag.X, 0, drag.Z) * dt;
            }

            player.Controller.CurrentWalls = mapReader.GetWalls();
            player.MoveAndSlideMeshBased(player.Velocity, player.Controller.CurrentWalls, dt);
            player.IsOnGround = player.Position.Y <= 0.01f;
            if (player.IsOnGround)
            {
                player.Position = new Vector3(player.Position.X, 0.01f, player.Position.Z);
                player.Velocity = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
            }

            foreach (var trigger in mapReader.GetTeleportTriggers())
            {
                if (trigger.IsColliding(player)) trigger.OnPlayerEnter(player);
                else trigger.OnPlayerExit(player);
            }
            foreach (var trigger in mapReader.GetTeleportTriggers()) trigger.Update(dt);
            player.UpdateCamera();
            teleportFadeOverlay.Update(dt);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            //Console.WriteLine($"Mouse delta: {e.Delta}");
            player.Camera.RotateY(-e.Delta.X * 0.002f);
            player.Camera.RotateX(-e.Delta.Y * 0.002f);
        }

        public void StartTeleportFade(Action onFadeComplete) => teleportFadeOverlay.StartFade(onFadeComplete);

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Width = e.Width;
            Height = e.Height;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            //Console.WriteLine($"Key pressed: {e.Key}");

            if (e.Alt && e.Key == Keys.Enter)
            {
                if (WindowState == WindowState.Fullscreen) SetWindowedMode();
                else SetFullscreenMode();
            }
            if (e.Alt && e.Key == Keys.Q)
            {
                Close();
            }
            if (e.Key == Keys.Escape)
            {
                CursorState = _mouseGrabbed ? CursorState.Normal : CursorState.Grabbed;
                _mouseGrabbed = !_mouseGrabbed;
            }
            if (e.Key == Keys.Space) isJumping = true;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Keys.Space) isJumping = false;
        }

        private void SetFullscreenMode()
        {
            WindowState = WindowState.Fullscreen;
            if (_args.Contains("--mac")) vpScale = 1.0f;
        }

        private void SetWindowedMode()
        {
            WindowState = WindowState.Normal;
            if (_args.Contains("--mac")) vpScale = 2.0f;
        }
    }
}
