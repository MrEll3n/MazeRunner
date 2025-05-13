using OpenTK.Mathematics;
using System;

namespace ZPG
{
    /// <summary>
    /// Osmistěnný (diamantový) teleport s animací pohupování, zpožděním a cooldownem.
    /// </summary>
    public class TeleportTrigger : Model, ITriggerZone
    {
        /* ---------- veřejné vlastnosti ---------- */

        /// <summary>ID teleportu, tj. číslice z mapy.</summary>
        public char Id { get; set; }

        /// <summary>Místo, kam se hráč po uplynutí delaye přenese.</summary>
        public Vector3 TargetPosition { get; set; }

        /// <summary>Čas, po jak dlouhé době přenos nastane (s).</summary>
        public float DelayBeforeTeleport { get; set; }   = 1.0f;

        /// <summary>Doba, po kterou se po teleportu nedá portál znovu spustit (s).</summary>
        public float CooldownAfterTeleport { get; set; } = 6.0f;

        /* ---------- interní stav ---------- */

        private float animationTime  = 0f;   // pro sinusové pohupování
        private float basePosY       = 0f;   // „nulová“ výška portálu
        private float rotationAngle  = 0f;   // úhel rotace portálu (pro animaci)

        private Player currentPlayer = null; // hráč, který právě stojí v portálu
        private float  delayTimer    = 0f;   // odpočítávání před teleportem
        private float  cooldownTimer = 0f;   // lokální cooldown
        private bool   isTeleporting = false;

        // --- sdílený globální cooldown (aby se neřetězily portály) ---
        private static float globalCooldown = 0f;
        private static readonly object cooldownLock = new();

        /* ---------- rozměry + kolizní tolerance ---------- */

        public float Width  { get; private set; }
        public float Height { get; private set; }
        public float Depth  { get; private set; }

        private float horizRadius;   // poloměr pro X-Z kolizi
        private float vertTolerance; // povolená odchylka ve výšce
        
        public FadeOverlay FadeOverlay { get; set; }

        private void RecomputeCollisionBounds()
        {
            horizRadius   = MathF.Max(Width, Depth) * 0.5f + 0.1f; // malá rezerva
            vertTolerance = Height * 0.5f               + 0.1f;
        }

        /* ---------- konstrukce ---------- */

        /// <param name="size">„Hrana“ diamantu (prakticky průměr portálu).</param>
        public TeleportTrigger(float size = 1.6f)
        {
            Width = Height = Depth = size;
            BuildDiamondMesh(size);
            ComputeNormals(Vertices, Triangles);
            Construct();

            // textura portálu
            try
            {
                TextureID = TextureLoader.LoadTexture("textures/teleport.jpg");
            }
            catch
            {
                TextureID = 0;
            }
        }
        
        public override Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateRotationY(rotationAngle)
                   * Matrix4.CreateTranslation(Position);
        }

        /* ----------------- MESH generátor (osmistěn) ----------------- */

        private void BuildDiamondMesh(float size)
        {
            Vertices.Clear();
            Triangles.Clear();

            // indexy: 0 = horní špička, 1 = dolní špička,
            // 2-5 = střed (N, E, S, W)
            float h = 1.0f;
            float stretch = 1.3f; // faktor protažení ve vertikálním směru

            Vertices.Add(new Vertex(new Vector3( 0,  h * stretch, 0), new Vector2(0.5f, 1f))); // 0 – horní špička
            Vertices.Add(new Vertex(new Vector3( 0, -h * stretch, 0), new Vector2(0.5f, 0f))); // 1 – spodní špička

            Vertices.Add(new Vertex(new Vector3( 0, 0, -h), new Vector2(0.5f, .5f))); // 2 N
            Vertices.Add(new Vertex(new Vector3( h, 0,  0), new Vector2(1f,   .5f))); // 3 E
            Vertices.Add(new Vertex(new Vector3( 0, 0,  h), new Vector2(0.5f, .5f))); // 4 S
            Vertices.Add(new Vertex(new Vector3(-h, 0,  0), new Vector2(0f,   .5f))); // 5 W

            // horní poloviny – CCW při pohledu zvenčí ⇒ normály ↑
            AddFace(0, 3, 2);
            AddFace(0, 4, 3);
            AddFace(0, 5, 4);
            AddFace(0, 2, 5);

            // spodní poloviny – CCW při pohledu zvenčí ⇒ normály ↓
            AddFace(2, 3, 1);
            AddFace(3, 4, 1);
            AddFace(4, 5, 1);
            AddFace(5, 2, 1);

            void AddFace(int a, int b, int c) => Triangles.Add(new Triangle(a, b, c));

            RecomputeCollisionBounds();
        }

        /* ----------------- veřejné utilitky ----------------- */

        /// <summary>Uloží počáteční Y a nastaví aktuální pozici modelu.</summary>
        public void SetBasePosition(Vector3 pos)
        {
            Position = pos;
            basePosY = pos.Y;
        }

        /* ----------------- detekce kolize hráče ----------------- */

        public bool IsColliding(Player plr)
        {
            //Console.WriteLine($"[DEBUG] playerY: {plr.Position.Y:F2} .. {plr.Position.Y + plr.CameraHeight:F2}, basePosY: {basePosY:F2}, posY: {Position.Y:F2}");
            float teleportRadius = 1.2f;
            float teleportHeight = 2.5f;

            // Hráčovy hranice (nohy až hlava)
            float playerYMin = plr.Position.Y;
            float playerYMax = plr.Position.Y + plr.CameraHeight;

            // Válcový trigger má střed u basePosY
            float yBottom = basePosY - teleportHeight / 2f;
            float yTop = basePosY + teleportHeight / 2f;

            // AABB-Y překrytí?
            bool yOverlap = playerYMax >= yBottom && playerYMin <= yTop;

            // Vzdálenost v XZ rovině
            Vector2 playerXZ = new(plr.Position.X, plr.Position.Z);
            Vector2 triggerXZ = new(Position.X, Position.Z);
            float distXZ = (playerXZ - triggerXZ).Length;

            return yOverlap && distXZ <= teleportRadius;
        }

        /* ----------------- reakce na vstup/odchod ----------------- */

        public void OnPlayerEnter(Player plr)
        {
            if (IsGlobalCooldown() || cooldownTimer > 0f) return;

            if (currentPlayer == null)
            {
                Console.WriteLine($"[DEBUG] OnPlayerEnter: hráč vstoupil do teleportu {Id}.");
                currentPlayer = plr;
                delayTimer = 0f;
            }
        }

        public void OnPlayerExit(Player plr)
        {
            // reset jen pokud nic neběží
            if (plr == currentPlayer && !isTeleporting && cooldownTimer <= 0f)
            {
                currentPlayer = null;
                delayTimer    = 0f;
            }
        }

        /* ----------------- hlavní Update ----------------- */

        public void Update(float dt)
        {
            // animace (lehké pohupování)
            animationTime += dt;
            Position = new Vector3(Position.X,
                                   basePosY + 0.1f * MathF.Sin(animationTime * 2f),
                                   Position.Z);
            
            rotationAngle += dt * .5f; // 1.0f = rychlost otáčení (radiány za sekundu)
            if (rotationAngle > MathF.Tau) rotationAngle -= MathF.Tau;

            TickGlobalCooldown(dt);

            // lokální cooldown
            if (cooldownTimer > 0f)
            {
                cooldownTimer = MathF.Max(0f, cooldownTimer - dt);
                return;
            }

            // čekání na uplynutí delaye
            if (currentPlayer != null)
            {
                delayTimer += dt;
                if (delayTimer >= DelayBeforeTeleport)
                    ExecuteTeleport();
            }
        }

        /* ----------------- TELEPORTACE ----------------- */

        private void ExecuteTeleport()
        {
            if (currentPlayer == null || FadeOverlay == null || FadeOverlay.IsActive)
                return;
            
            if (currentPlayer == null || FadeOverlay == null)
            {
                Console.WriteLine("[TeleportTrigger] Teleport aborted – no player or overlay.");
                return;
            }

            Console.WriteLine("[TeleportTrigger] Starting teleport fade...");
            isTeleporting = true;

            FadeOverlay.StartFade(() =>
            {
                Console.WriteLine("[TeleportTrigger] Teleporting player.");
                currentPlayer.Position = TargetPosition + new Vector3(0, -1f, 0);
                currentPlayer.Velocity = Vector3.Zero;

                cooldownTimer = CooldownAfterTeleport;
                SetGlobalCooldown(CooldownAfterTeleport);

                currentPlayer = null;
                delayTimer = 0f;
                isTeleporting = false;
            });
        }


        /* ----------------- globální cooldown ----------------- */

        private static void SetGlobalCooldown(float t)
        {
            lock (cooldownLock)
            {
                globalCooldown = MathF.Max(globalCooldown, t);
            }
        }

        private static void TickGlobalCooldown(float dt)
        {
            lock (cooldownLock)
            {
                if (globalCooldown > 0f)
                    globalCooldown = MathF.Max(0f, globalCooldown - dt);
            }
        }

        private static bool IsGlobalCooldown()
        {
            lock (cooldownLock) { return globalCooldown > 0f; }
        }
    }
}
