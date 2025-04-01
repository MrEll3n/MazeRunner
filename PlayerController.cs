using OpenTK.Mathematics;

namespace ZPG
{
    public class PlayerController
    {
        private readonly Player player;
        private readonly List<Vector3> forces = new();

        public Vector3 Gravity { get; set; } = new Vector3(0, -30.0f, 0);
        public float GroundFriction { get; set; } = 8.0f;

        public PlayerController(Player player)
        {
            this.player = player;
        }

        public void AddForce(Vector3 force)
        {
            forces.Add(force);
        }

        public void Update(float dt)
        {
            // gravitace
            AddForce(Gravity * player.Mass);

            // suma všech sil
            Vector3 totalForce = Vector3.Zero;
            foreach (var f in forces)
                totalForce += f;
            forces.Clear();

            // akcelerace = F / m
            player.Acceleration = totalForce / player.Mass;

            // integrace pohybu
            player.Velocity += player.Acceleration * dt;
            player.Position += player.Velocity * dt;

            // jednoduchá podlaha
            if (player.Position.Y < 0)
            {
                player.Position = new Vector3(player.Position.X, 0, player.Position.Z);
                player.Velocity = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                player.IsOnGround = true;
            }

            // tření na zemi
            if (player.IsOnGround)
            {
                Vector3 horizontal = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                horizontal -= horizontal * GroundFriction * dt;
                player.Velocity = new Vector3(horizontal.X, player.Velocity.Y, horizontal.Z);
            }
        }
    }
}
