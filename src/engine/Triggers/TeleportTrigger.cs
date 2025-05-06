using OpenTK.Mathematics;
using System;

namespace ZPG
{
    public class TeleportTrigger : Model, ITriggerZone
    {
        public Vector3 TargetPosition { get; set; }
        public char Id { get; set; }
        private float animationTime = 0f;

        public TeleportTrigger(float width = 1.5f, float height = 0.1f, float depth = 1.5f)
        {
            CreateTeleportPortal(width, height, depth);
            ComputeNormals(Vertices, Triangles);
            Construct();

            // Načti texturu teleportu (pouze pokud je ještě nenastaveno externě)
            TextureID = TextureLoader.LoadTexture("textures/teleport.jpg");
            Console.WriteLine($"[TeleportTrigger] Loaded teleport texture for ID {Id}, TextureID: {TextureID}");
        }

        private void CreateTeleportPortal(float width, float height, float depth)
        {
            float w = width / 2f;
            float h = height;
            float d = depth / 2f;

            int segments = 24;
            float segmentAngle = 2 * MathF.PI / segments;

            Vertices.Add(new Vertex(new Vector3(0, h, 0), new Vector2(0.5f, 0.5f)));

            for (int i = 0; i < segments; i++)
            {
                float angle = i * segmentAngle;
                float x = w * MathF.Cos(angle);
                float z = d * MathF.Sin(angle);
                Vertices.Add(new Vertex(new Vector3(x, h, z), new Vector2((MathF.Cos(angle) + 1) / 2, (MathF.Sin(angle) + 1) / 2)));

                if (i < segments - 1)
                    Triangles.Add(new Triangle(0, i + 1, i + 2));
                else
                    Triangles.Add(new Triangle(0, i + 1, 1));
            }
        }

        public void OnPlayerEnter(Player player)
        {
            Console.WriteLine($"[TeleportTrigger] Player entered teleport zone '{Id}'");
            player.Position = TargetPosition + new Vector3(0, 0.01f, 0);
            player.Velocity = Vector3.Zero;
        }

        public void Update(float deltaTime)
        {
            animationTime += deltaTime;

            // Jednoduchá vertikální oscilace
            float yOffset = 0.1f * MathF.Sin(animationTime * 2.0f);
            Position = new Vector3(Position.X, yOffset, Position.Z);
        }
    }
}
