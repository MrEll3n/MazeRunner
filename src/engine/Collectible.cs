using OpenTK.Mathematics;
using System;

namespace ZPG
{
    /// <summary>
    /// Objekt, který lze sebrat – chová se jako billboard a reaguje na kontakt s hráčem.
    /// </summary>
    public class Collectible : Billboard, ITriggerZone
    {
        private bool isPlayerInside = false;
        public bool IsCollected = false;

        public Action OnCollected;

        private float triggerRadius;

        private float animationTime = 0f;
        private Vector3 basePosition;

        public Collectible(Vector3 position, Vector2 size)
            : base(position, size, alignX: false, alignY: true, alignZ: false)
        {
            triggerRadius = Math.Max(size.X, size.Y) * 0.6f;
            basePosition = position;
        }

        public void Update(float dt)
        {
            if (IsCollected)
                return;

            animationTime += dt;

            // sinusové pohupování nahoru/dolů
            float offset = 0.1f * MathF.Sin(animationTime * 2f);
            SetPosition(basePosition + new Vector3(0, offset, 0));
        }

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

        public virtual void OnPlayerEnter(Player player)
        {
            if (IsCollected)
                return;

            IsCollected = true;
            Console.WriteLine($"[Collectible] Player collected the item.");
            SoundManager.Instance.PlaySound("assets/sfx/paper_pickup.wav");

            OnCollected?.Invoke();
        }

        public virtual void OnPlayerExit(Player player) { }

        public Vector3 GetPosition()
        {
            return basePosition;
        }
    }
}