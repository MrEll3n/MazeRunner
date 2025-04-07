using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje hráčskou postavu ve 3D prostoru, včetně fyziky, kamery a svítilny.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Aktuální pozice hráče ve světovém prostoru.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Rychlost pohybu hráče v jednotlivých osách.
        /// </summary>
        public Vector3 Velocity { get; set; } = Vector3.Zero;

        /// <summary>
        /// Akcelerace hráče (např. vlivem gravitace).
        /// </summary>
        public Vector3 Acceleration { get; set; } = Vector3.Zero;

        /// <summary>
        /// Hmotnost hráče (využitelná pro síly, pokud bude potřeba).
        /// </summary>
        public float Mass { get; set; } = 80.0f;

        /// <summary>
        /// Indikuje, zda se hráč aktuálně nachází na zemi (umožňuje skákání).
        /// </summary>
        public bool IsOnGround { get; set; } = true;

        /// <summary>
        /// Kamera reprezentující hráčův pohled.
        /// </summary>
        public Camera Camera { get; private set; }

        /// <summary>
        /// Svítilna připojená k hráči.
        /// </summary>
        public Flashlight Flashlight { get; private set; }

        /// <summary>
        /// Výška očí hráče nad pozicí jeho těla.
        /// </summary>
        public float CameraHeight = 1.7f;

        /// <summary>
        /// Kolizní poloměr hráče při detekci kolizí se stěnami.
        /// </summary>
        public float CollisionRadius = 1.0f;

        private readonly PlayerController controller;

        /// <summary>
        /// Vytvoří nového hráče na zadané pozici a přiřadí mu kameru, ovladač a svítilnu.
        /// </summary>
        /// <param name="startPosition">Počáteční pozice hráče ve světě.</param>
        /// <param name="window">Okno pro výpočet poměru stran kamery.</param>
        public Player(Vector3 startPosition, Window window)
        {
            Position = startPosition;
            Camera = new Camera(Position + new Vector3(0, CameraHeight, 0)) { Window = window };
            Flashlight = new Flashlight(this);
            controller = new PlayerController(this);
        }

        /// <summary>
        /// Aplikuje sílu na hráče (např. pohyb nebo skok).
        /// </summary>
        public void AddForce(Vector3 force)
        {
            controller.AddForce(force);
        }

        /// <summary>
        /// Pohybuje hráče směrem k cílové rychlosti (např. vstupem z klávesnice).
        /// </summary>
        public void MoveToward(Vector3 targetVelocity)
        {
            controller.MoveToward(targetVelocity);
        }

        /// <summary>
        /// Uskuteční skok – nastaví vertikální rychlost a zruší IsOnGround.
        /// </summary>
        /// <param name="velocityY">Počáteční rychlost skoku (směrem nahoru).</param>
        public void Jump(float velocityY)
        {
            if (IsOnGround)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                Velocity += new Vector3(0, velocityY, 0);
                IsOnGround = false;
            }
        }

        /// <summary>
        /// Aktualizuje stav hráče včetně fyziky a kamery.
        /// </summary>
        /// <param name="deltaTime">Doba mezi snímky (v sekundách).</param>
        public void Update(float deltaTime)
        {
            controller.Update(deltaTime);
            Camera.Position = Position + new Vector3(0, CameraHeight, 0);
        }

        /// <summary>
        /// Provádí pohyb hráče s kolizí vůči trojúhelníkové síti stěn (mesh-based collision).
        /// Implementuje základní "move and slide" s korekcí pozice.
        /// </summary>
        /// <param name="desiredVelocity">Požadovaná rychlost hráče.</param>
        /// <param name="walls">Seznam stěn, se kterými se může hráč srazit.</param>
        /// <param name="deltaTime">Doba mezi snímky (v sekundách).</param>
        public void MoveAndSlideMeshBased(Vector3 desiredVelocity, IEnumerable<Wall> walls, float deltaTime)
        {
            float radius = 0.3f;

            Vector3 oldPosition = Position;
            Vector3 move = desiredVelocity * deltaTime;
            Vector3 newPos = Position + move;

            Vector3 totalCorrection = Vector3.Zero;

            // Kontrola kolizí se všemi trojúhelníky všech stěn
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

            newPos += totalCorrection;
            Position = newPos;

            // Výpočet reálného horizontálního pohybu hráče (např. pro zachování inerce)
            Vector3 realMovement = Position - oldPosition;
            Velocity = new Vector3(realMovement.X / deltaTime, Velocity.Y, realMovement.Z / deltaTime);
        }
    }
}
