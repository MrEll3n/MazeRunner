using OpenTK.Mathematics;

namespace ZPG
{
    public class PlayerController
    {
        private readonly Player player;
        private readonly List<Vector3> forces = new();
        private Vector3? desiredVelocity = null;

        public Vector3 Gravity { get; set; } = new Vector3(0, -15f, 0);
        public float GroundFriction { get; set; } = 8.0f;
        public float ControlFactor { get; set; } = 20f;

        public PlayerController(Player player)
        {
            this.player = player;
        }

        public void AddForce(Vector3 force)
        {
            //Console.WriteLine("Force applied: " + force);
            forces.Add(force);
        }

        public void MoveToward(Vector3 targetVelocity)
        {
            desiredVelocity = targetVelocity;
        }

        public void Update(float dt)
        {
            // gravitace
            AddForce(Gravity * player.Mass);

            // suma všech sil
            Vector3 totalForce = Vector3.Zero;
            foreach (var f in forces)
            {
                totalForce += f;
            }
            forces.Clear();

            // akcelerace = F / m
            player.Acceleration = totalForce / player.Mass;

            // integrace pohybu
            player.Velocity += player.Acceleration * dt;
            player.Position += player.Velocity * dt;

            // jednoduchá podlaha
            if (player.Position.Y <= 0.01f)
            {
                player.Position = new Vector3(player.Position.X, 0.01f, player.Position.Z);
                player.Velocity = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                if (!player.IsOnGround)
                {
                    Console.WriteLine("Landed");
                }
                player.IsOnGround = true;
            }

            // tření na zemi
            if (player.IsOnGround)
            {
                Vector3 horizontal = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                horizontal -= horizontal * GroundFriction * dt;
                player.Velocity = new Vector3(horizontal.X, player.Velocity.Y, horizontal.Z);
            }

            // Air drag — zpomaluje horizontální pohyb ve vzduchu
            if (!player.IsOnGround)
            {
                Vector3 horizontal = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 drag = horizontal * 2.0f; // síla odporu – můžeš ladit
                player.Velocity -= new Vector3(drag.X, 0, drag.Z) * dt;
            }

            // směrový pohyb podle cílové rychlosti
            if (desiredVelocity.HasValue)
            {
                Vector3 current = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 desired = desiredVelocity.Value;
                Vector3 diff = desired - current;

                Vector3 force = diff * ControlFactor * player.Mass;

                AddForce(new Vector3(force.X, 0, force.Z));
                desiredVelocity = null;
            }
        }
    }
}
