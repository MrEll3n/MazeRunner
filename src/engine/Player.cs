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

        private float bobTime = 0f;
        private float bobSpeed = 10f;
        private float bobAmount = 0.035f;
        private float previousBobX = 0f;
        private float previousBobY = 0f;

        private readonly PlayerController controller;

        public List<Model> TriggerModels { get; set; } = new();

        private Vector2i lastTilePosition = new(int.MinValue, int.MinValue);

        public Player(Vector3 startPosition, Window window)
        {
            Position = startPosition;

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

        public bool IsMoving => new Vector2(Velocity.X, Velocity.Z).LengthSquared > 0.05f;

        public void Jump(float velocityY)
        {
            if (IsOnGround)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                Velocity += new Vector3(0, velocityY, 0);
                IsOnGround = false;
            }
        }

        public void UpdateCamera(float deltaTime)
        {
            Vector3 baseCamPos = Position + new Vector3(0, CameraHeight, 0);
            Vector2 horizVel = new(Velocity.X, Velocity.Z);
            bool isWalking = IsOnGround && horizVel.Length > 0.05f;

            // Čas bobbingu roste jen při chůzi
            if (isWalking)
                bobTime += deltaTime;

            float frequency = 2f;
            float targetVertical = isWalking ? MathF.Sin(bobTime * frequency * MathF.Tau) * bobAmount : 0f;
            float targetHorizontal = isWalking ? MathF.Sin(bobTime * frequency * MathF.Tau * 0.5f) * bobAmount * 0.5f : 0f;

            // Utlumení směrem k 0 pokud stojí
            float smoothing = 10f; // vyšší = rychlejší návrat
            if (!isWalking)
            {
                targetVertical = previousBobY + (0f - previousBobY) * deltaTime * smoothing;
                targetHorizontal = previousBobX + (0f - previousBobX) * deltaTime * smoothing;
            }

            previousBobX = targetHorizontal;
            previousBobY = targetVertical;

            Camera.Position = baseCamPos + new Vector3(targetHorizontal, targetVertical, 0);
        }

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