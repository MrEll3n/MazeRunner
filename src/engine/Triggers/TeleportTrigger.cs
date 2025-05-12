using OpenTK.Mathematics;
using System;

namespace ZPG
{
    public class TeleportTrigger : Model, ITriggerZone
    {
        public Vector3 TargetPosition { get; set; }
        public char Id { get; set; }
        private float animationTime = 0f;
        private float basePositionY = 0f;

        // Collision parameters
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Depth { get; private set; }

        // Teleport timing parameters
        public float DelayBeforeTeleport { get; set; } = 2.0f;
        public float CooldownAfterTeleport { get; set; } = 5.0f;
        private float currentDelayTimer = 0f;
        private float currentCooldownTimer = 0f;
        private Player currentPlayer = null;
        private bool isTeleporting = false;
        
        // Globální cooldown pro všechny teleporty
        private static float globalPlayerCooldown = 0f;
        private static readonly object cooldownLock = new object();

        public TeleportTrigger(float width = 1.5f, float height = 1.0f, float depth = 1.5f)
        {
            Width = width;
            Height = height;
            Depth = depth;

            CreateTeleportPortal(width, height, depth);
            ComputeNormals(Vertices, Triangles);
            Construct();

            try
            {
                TextureID = TextureLoader.LoadTexture("textures/teleport.jpg");
                Console.WriteLine($"[TeleportTrigger] Loaded teleport texture. TextureID: {TextureID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TeleportTrigger] Failed to load teleport texture: {ex.Message}");
                TextureID = 0;
            }
        }

        private void CreateTeleportPortal(float width, float height, float depth)
        {
            float w = width / 2f;
            float h = height / 2f;
            float d = depth / 2f;

            int segments = 24;
            float segmentAngle = 2 * MathF.PI / segments;

            // Spodní střed
            Vertices.Add(new Vertex(new Vector3(0, -h, 0), new Vector2(0.5f, 0.5f)));

            for (int i = 0; i < segments; i++)
            {
                float angle = i * segmentAngle;
                float x = w * MathF.Cos(angle);
                float z = d * MathF.Sin(angle);
                Vertices.Add(new Vertex(new Vector3(x, -h, z), new Vector2((MathF.Cos(angle) + 1) / 2, (MathF.Sin(angle) + 1) / 2)));

                // CW pořadí → otočíme trojúhelník
                if (i < segments - 1)
                    Triangles.Add(new Triangle(0, i + 2, i + 1));
                else
                    Triangles.Add(new Triangle(0, 1, i + 1));
            }

            // Horní střed
            Vertices.Add(new Vertex(new Vector3(0, h, 0), new Vector2(0.5f, 0.5f)));

            for (int i = 0; i < segments; i++)
            {
                float angle = i * segmentAngle;
                float x = w * MathF.Cos(angle);
                float z = d * MathF.Sin(angle);
                Vertices.Add(new Vertex(new Vector3(x, h, z), new Vector2((MathF.Cos(angle) + 1) / 2, (MathF.Sin(angle) + 1) / 2)));

                // CW pořadí horních trojúhelníků
                if (i < segments - 1)
                    Triangles.Add(new Triangle(segments + 1, segments + i + 3, segments + i + 2));
                else
                    Triangles.Add(new Triangle(segments + 1, segments + 2, segments + i + 2));
            }
        }

        public void OnPlayerEnter(Player player)
        {
            // Kontrola globálního cooldownu
            if (IsGlobalCooldownActive())
            {
                // Pokud je hráč v globálním cooldownu, ignorujeme jeho vstup do teleportu
                return;
            }
            
            // Kontrola lokálního cooldownu tohoto teleportu
            if (IsOnCooldown)
            {
                return;
            }
            
            // Nejprve nastavíme aktuálního hráče
            if (currentPlayer == null)
            {
                currentPlayer = player;
                currentDelayTimer = 0f;
                Console.WriteLine($"[TeleportTrigger] Player entered teleport zone '{Id}', starting delay timer");
            }
        }

        private void ExecuteTeleport(Player player)
        {
            Console.WriteLine($"[TeleportTrigger] Executing teleport '{Id}'");
            Console.WriteLine($"[TeleportTrigger] Current Position: {player.Position}");
            Console.WriteLine($"[TeleportTrigger] Target Position: {TargetPosition}");

            player.Position = TargetPosition + new Vector3(0, 0.1f, 0);
            player.Velocity = Vector3.Zero;
            
            Console.WriteLine($"[TeleportTrigger] Player teleported to: {player.Position}");
            
            // Nastavíme lokální cooldown po teleportaci
            currentCooldownTimer = CooldownAfterTeleport;
            
            // Nastavíme globální cooldown
            SetGlobalCooldown(CooldownAfterTeleport);
            
            isTeleporting = false;
        }

        public void OnPlayerExit(Player player)
        {
            if (currentPlayer == player && !isTeleporting)
            {
                Console.WriteLine($"[TeleportTrigger] Player left teleport zone '{Id}', resetting delay timer");
                currentPlayer = null;
                currentDelayTimer = 0f;
            }
        }

        public void Update(float deltaTime)
        {
            // Aktualizace globálního cooldownu
            UpdateGlobalCooldown(deltaTime);
            
            // Animace teleportu - mírné pohupování nahoru a dolů
            animationTime += deltaTime;
            float yOffset = 0.1f * MathF.Sin(animationTime * 2.0f);
            Position = new Vector3(Position.X, basePositionY + yOffset, Position.Z);

            // Zpracování časovače pro delay teleportace
            if (currentPlayer != null && !isTeleporting && currentCooldownTimer <= 0f && !IsGlobalCooldownActive())
            {
                currentDelayTimer += deltaTime;
                
                if (currentDelayTimer >= DelayBeforeTeleport)
                {
                    Console.WriteLine($"[TeleportTrigger] Delay timer completed, initiating teleport");
                    isTeleporting = true;
                    ExecuteTeleport(currentPlayer);
                    currentDelayTimer = 0f;
                }
                else if (currentDelayTimer >= 1.0f && currentDelayTimer % 1.0f < deltaTime)
                {
                    // Logovací zpráva každou sekundu
                    Console.WriteLine($"[TeleportTrigger] Teleport '{Id}' activating in {DelayBeforeTeleport - currentDelayTimer:F1} seconds");
                }
            }

            // Zpracování cooldownu po teleportaci
            if (currentCooldownTimer > 0f)
            {
                currentCooldownTimer -= deltaTime;
                
                if (currentCooldownTimer <= 0f)
                {
                    Console.WriteLine($"[TeleportTrigger] Teleport '{Id}' cooldown ended");
                }
            }
        }

        public bool IsCollidingWithPlayer(Player player)
        {
            // Použijeme skutečné hodnoty Width, Depth místo konstantních hodnot
            float horizontalRadius = Math.Max(Width, Depth) / 2f;
            float verticalTolerance = Height / 2f;
            
            // Přidáme malou toleranci pro lepší detekci
            horizontalRadius += 0.1f;
            verticalTolerance += 0.1f;
            
            Vector2 triggerXZ = new Vector2(Position.X, Position.Z);
            Vector2 playerXZ = new Vector2(player.Position.X, player.Position.Z);
            float distXZ = (playerXZ - triggerXZ).Length;
            float yDiff = Math.Abs(player.Position.Y - Position.Y);

            bool colliding = distXZ <= horizontalRadius && yDiff <= verticalTolerance;

            // Pouze pro debug - omezíme množství zpráv
            if (colliding && Math.Floor(animationTime) % 3 == 0)
            {
                //Console.WriteLine($"[TeleportTrigger] COLLISION with '{Id}': DistXZ={distXZ:F3}, YDiff={yDiff:F3}");
            }

            return colliding;
        }

        public void SetBasePosition(Vector3 position)
        {
            basePositionY = position.Y;
            Position = position;
        }
        
        public bool IsOnCooldown => currentCooldownTimer > 0f;
        
        public float GetDelayProgress()
        {
            if (currentDelayTimer <= 0f || DelayBeforeTeleport <= 0f)
                return 0f;
                
            return Math.Min(currentDelayTimer / DelayBeforeTeleport, 1.0f);
        }
        
        // Metody pro práci s globálním cooldownem
        private static void SetGlobalCooldown(float cooldownTime)
        {
            lock (cooldownLock)
            {
                globalPlayerCooldown = cooldownTime;
                Console.WriteLine($"[TeleportTrigger] Set global cooldown: {cooldownTime}s");
            }
        }
        
        private static void UpdateGlobalCooldown(float deltaTime)
        {
            lock (cooldownLock)
            {
                if (globalPlayerCooldown > 0f)
                {
                    globalPlayerCooldown -= deltaTime;
                    if (globalPlayerCooldown <= 0f)
                    {
                        globalPlayerCooldown = 0f;
                        Console.WriteLine("[TeleportTrigger] Global cooldown ended");
                    }
                }
            }
        }
        
        private static bool IsGlobalCooldownActive()
        {
            lock (cooldownLock)
            {
                return globalPlayerCooldown > 0f;
            }
        }
    }
}