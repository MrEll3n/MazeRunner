using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace ZPG
{
    public class FadeOverlay
    {
        public Shader Shader { get; set; }

        public float Alpha { get; private set; } = 0f;
        public bool IsActive { get; private set; } = false;

        private bool fadingIn = false;
        private bool fadingOut = false;
        private const float fadeSpeed = 1.5f;
        private Action onFadeInComplete;

        public void StartFade(Action onFadeInDone)
        {
            if (IsActive) return;

            Alpha = 0f;
            fadingIn = true;
            fadingOut = false;
            IsActive = true;
            onFadeInComplete = onFadeInDone;
        }

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
