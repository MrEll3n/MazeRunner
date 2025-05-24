using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System;

namespace ZPG
{
    /// <summary>
    /// Represents an animated diamond-shaped teleport trigger.
    /// When the player enters the trigger area, a teleportation
    /// sequence with delay and cooldown is executed.
    /// </summary>
    public class TeleportTrigger : Model, ITriggerZone
    {
        /// <summary>Unique identifier of the teleport trigger (e.g. tile symbol in map).</summary>
        public char Id { get; set; }

        /// <summary>Target position to which the player will be teleported.</summary>
        public Vector3 TargetPosition { get; set; }

        /// <summary>Time (in seconds) to wait before teleportation occurs after activation.</summary>
        public float DelayBeforeTeleport { get; set; } = 1.0f;

        /// <summary>Cooldown period (in seconds) after teleportation.</summary>
        public float CooldownAfterTeleport { get; set; } = 6.0f;

        /// <summary>Alpha transparency for rendering the teleport model.</summary>
        public float Transparency { get; set; } = 0.65f;

        private float animationTime = 0f;
        private float basePosY = 0f;
        private float rotationAngle = 0f;

        private Player currentPlayer = null;
        private float delayTimer = 0f;
        private float cooldownTimer = 0f;
        private bool isTeleporting = false;

        private static float globalCooldown = 0f;
        private static readonly object cooldownLock = new();

        /// <summary>Width of the teleport model.</summary>
        public float Width { get; private set; }

        /// <summary>Height of the teleport model.</summary>
        public float Height { get; private set; }

        /// <summary>Depth of the teleport model.</summary>
        public float Depth { get; private set; }

        private float horizRadius;
        private float vertTolerance;

        /// <summary>Reference to the screen fade overlay used during teleportation.</summary>
        public FadeOverlay FadeOverlay { get; set; }

        /// <summary>
        /// Recomputes the bounding collision radius based on model dimensions.
        /// </summary>
        private void RecomputeCollisionBounds()
        {
            horizRadius = MathF.Max(Width, Depth) * 0.5f + 0.1f;
            vertTolerance = Height * 0.5f + 0.1f;
        }

        /// <summary>
        /// Constructs a new teleport trigger with the specified size.
        /// </summary>
        /// <param name="size">Visual size (diameter) of the diamond-shaped model.</param>
        public TeleportTrigger(float size = 1.6f)
        {
            Width = Height = Depth = size;
            BuildDiamondMesh(size);
            ComputeNormals(Vertices, Triangles);
            Construct();

            try
            {
                TextureID = TextureLoader.LoadTexture("textures/teleport.png");
            }
            catch
            {
                TextureID = 0;
            }
        }

        /// <summary>
        /// Computes and returns the model transformation matrix.
        /// </summary>
        public override Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateRotationY(rotationAngle)
                   * Matrix4.CreateTranslation(Position);
        }

        /// <summary>
        /// Generates a diamond-shaped mesh for the teleport model.
        /// </summary>
        private void BuildDiamondMesh(float size)
        {
            Vertices.Clear();
            Triangles.Clear();

            float h = 1.0f;
            float stretch = 1.3f;

            Vertices.Add(new Vertex(new Vector3(0, h * stretch, 0), new Vector2(0.5f, 1f))); // Top
            Vertices.Add(new Vertex(new Vector3(0, -h * stretch, 0), new Vector2(0.5f, 0f))); // Bottom

            Vertices.Add(new Vertex(new Vector3(0, 0, -h), new Vector2(0.5f, .5f))); // North
            Vertices.Add(new Vertex(new Vector3(h, 0, 0), new Vector2(1f, .5f)));    // East
            Vertices.Add(new Vertex(new Vector3(0, 0, h), new Vector2(0.5f, .5f)));  // South
            Vertices.Add(new Vertex(new Vector3(-h, 0, 0), new Vector2(0f, .5f)));   // West

            AddFace(0, 3, 2);
            AddFace(0, 4, 3);
            AddFace(0, 5, 4);
            AddFace(0, 2, 5);

            AddFace(2, 3, 1);
            AddFace(3, 4, 1);
            AddFace(4, 5, 1);
            AddFace(5, 2, 1);

            void AddFace(int a, int b, int c) => Triangles.Add(new Triangle(a, b, c));

            RecomputeCollisionBounds();
        }

        /// <summary>
        /// Sets the base position of the model and stores its Y value.
        /// </summary>
        public void SetBasePosition(Vector3 pos)
        {
            Position = pos;
            basePosY = pos.Y;
        }

        /// <summary>
        /// Checks if the player is within the teleport activation zone.
        /// </summary>
        public bool IsColliding(Player plr)
        {
            float teleportRadius = 1.2f;
            float teleportHeight = 2.5f;

            float playerYMin = plr.Position.Y;
            float playerYMax = plr.Position.Y + plr.CameraHeight;

            float yBottom = basePosY - teleportHeight / 2f;
            float yTop = basePosY + teleportHeight / 2f;

            bool yOverlap = playerYMax >= yBottom && playerYMin <= yTop;

            Vector2 playerXZ = new(plr.Position.X, plr.Position.Z);
            Vector2 triggerXZ = new(Position.X, Position.Z);
            float distXZ = (playerXZ - triggerXZ).Length;

            return yOverlap && distXZ <= teleportRadius;
        }

        /// <summary>
        /// Handles the logic when the player enters the teleport trigger.
        /// </summary>
        public void OnPlayerEnter(Player plr)
        {
            if (IsGlobalCooldown() || cooldownTimer > 0f) return;

            if (currentPlayer == null)
            {
                Console.WriteLine($"[DEBUG] OnPlayerEnter: player entered teleport {Id}.");
                currentPlayer = plr;
                delayTimer = 0f;

                // SoundManager.Instance.PlaySound("assets/sfx/teleport_start.wav", Position);
            }
        }

        /// <summary>
        /// Handles the logic when the player leaves the teleport trigger.
        /// </summary>
        public void OnPlayerExit(Player plr)
        {
            if (plr == currentPlayer && !isTeleporting && cooldownTimer <= 0f)
            {
                currentPlayer = null;
                delayTimer = 0f;
            }
        }

        /// <summary>
        /// Updates the teleport trigger state (animation, cooldown, delay).
        /// </summary>
        public void Update(float dt)
        {
            animationTime += dt;
            Position = new Vector3(Position.X,
                                   basePosY + 0.1f * MathF.Sin(animationTime * 2f),
                                   Position.Z);

            rotationAngle += dt * 0.5f;
            if (rotationAngle > MathF.Tau) rotationAngle -= MathF.Tau;

            TickGlobalCooldown(dt);

            if (cooldownTimer > 0f)
            {
                cooldownTimer = MathF.Max(0f, cooldownTimer - dt);
                return;
            }

            if (currentPlayer != null)
            {
                delayTimer += dt;
                if (delayTimer >= DelayBeforeTeleport)
                    ExecuteTeleport();
            }
        }

        /// <summary>
        /// Executes the teleportation and resets timers.
        /// </summary>
        private void ExecuteTeleport()
        {
            if (currentPlayer == null || FadeOverlay == null || FadeOverlay.IsActive)
                return;

            Console.WriteLine("[TeleportTrigger] Starting teleport fade...");
            isTeleporting = true;

            SoundManager.Instance.PlaySound("assets/sfx/teleport_end.wav", TargetPosition);

            FadeOverlay.StartFade(() =>
            {
                Console.WriteLine("[TeleportTrigger] Teleporting player.");
                currentPlayer.Position = TargetPosition + new Vector3(0, -1f, 0);
                currentPlayer.Velocity = Vector3.Zero;

                cooldownTimer = CooldownAfterTeleport;
                SetGlobalCooldown(CooldownAfterTeleport);

                currentPlayer = null;
                delayTimer = 0f;
                isTeleporting = false;
            });
        }

        /// <summary>
        /// Sets the global cooldown for all teleport triggers.
        /// </summary>
        private static void SetGlobalCooldown(float t)
        {
            lock (cooldownLock)
            {
                globalCooldown = MathF.Max(globalCooldown, t);
            }
        }

        /// <summary>
        /// Updates the global cooldown timer.
        /// </summary>
        private static void TickGlobalCooldown(float dt)
        {
            lock (cooldownLock)
            {
                if (globalCooldown > 0f)
                    globalCooldown = MathF.Max(0f, globalCooldown - dt);
            }
        }

        /// <summary>
        /// Checks if a global cooldown is currently active.
        /// </summary>
        private static bool IsGlobalCooldown()
        {
            lock (cooldownLock) { return globalCooldown > 0f; }
        }

        /// <summary>
        /// Renders the teleport trigger to the screen.
        /// </summary>
        public override void Draw(Camera camera)
        {
            Matrix4 modelMatrix = GetModelMatrix();

            Shader.Use();
            Shader.SetUniform("projection", camera.Projection);
            Shader.SetUniform("view", camera.View);
            Shader.SetUniform("model", modelMatrix);
            Shader.SetUniform("uAlpha", Transparency);

            if (TextureID > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                Shader.SetUniform("uTexture", 0);
            }

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, Triangles.Count * 3, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }
    }
}