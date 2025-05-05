using OpenTK.Mathematics;
using System.Collections.Generic;

namespace ZPG
{
    public class PlayerController
    {
        private readonly Player player;

        private readonly List<Vector3> forces = new();
        private Vector3? desiredVelocity = null;

        public Vector3 Gravity { get; set; } = new Vector3(0, -15f, 0);
        public float GroundFriction { get; set; } = 8.0f;
        public float ControlFactor { get; set; } = 40f;

        public List<Wall> CurrentWalls { get; set; } = new();

        public PlayerController(Player player)
        {
            this.player = player;
        }

        public void AddForce(Vector3 force)
        {
            forces.Add(force);
        }

        public void MoveToward(Vector3 targetVelocity)
        {
            desiredVelocity = targetVelocity;
        }

        public void ApplyInputControl()
        {
            if (desiredVelocity.HasValue)
            {
                Vector3 current = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 desired = desiredVelocity.Value;
                Vector3 diff = desired - current;

                Vector3 force = diff * ControlFactor * player.Mass;
                AddForce(new Vector3(force.X, 0, force.Z));
            }
        }

        public Vector3 ConsumeForces()
        {
            Vector3 total = Gravity * player.Mass;
            foreach (var f in forces)
                total += f;
            forces.Clear();
            return total;
        }

        public void ClearInput()
        {
            desiredVelocity = null;
        }

        public bool HasInput => desiredVelocity.HasValue;
    }
}