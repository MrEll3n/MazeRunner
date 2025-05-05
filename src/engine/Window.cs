using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZPG
{
    /// <summary>
    /// Hlavní okno hry – zajišťuje celý životní cyklus hry, vykreslování a vstupy.
    /// </summary>
    public class Window : GameWindow
    {
        /// <summary>Načítání mapy ze souboru a převod znaků na objekty.</summary>
        public MapReader mapReader;

        /// <summary>Viewport určuje oblast vykreslování a poměr stran.</summary>
        public Viewport viewport;

        /// <summary>Projekční matice kamery (perspektiva).</summary>
        public Matrix4 projection = new Matrix4();

        /// <summary>View matice – pozice a rotace kamery ve scéně.</summary>
        public Matrix4 view = new Matrix4();

        /// <summary>Instance hráče – pozice, kamera, pohyb, kolize.</summary>
        private Player player;

        /// <summary>Šířka okna (používá se pro přepočet viewportu).</summary>
        public int Width { get; private set; } = 800;

        /// <summary>Výška okna.</summary>
        public int Height { get; private set; } = 600;

        /// <summary>Přepínač zachycení myši (pro FPS ovládání).</summary>
        private bool _mouseGrabbed = true;

        /// <summary>Měřítko viewportu (např. Retina displeje na Macu).</summary>
        private float vpScale = 1.0f;

        /// <summary>Indikace, že hráč právě drží mezerník (skok).</summary>
        private bool isJumping = false;

        /// <summary>Počítadlo snímků za sekundu.</summary>
        private int frameCount = 0;

        /// <summary>Časová akumulace pro výpočet FPS.</summary>
        private double fpsTimer = 0;

        /// <summary>ID textur použitých ve světě (stěna, podlaha, strop).</summary>
        private int wallTexture, floorTexture, ceilingTexture;

        /// <summary>Předané argumenty při spuštění (např. --fullscreen).</summary>
        private string[] _args { get; set; }

        /// <summary>
        /// Vytvoření hlavního okna s defaultním nastavením a zpracováním argumentů.
        /// </summary>
        public Window(string[] args) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CursorState = CursorState.Grabbed;
            _args = args;
        }

        /// <summary>Zpracování klávesových vstupů (přepnutí fullscreen, myši, skok).</summary>
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

        /// <summary>Zpracování uvolnění klávesy (např. konec skoku).</summary>
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Keys.Space)
            {
                isJumping = false;
            }
        }

        /// <summary>
        /// Inicializace OpenGL, načtení shaderů, mapy, textur a vytvoření objektů.
        /// </summary>
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

            // Texture loading
            this.wallTexture = TextureLoader.LoadTexture("textures/wall.png");
            this.floorTexture = TextureLoader.LoadTexture("textures/carpet.jpg");
            this.ceilingTexture = TextureLoader.LoadTexture("textures/ceiling.png");

            foreach (var wall in mapReader.GetWalls())
            {
                wall.Shader = shader;
                wall.TextureID = wallTexture;
            }

            Quad floor = new Quad(128, 128, 0f, true)
            {
                Shader = shader,
                TextureID = floorTexture
            };

            Quad ceiling = new Quad(128, 128, 3f, false)
            {
                Shader = shader,
                TextureID = ceilingTexture
            };

            mapReader.GetRenderables().Add(floor);
            mapReader.GetRenderables().Add(ceiling);

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
            shader.SetUniform("cutOff", MathF.Cos(MathHelper.DegreesToRadians(20f)));
            shader.SetUniform("outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(35f)));
            shader.SetUniform("viewPos", player.Camera.Position);
        }

        /// <summary>Změna směru pohledu hráče podle pohybu myši.</summary>
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_mouseGrabbed) return;

            player.Camera.RotateX(-e.DeltaY / 250f);
            player.Camera.RotateY(-e.DeltaX / 250f);
        }

        /// <summary>Změna FOV hráče pomocí kolečka myši.</summary>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            player.Camera.ChangeFOV(e.OffsetY < 0 ? +1.0f : -1.0f);
        }

        /// <summary>
        /// Hlavní vykreslovací smyčka – aplikuje světlo, vykreslí všechny modely a počítá FPS.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            viewport.Set();
            viewport.Clear();

            var shader = mapReader.GetWalls().First().Shader;

            // Reflektor hráče (svítilna)
            player.Flashlight.Apply(shader);

            // Vykresli stěny
            mapReader.GetRenderables().ForEach(model => model.Draw(player.Camera));

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

        /// <summary>
        /// Aktualizace logiky hry – pohyb hráče, zpracování vstupů, skákání.
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float dt = (float)args.Time;

            // === Movement Input ===
            float playerSpeed = 1.4f;
            if (KeyboardState.IsKeyDown(Keys.LeftShift))
                playerSpeed *= 1.8f;

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

            // === Jump input ===
            if (isJumping && player.IsOnGround)
            {
                player.Jump(5f); // you can tune the jump force
            }

            // === Physics + collision update ===
            player.Controller.CurrentWalls = mapReader.GetWalls();
            player.Update(dt);
        }


        /// <summary>Aktualizace velikosti okna (viewport, poměr stran).</summary>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Width = e.Width;
            Height = e.Height;
        }

        /// <summary>Uvolnění prostředků při ukončení aplikace.</summary>
        protected override void OnUnload()
        {
            base.OnUnload();
        }

        /// <summary>Nastaví okno do režimu fullscreen a upraví viewport pro macOS.</summary>
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

        /// <summary>Nastaví okno zpět do okna (windowed mode).</summary>
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
