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
        public float CooldownAfterTeleport { get; set; } = 2.0f;

        /* ---------- interní stav ---------- */

        private float animationTime  = 0f;   // pro sinusové pohupování
        private float basePosY       = 0f;   // „nulová“ výška portálu

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

        /* ----------------- MESH generátor (osmistěn) ----------------- */

        private void BuildDiamondMesh(float size)
        {
            Vertices.Clear();
            Triangles.Clear();

            float h = size * 0.5f;

            // indexy: 0 = horní špička, 1 = dolní špička,
            // 2-5 = střed (N, E, S, W)
            Vertices.Add(new Vertex(new Vector3( 0,  h, 0), new Vector2(0.5f, 1f))); // 0
            Vertices.Add(new Vertex(new Vector3( 0, -h, 0), new Vector2(0.5f, 0f))); // 1

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
            Vector2 trgXZ = new(Position.X, Position.Z);
            Vector2 plyXZ = new(plr.Position.X, plr.Position.Z);

            float distXZ = (plyXZ - trgXZ).Length;
            float yDiff  = MathF.Abs(plr.Position.Y - Position.Y);

            return distXZ <= horizRadius && yDiff <= vertTolerance;
        }

        /* ----------------- reakce na vstup/odchod ----------------- */

        public void OnPlayerEnter(Player plr)
        {
            if (IsGlobalCooldown() || cooldownTimer > 0f) return;

            // nechceme resetovat delay, pokud už hráč stojí uvnitř
            if (currentPlayer == null)
            {
                currentPlayer = plr;
                delayTimer    = 0f;
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
            if (currentPlayer == null) return;

            isTeleporting           = true;
            currentPlayer.Position  = TargetPosition + new Vector3(0, 0.1f, 0);
            currentPlayer.Velocity  = Vector3.Zero;

            cooldownTimer = CooldownAfterTeleport;
            SetGlobalCooldown(CooldownAfterTeleport);

            // reset lokálních stavů
            currentPlayer  = null;
            delayTimer     = 0f;
            isTeleporting  = false;
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
