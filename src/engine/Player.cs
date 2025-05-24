using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ZPG
{
    /// <summary>
    /// Represents the player entity, including movement, physics, camera, and interaction with the environment.
    /// </summary>
    public class Player
    {
        /// <summary>Current world position of the player.</summary>
        public Vector3 Position { get; set; }
        /// <summary>Current velocity of the player.</summary>
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        /// <summary>Current acceleration applied to the player.</summary>
        public Vector3 Acceleration { get; set; } = Vector3.Zero;

        /// <summary>Mass of the player used in physics calculations.</summary>
        public float Mass { get; set; } = 80.0f;
        /// <summary>Indicates whether the player is currently on the ground.</summary>
        public bool IsOnGround { get; set; } = true;

        /// <summary>The camera object attached to the player.</summary>
        public Camera Camera { get; private set; }
        /// <summary>The flashlight object held by the player.</summary>
        public Flashlight Flashlight { get; private set; }

        /// <summary>Height of the camera from the player's position.</summary>
        public float CameraHeight = 1.7f;
        /// <summary>Collision radius for interactions with the world geometry.</summary>
        public float CollisionRadius = 1.0f;

        private float bobTime = 0f;
        private float bobSpeed = 10f;
        private float bobAmount = 0.035f;
        private float previousBobX = 0f;
        private float previousBobY = 0f;

        private readonly PlayerController controller;

        /// <summary>Models currently triggering events for the player (e.g. collectibles, triggers).</summary>
        public List<Model> TriggerModels { get; set; } = new();

        private Vector2i lastTilePosition = new(int.MinValue, int.MinValue);

        /// <summary>
        /// Creates a new player instance at the given start position and assigns its camera and flashlight.
        /// </summary>
        /// <param name="startPosition">Initial world position of the player.</param>
        /// <param name="window">The game window used for camera attachment.</param>
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

        /// <summary>Provides access to the player's controller for movement and force interactions.</summary>
        public PlayerController Controller => controller;

        /// <summary>Applies an external force to the player.</summary>
        /// <param name="force">The force vector to apply.</param>
        public void AddForce(Vector3 force) => controller.AddForce(force);

        /// <summary>Applies a velocity-based movement force toward a target velocity.</summary>
        /// <param name="targetVelocity">Desired movement direction and magnitude.</param>
        public void MoveToward(Vector3 targetVelocity) => controller.MoveToward(targetVelocity);

        /// <summary>Indicates whether the player is currently moving on the horizontal plane.</summary>
        public bool IsMoving => new Vector2(Velocity.X, Velocity.Z).LengthSquared > 0.05f;

        /// <summary>Triggers a jump action by applying vertical velocity if the player is grounded.</summary>
        /// <param name="velocityY">The vertical jump velocity.</param>
        public void Jump(float velocityY)
        {
            if (IsOnGround)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                Velocity += new Vector3(0, velocityY, 0);
                IsOnGround = false;
            }
        }

        /// <summary>Updates the player’s camera with simulated head bobbing based on movement.</summary>
        /// <param name="deltaTime">Time since last frame in seconds.</param>
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

        /// <summary>
        /// Moves the player with collision response against a triangle mesh-based world.
        /// Uses sphere-triangle collision resolution.
        /// </summary>
        /// <param name="desiredVelocity">Target velocity to apply.</param>
        /// <param name="walls">Walls containing triangle geometry to collide with.</param>
        /// <param name="deltaTime">Frame time step in seconds.</param>
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