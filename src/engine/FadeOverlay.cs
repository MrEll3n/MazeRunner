using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace ZPG
{
    /// <summary>
    /// Manages a full-screen fade-in and fade-out visual overlay with alpha blending.
    /// Used to transition scenes or teleport the player with a smooth visual effect.
    /// </summary>
    public class FadeOverlay
    {
        /// <summary>
        /// The shader used to render the full-screen fade effect.
        /// </summary>
        public Shader Shader { get; set; }

        /// <summary>
        /// Current transparency level (0 = transparent, 1 = opaque).
        /// </summary>
        public float Alpha { get; private set; } = 0f;

        /// <summary>
        /// Whether a fade is currently active (in progress).
        /// </summary>
        public bool IsActive { get; private set; } = false;

        private bool fadingIn = false;
        private bool fadingOut = false;
        private const float fadeSpeed = 1.5f;
        private Action onFadeInComplete;

        /// <summary>
        /// Starts the fade-in followed by fade-out sequence. Triggers the provided callback when fade-in completes.
        /// </summary>
        /// <param name="onFadeInDone">Callback invoked after fade-in completes and before fade-out begins.</param>
        public void StartFade(Action onFadeInDone)
        {
            if (IsActive) return;

            Alpha = 0f;
            fadingIn = true;
            fadingOut = false;
            IsActive = true;
            onFadeInComplete = onFadeInDone;
        }

        /// <summary>
        /// Updates the fade progress based on delta time. Handles both fade-in and fade-out transitions.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            if (!IsActive) return;

            if (fadingIn)
            {
                Alpha += fadeSpeed * dt;
                if (Alpha >= 1f)
                {
                    Alpha = 1f;
                    fadingIn = false;
                    onFadeInComplete?.Invoke();
                    fadingOut = true;
                }
            }
            else if (fadingOut)
            {
                Alpha -= fadeSpeed * dt;
                if (Alpha <= 0f)
                {
                    Alpha = 0f;
                    fadingOut = false;
                    IsActive = false;
                }
            }
        }

        /// <summary>
        /// Renders the full-screen quad with the current alpha transparency, using the assigned shader.
        /// </summary>
        /// <param name="width">Viewport width.</param>
        /// <param name="height">Viewport height.</param>
        public void DrawFullScreenQuad(int width, int height)
        {
            if (!IsActive || Alpha <= 0f || Shader == null)
                return;

            if (!initialized)
                InitQuad();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shader.Use();
            Shader.SetUniform("uAlpha", Alpha);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }
        
        private int vao;
        private int vbo;
        private bool initialized = false;

        private void InitQuad()
        {
            float[] quadVertices = {
                // positions     // texCoords
                -1.0f,  1.0f,     0.0f, 1.0f,
                -1.0f, -1.0f,     0.0f, 0.0f,
                1.0f,  1.0f,     1.0f, 1.0f,
                1.0f, -1.0f,     1.0f, 0.0f,
            };

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            initialized = true;
        }

    }
}
