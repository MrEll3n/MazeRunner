using OpenTK.Mathematics;
using System.Collections.Generic;

namespace ZPG
{
    /// <summary>
    /// Handles player movement forces, gravity, and directional input control logic.
    /// Applies physics-based movement by accumulating and consuming forces each frame.
    /// </summary>
    public class PlayerController
    {
        private readonly Player player;

        private readonly List<Vector3> forces = new();
        private Vector3? desiredVelocity = null;

        /// <summary>
        /// Gravity force applied to the player, typically pointing downward.
        /// </summary>
        public Vector3 Gravity { get; set; } = new Vector3(0, -15f, 0);
        /// <summary>
        /// Amount of friction applied when the player is on the ground.
        /// </summary>
        public float GroundFriction { get; set; } = 8.0f;
        /// <summary>
        /// Factor used to scale player control responsiveness to desired movement direction.
        /// </summary>
        public float ControlFactor { get; set; } = 40f;

        /// <summary>
        /// List of wall objects currently affecting the player's movement (e.g. collisions).
        /// </summary>
        public List<Wall> CurrentWalls { get; set; } = new();

        /// <summary>
        /// Initializes the player controller for the given player entity.
        /// </summary>
        /// <param name="player">The player associated with this controller.</param>
        public PlayerController(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Adds a force vector to be applied to the player during the current frame.
        /// </summary>
        /// <param name="force">The force vector to apply.</param>
        public void AddForce(Vector3 force)
        {
            forces.Add(force);
        }

        /// <summary>
        /// Sets the desired velocity that the player should move toward.
        /// </summary>
        /// <param name="targetVelocity">Target horizontal movement velocity.</param>
        public void MoveToward(Vector3 targetVelocity)
        {
            desiredVelocity = targetVelocity;
        }

        /// <summary>
        /// Applies control logic to move the player in the direction of desired velocity.
        /// Computes and adds a force based on the velocity difference.
        /// </summary>
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

        /// <summary>
        /// Computes and returns the total force to apply, including gravity and user-applied forces.
        /// Clears the force list after computation.
        /// </summary>
        /// <returns>The total accumulated force vector.</returns>
        public Vector3 ConsumeForces()
        {
            Vector3 total = Gravity * player.Mass;
            foreach (var f in forces)
                total += f;
            forces.Clear();
            return total;
        }

        /// <summary>
        /// Clears the current directional input velocity.
        /// </summary>
        public void ClearInput()
        {
            desiredVelocity = null;
        }

        /// <summary>
        /// Indicates whether the player currently has directional input set.
        /// </summary>
        public bool HasInput => desiredVelocity.HasValue;
    }
}