using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Ovladač hráče zodpovědný za zpracování fyziky a vstupů.
    /// Integruje síly, rychlost, akceleraci a zajišťuje pohyb hráče.
    /// </summary>
    public class PlayerController
    {
        private readonly Player player;

        /// <summary>
        /// Seznam sil, které se aplikují při příštím fyzikálním výpočtu.
        /// </summary>
        private readonly List<Vector3> forces = new();

        /// <summary>
        /// Požadovaná horizontální rychlost hráče (např. podle vstupu).
        /// </summary>
        private Vector3? desiredVelocity = null;

        /// <summary>
        /// Vektor gravitační síly (standardně směřuje dolů).
        /// </summary>
        public Vector3 Gravity { get; set; } = new Vector3(0, -15f, 0);

        /// <summary>
        /// Koeficient tření aplikovaný při dotyku se zemí.
        /// </summary>
        public float GroundFriction { get; set; } = 8.0f;

        /// <summary>
        /// Míra, jakou hráč reaguje na vstup – síla směrového ovládání.
        /// </summary>
        public float ControlFactor { get; set; } = 20f;

        /// <summary>
        /// Inicializuje nový instanci <see cref="PlayerController"/> pro daného hráče.
        /// </summary>
        public PlayerController(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Přidá sílu, která bude aplikována při následující aktualizaci.
        /// </summary>
        public void AddForce(Vector3 force)
        {
            forces.Add(force);
        }

        /// <summary>
        /// Nastaví požadovanou horizontální rychlost hráče.
        /// </summary>
        public void MoveToward(Vector3 targetVelocity)
        {
            desiredVelocity = targetVelocity;
        }

        /// <summary>
        /// Aktualizuje pohyb hráče podle sil, vstupů a prostředí.
        /// </summary>
        /// <param name="dt">Delta time (čas mezi snímky v sekundách).</param>
        public void Update(float dt)
        {
            // Aplikuj gravitaci
            AddForce(Gravity * player.Mass);

            // Sečti všechny síly
            Vector3 totalForce = Vector3.Zero;
            foreach (var f in forces)
            {
                totalForce += f;
            }
            forces.Clear();

            // Výpočet akcelerace
            player.Acceleration = totalForce / player.Mass;

            // Integrace rychlosti a pozice (Eulerův krok)
            player.Velocity += player.Acceleration * dt;
            player.Position += player.Velocity * dt;

            // Detekce země – jednoduchá rovina ve výšce 0
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

            // Tření na zemi – snižuje horizontální rychlost
            if (player.IsOnGround)
            {
                Vector3 horizontal = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                horizontal -= horizontal * GroundFriction * dt;
                player.Velocity = new Vector3(horizontal.X, player.Velocity.Y, horizontal.Z);
            }

            // Odpor vzduchu – snižuje horizontální rychlost ve vzduchu
            if (!player.IsOnGround)
            {
                Vector3 horizontal = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 drag = horizontal * 2.0f; // Můžeš upravit intenzitu odporu
                player.Velocity -= new Vector3(drag.X, 0, drag.Z) * dt;
            }

            // Aplikace vstupu (směrového pohybu)
            if (desiredVelocity.HasValue)
            {
                Vector3 current = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
                Vector3 desired = desiredVelocity.Value;
                Vector3 diff = desired - current;

                // Síla směrem k požadované rychlosti
                Vector3 force = diff * ControlFactor * player.Mass;
                AddForce(new Vector3(force.X, 0, force.Z));

                desiredVelocity = null;
            }
        }
    }
}
