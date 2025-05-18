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
        private bool isCollected = false;

        public Action OnCollected; // Akce, která se provede po sebrání

        private float triggerRadius;

        public Collectible(Vector3 position, Vector2 size)
            : base(position, size, alignX: false, alignY: true, alignZ: false)
        {
            triggerRadius = Math.Max(size.X, size.Y) * 0.6f;
        }

        /// <summary>
        /// Kontrola, zda hráč vstoupil do sběrné zóny.
        /// </summary>
        public void CheckTrigger(Player player)
        {
            if (isCollected)
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
            float collectibleRadius = 1.2f;
            float collectibleHeight = 2.5f;

            float playerYMin = player.Position.Y;
            float playerYMax = player.Position.Y + player.CameraHeight;

            float yBottom = GetPosition().Y - collectibleHeight / 2f;
            float yTop = GetPosition().Y + collectibleHeight / 2f;

            bool yOverlap = playerYMax >= yBottom && playerYMin <= yTop;

            Vector2 playerXZ = new(player.Position.X, player.Position.Z);
            Vector2 collectibleXZ = new(GetPosition().X, GetPosition().Z);
            float distXZ = (playerXZ - collectibleXZ).Length;

            return yOverlap && distXZ <= collectibleRadius;
        }

        public virtual void OnPlayerEnter(Player player)
        {
            if (isCollected)
                return;

            isCollected = true;
            Console.WriteLine($"[Collectible] Player collected the item.");

            // Spusť akci po sebrání
            OnCollected?.Invoke();
        }

        public virtual void OnPlayerExit(Player player)
        {
            // Neprovádíme nic, ale je zde kvůli rozhraní
        }

        public Vector3 GetPosition()
        {
            return base.GetModelMatrix().ExtractTranslation();
        }
    }
}