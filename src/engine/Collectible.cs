using OpenTK.Mathematics;
using System;

namespace ZPG
{
    /// <summary>
    /// Represents a collectible item in the world that behaves like a billboard and triggers
    /// a response when the player comes into contact with it.
    /// </summary>
    public class Collectible : Billboard, ITriggerZone
    {
        private bool isPlayerInside = false;

        /// <summary>
        /// Indicates whether the collectible has already been collected.
        /// </summary>
        public bool IsCollected = false;

        /// <summary>
        /// Event triggered when the item is collected by the player.
        /// </summary>
        public Action OnCollected;

        private float triggerRadius;

        private float animationTime = 0f;
        private Vector3 basePosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Collectible"/> class.
        /// </summary>
        /// <param name="position">The position of the collectible in the world.</param>
        /// <param name="size">The size of the collectible billboard.</param>
        public Collectible(Vector3 position, Vector2 size)
            : base(position, size, alignX: false, alignY: true, alignZ: false)
        {
            triggerRadius = Math.Max(size.X, size.Y) * 0.6f;
            basePosition = position;
        }

        /// <summary>
        /// Updates the animation state of the collectible (floating effect).
        /// </summary>
        /// <param name="dt">Delta time since the last update.</param>
        public void Update(float dt)
        {
            if (IsCollected)
                return;

            animationTime += dt;

            // sinusové pohupování nahoru/dolů
            float offset = 0.1f * MathF.Sin(animationTime * 2f);
            SetPosition(basePosition + new Vector3(0, offset, 0));
        }

        /// <summary>
        /// Checks whether the player is currently inside the trigger area of the collectible.
        /// </summary>
        /// <param name="player">The player to check against.</param>
        public void CheckTrigger(Player player)
        {
            if (IsCollected)
                return;

            bool isInsideNow = IsPlayerInside(player);

            if (!isPlayerInside && isInsideNow)
                OnPlayerEnter(player);
            else if (isPlayerInside && !isInsideNow)
                OnPlayerExit(player);

            isPlayerInside = isInsideNow;
        }

        /// <summary>
        /// Determines if the player is within the collectible's collision bounds.
        /// </summary>
        /// <param name="player">The player to test against.</param>
        /// <returns>True if the player is inside the trigger area; otherwise false.</returns>
        public bool IsPlayerInside(Player player)
        {
            float collectibleRadius = 0.6f;
            float collectibleHeight = 2.5f;

            float playerYMin = player.Position.Y;
            float playerYMax = player.Position.Y + player.CameraHeight;

            float yBottom = basePosition.Y - collectibleHeight / 2f;
            float yTop = basePosition.Y + collectibleHeight / 2f;

            bool yOverlap = playerYMax >= yBottom && playerYMin <= yTop;

            Vector2 playerXZ = new(player.Position.X, player.Position.Z);
            Vector2 collectibleXZ = new(basePosition.X, basePosition.Z);
            float distXZ = (playerXZ - collectibleXZ).Length;

            return yOverlap && distXZ <= collectibleRadius;
        }

        /// <summary>
        /// Called when the player enters the trigger zone and collects the item.
        /// </summary>
        /// <param name="player">The player who collected the item.</param>
        public virtual void OnPlayerEnter(Player player)
        {
            if (IsCollected)
                return;

            IsCollected = true;
            Console.WriteLine($"[Collectible] Player collected the item.");
            SoundManager.Instance.PlaySound("assets/sfx/paper_pickup.wav");

            OnCollected?.Invoke();
        }

        /// <summary>
        /// Called when the player exits the trigger zone (default: no action).
        /// </summary>
        /// <param name="player">The player exiting the area.</param>
        public virtual void OnPlayerExit(Player player) { }

        /// <summary>
        /// Gets the original base position of the collectible.
        /// </summary>
        /// <returns>The base position vector.</returns>
        public Vector3 GetPosition()
        {
            return basePosition;
        }
    }
}