using OpenTK.Mathematics;

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

        public Player(Vector3 startPosition, Window window)
        {
            Position = startPosition;
            Camera = new Camera(Position + new Vector3(0, CameraHeight, 0)) { Window = window };
            Flashlight = new Flashlight(this);
            controller = new PlayerController(this);
        }

        public void AddForce(Vector3 force)
        {
            controller.AddForce(force);
        }

        public void MoveToward(Vector3 targetVelocity)
        {
            controller.MoveToward(targetVelocity);
        }

        public void Jump(float velocityY)
        {
            if (IsOnGround)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                Velocity += new Vector3(0, velocityY, 0); // přímo nastavíme výchozí rychlost vzhůru
                IsOnGround = false;
            }
        }

        public void Update(float deltaTime)
        {
            controller.Update(deltaTime);
            Camera.Position = Position + new Vector3(0, 1.7f, 0);
        }

        public void MoveAndSlideMeshBased(Vector3 desiredVelocity, IEnumerable<Wall> walls, float deltaTime)
        {
            float radius = 0.3f;

            // Uložení původní pozice pro výpočet skutečného posunu
            Vector3 oldPosition = Position;

            // Pohyb podle požadované rychlosti
            Vector3 move = desiredVelocity * deltaTime;
            Vector3 newPos = Position + move;

            Vector3 totalCorrection = Vector3.Zero;

            // Kolizní kontrola a korekce
            foreach (var wall in walls)
            {
                foreach (var tri in wall.Triangles)
                {
                    Vector3 a = wall.Vertices[tri.I1].Position + wall.Position;
                    Vector3 b = wall.Vertices[tri.I2].Position + wall.Position;
                    Vector3 c = wall.Vertices[tri.I3].Position + wall.Position;

                    if (MeshCollisionHelper.SphereIntersectsTriangle(newPos, radius, a, b, c, out Vector3 pushOut))
                    {
                        totalCorrection += pushOut;
                    }
                }
            }

            // Aplikace korekcí a nové pozice
            newPos += totalCorrection;
            Position = newPos;

            // Výpočet skutečného pohybu a aktualizace horizontální složky Velocity
            Vector3 realMovement = Position - oldPosition;
            Velocity = new Vector3(realMovement.X / deltaTime, Velocity.Y, realMovement.Z / deltaTime);
        }
    }
}
