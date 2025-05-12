using OpenTK.Mathematics;
using System;

namespace ZPG
{
    public class TeleportTrigger : Model, ITriggerZone
    {
        public Vector3 TargetPosition { get; set; }
        public char Id { get; set; }
        private float animationTime = 0f;

        // Collision parameters
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Depth { get; private set; }

        public TeleportTrigger(float width = 1.5f, float height = 2.0f, float depth = 1.5f)
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
            Console.WriteLine($"[TeleportTrigger] Player entered teleport zone '{Id}'");
            Console.WriteLine($"[TeleportTrigger] Current Position: {player.Position}");
            Console.WriteLine($"[TeleportTrigger] Target Position: {TargetPosition}");

            player.Position = TargetPosition + new Vector3(0, 0.1f, 0);
            player.Velocity = Vector3.Zero;

            Console.WriteLine($"[TeleportTrigger] Player teleported to: {player.Position}");
        }

        public void Update(float deltaTime)
        {
            animationTime += deltaTime;
            float yOffset = 0.1f * MathF.Sin(animationTime * 2.0f);
            Position = new Vector3(Position.X, yOffset, Position.Z);
        }

        public bool IsCollidingWithPlayer(Player player)
        {
            Vector3 playerPos = player.Position;
            float playerRadius = player.CollisionRadius;

            Vector3 teleporterPos = Position;

            bool xCollision = Math.Abs(playerPos.X - teleporterPos.X) < (Width / 2 + playerRadius);
            bool yCollision = Math.Abs(playerPos.Y - teleporterPos.Y) < (Height / 2 + playerRadius);
            bool zCollision = Math.Abs(playerPos.Z - teleporterPos.Z) < (Depth / 2 + playerRadius);

            bool isColliding = xCollision && yCollision && zCollision;

            if (isColliding)
            {
                Console.WriteLine($"[TeleportTrigger] Collision detected with teleport '{Id}'");
                Console.WriteLine($"[TeleportTrigger] Player Position: {playerPos}");
                Console.WriteLine($"[TeleportTrigger] Teleporter Position: {teleporterPos}");
            }

            return isColliding;
        }
    }
}