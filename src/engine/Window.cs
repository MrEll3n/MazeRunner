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
    /// <summary>
    /// Main game window class for MazeRunner. Handles rendering, input, physics, and scene logic.
    /// </summary>
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
        private int frameCountFinal = 0;

        private int wallTexture, floorTexture, ceilingTexture, collectibleTexture;
        private HudRenderer hud;
        private Shader basicShader;
        private Shader fadeShader;
        private Shader transShader;
        private Shader collectibleShader;
        private FadeOverlay teleportFadeOverlay;
        
        private string[] _args { get; set; }
        private int collectedCount = 0;

        /// <summary>
        /// Creates a new instance of the game window with optional startup arguments.
        /// </summary>
        /// <param name="args">Startup arguments, e.g., --fullscreen or --mac.</param>
        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            _args = args;
        }

        /// <summary>
        /// Initializes OpenGL state, loads shaders, textures, and sets up the map and player.
        /// </summary>
        protected override void OnLoad() {
            base.OnLoad();
            GL.LoadBindings(new GLFWBindingsContext());

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.FrontFace(FrontFaceDirection.Ccw);

            for (int i = 0; i < _args.Length; ++i) {
                if (_args[i] == "--fullscreen")
                    SetFullscreenMode();
                if (_args[i] == "--mac")
                    vpScale = 2.0f;
            }

            viewport = new Viewport() { Top = 0, Left = 0, Width = 1 * vpScale, Height = 1 * vpScale, Window = this };

            basicShader = new Shader("shaders/basic.vert", "shaders/basic.frag");
            fadeShader = new Shader("shaders/fade.vert", "shaders/fade.frag");
            transShader = new Shader("shaders/basic.vert", "shaders/transparent.frag");
            collectibleShader = new Shader("shaders/basic.vert", "shaders/collectible.frag");
            teleportFadeOverlay = new FadeOverlay { Shader = fadeShader };

            wallTexture = TextureLoader.LoadTexture("textures/wall.png");
            floorTexture = TextureLoader.LoadTexture("textures/carpet.jpg");
            ceilingTexture = TextureLoader.LoadTexture("textures/ceiling.png");
            int teleportTextureID = TextureLoader.LoadTexture("textures/teleport.jpg");
            if (teleportTextureID == 0) teleportTextureID = wallTexture;
            collectibleTexture = TextureLoader.LoadTexture("textures/paper.png");
            
            hud = new HudRenderer("textures/basic_font.png");

            mapReader = new MapReader(basicShader);
            mapReader.ReadFile("map.txt");

            foreach (var wall in mapReader.GetWalls()) {
                wall.Shader = basicShader;
                wall.TextureID = wallTexture;
            }

            foreach (var trigger in mapReader.GetTeleportTriggers()) {
                trigger.Shader = transShader;
                trigger.TextureID = teleportTextureID;
                trigger.FadeOverlay = teleportFadeOverlay;
                trigger.Transparency = 0.65f;
            }

            foreach (var collectible in mapReader.GetCollectibles()) {
                collectible.Shader = collectibleShader;
                collectible.TextureID = collectibleTexture;
            }

            mapReader.GetRenderables().Add(
                new Quad(
                    center: new Vector3(0, 0f, 0),
                    rightDir: Vector3.UnitX,               
                    upDir: Vector3.UnitZ,                  
                    width: 128f,
                    height: 128f,
                    flip: true
                )
                {
                    Shader = basicShader,
                    TextureID = floorTexture
                }
            );

            mapReader.GetRenderables().Add(
                new Quad(
                    center: new Vector3(0, 3f, 0),
                    rightDir: Vector3.UnitX,
                    upDir: Vector3.UnitZ,
                    width: 128f,
                    height: 128f,
                    flip: false
                )
                {
                    Shader = basicShader,
                    TextureID = ceilingTexture
                }
            );

            player = new Player(mapReader.GetPlayerStartPosition(), this);
            player.TriggerModels = mapReader.GetTeleportTriggers().Cast<Model>().ToList();

            CursorState = CursorState.Grabbed;
            
            SoundManager.Instance.PlayMusic("assets/music/zpg_theme_ost.wav");
        }

        /// <summary>
        /// Handles rendering of the scene every frame including HUD and fade overlays.
        /// </summary>
        /// <param name="args">Frame timing arguments.</param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            viewport.Set();
            viewport.Clear();

            basicShader.Use();
            player.Flashlight.Apply(basicShader);

            foreach (var renderable in mapReader.GetRenderables())
            {
                if (!(renderable is TeleportTrigger or Collectible))
                    renderable.Draw(player.Camera);
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(false);

            transShader.Use();
            player.Flashlight.Apply(transShader);
            player.Flashlight.Apply(collectibleShader);

            foreach (var transparent in mapReader.GetTeleportTriggers().Cast<Model>().Concat(mapReader.GetCollectibles()))
            {
                if (transparent is Collectible c && c.IsCollected)
                    continue;
                transparent.Draw(player.Camera);
            }

            hud.DrawText($"Papers: {collectedCount}", 0.02f, 0.02f, 2f, Width, Height, align: TextAlign.Left);
            hud.DrawText($"{frameCountFinal}", 0.98f, 0.95f, 1f, Width, Height, align: TextAlign.Right);
            

            GL.DepthMask(true);
            
            

            fadeShader.Use();
            teleportFadeOverlay.DrawFullScreenQuad(Width, Height);
            

            SwapBuffers();
            frameCount++;
            fpsTimer += args.Time;
            if (fpsTimer >= 1.0)
            {
                Title = $"MazeRunner - FPS: {frameCount}, Collected: {collectedCount}";
                frameCountFinal = frameCount;
                frameCount = 0;
                fpsTimer = 0;
            }
        }

        /// <summary>
        /// Updates physics, player movement, triggers, collectibles, and sound state.
        /// </summary>
        /// <param name="args">Frame timing arguments.</param>
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

            foreach (var collectible in mapReader.GetCollectibles())
            {
                if (!collectible.IsCollected)
                {
                    collectible.Update(dt);
                    collectible.CheckTrigger(player);
                    if (collectible.IsCollected)
                    {
                        collectedCount++;
                        Console.WriteLine($"[INFO] Collected: {collectedCount}");
                    }
                }
            }

            player.UpdateCamera(dt);
            teleportFadeOverlay.Update(dt);
            
            Vector2 hVel = new(player.Velocity.X, player.Velocity.Z);
            SoundManager.Instance.UpdateWalking(player.Position, hVel.Length, dt);
        }

        /// <summary>
        /// Handles mouse movement input for rotating the camera.
        /// </summary>
        /// <param name="e">Mouse movement event data.</param>
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            player.Camera.RotateY(-e.Delta.X * 0.002f);
            player.Camera.RotateX(-e.Delta.Y * 0.002f);
        }

        /// <summary>
        /// Triggers a full-screen fade animation used during teleportation.
        /// </summary>
        /// <param name="onFadeComplete">Callback to run after fade-in completes.</param>
        public void StartTeleportFade(Action onFadeComplete) => teleportFadeOverlay.StartFade(onFadeComplete);

        /// <summary>
        /// Handles window resizing and updates internal dimensions.
        /// </summary>
        /// <param name="e">Resize event data.</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Width = e.Width;
            Height = e.Height;
        }

        /// <summary>
        /// Handles key press events for toggling fullscreen, mouse grabbing, jumping, and quitting.
        /// </summary>
        /// <param name="e">Key event data.</param>
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

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

        /// <summary>
        /// Handles key release events such as stopping jump activation.
        /// </summary>
        /// <param name="e">Key event data.</param>
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Keys.Space) isJumping = false;
        }

        /// <summary>
        /// Switches the game window to fullscreen mode and adjusts viewport scale for macOS.
        /// </summary>
        private void SetFullscreenMode()
        {
            WindowState = WindowState.Fullscreen;
            if (_args.Contains("--mac")) vpScale = 1.0f;
        }

        /// <summary>
        /// Switches the game window to windowed mode and adjusts viewport scale for macOS.
        /// </summary>
        private void SetWindowedMode()
        {
            WindowState = WindowState.Normal;
            if (_args.Contains("--mac")) vpScale = 2.0f;
        }
    }
}