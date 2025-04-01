using OpenTK.Mathematics;

namespace ZPG
{
    public class Player
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 Acceleration { get; set; } = Vector3.Zero;

        public float Mass { get; set; } = 1.0f;

        public bool IsOnGround { get; set; } = true;

        public Camera Camera { get; private set; }

        private readonly PlayerController controller;

        public Player(Vector3 startPosition, float aspectRatio)
        {
            Position = startPosition;
            Camera = new Camera(Position + new Vector3(0, 1.7f, 0), aspectRatio);
            controller = new PlayerController(this);
        }

        public void AddForce(Vector3 force)
        {
            controller.AddForce(force);
        }

        public void Jump(float force)
        {
            if (IsOnGround)
            {
                AddForce(new Vector3(0, force, 0));
                IsOnGround = false;
            }
        }

        public void Update(float deltaTime)
        {
            controller.Update(deltaTime);
            Camera.Position = Position + new Vector3(0, 1.7f, 0);
        }
    }
}