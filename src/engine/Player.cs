using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ZPG
{
    public class Player
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 Acceleration { get; set; } = Vector3.Zero;

        public float Mass { get; set; } = 80.0f;
        public bool IsOnGround { get; set; } = true;

        public Camera Camera { get; private set; }
        public Flashlight Flashlight { get; private set; }

        public float CameraHeight = 1.7f;
        public float CollisionRadius = 1.0f;

        private readonly PlayerController controller;

        public List<Model> TriggerModels { get; set; } = new();

        private Vector2i lastTilePosition = new(int.MinValue, int.MinValue);

        public Player(Vector3 startPosition, Window window)
        {
            Position = startPosition;

            // ✅ přiřazení kamery s odkazem na okno kvůli fade efektu
            Camera = new Camera(Position + new Vector3(0, CameraHeight, 0))
            {
                Window = window
            };

            Flashlight = new Flashlight(this);
            controller = new PlayerController(this);
        }

        public PlayerController Controller => controller;

        public void AddForce(Vector3 force) => controller.AddForce(force);

        public void MoveToward(Vector3 targetVelocity) => controller.MoveToward(targetVelocity);

        public void Jump(float velocityY)
        {
            if (IsOnGround)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                Velocity += new Vector3(0, velocityY, 0);
                IsOnGround = false;
            }
        }

        public void UpdateCamera()
        {
            Camera.Position = Position + new Vector3(0, CameraHeight, 0);
        }

        // === fyzika + kolize ===
        public void MoveAndSlideMeshBased(Vector3 desiredVelocity, IEnumerable<Wall> walls, float deltaTime)
        {
            float radius = 0.3f;
            Vector3 move = desiredVelocity * deltaTime;
            Vector3 newPos = Position + move;
            Vector3 totalCorrection = Vector3.Zero;

            foreach (var wall in walls)
            {
                foreach (var tri in wall.Triangles)
                {
                    Vector3 a = wall.Vertices[tri.I1].Position + wall.Position;
                    Vector3 b = wall.Vertices[tri.I2].Position + wall.Position;
                    Vector3 c = wall.Vertices[tri.I3].Position + wall.Position;

                    if (MeshCollisionHelper.SphereIntersectsTriangle(newPos, radius, a, b, c, out Vector3 pushOut))
                        totalCorrection += pushOut;
                }
            }

            newPos += totalCorrection;
            Position = newPos;

            if (totalCorrection != Vector3.Zero)
            {
                Vector3 normal = totalCorrection.Normalized();
                Velocity -= Vector3.Dot(Velocity, normal) * normal;
            }
        }
    }
}
