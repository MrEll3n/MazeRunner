using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ZPG
{
    /// <summary>
    /// Renders 2D HUD text using a bitmap font texture arranged in a 16x8 character grid.
    /// Each character is drawn as a textured quad. Supports alignment and scaling based on screen resolution.
    /// </summary>
    public class HudRenderer
    {
        private readonly int textureId;
        private readonly Shader shader;
        private int vao, vbo;
        private const int CHAR_SIZE = 32;
        private const int CHAR_COLS = 16;
        private const int CHAR_ROWS = 8;

        /// <summary>
        /// Initializes the HUD renderer by loading the font texture and setting up OpenGL buffers.
        /// </summary>
        /// <param name="texturePath">Path to the font texture image.</param>
        public HudRenderer(string texturePath)
        {
            textureId = TextureLoader.LoadTexture(texturePath);
            shader = new Shader("shaders/hud.vert", "shaders/hud.frag");

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, 6 * sizeof(float) * 4 * 128, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Renders a string of text on the screen at the specified normalized coordinates and size.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="normX">Normalized horizontal screen position (0 to 1).</param>
        /// <param name="normY">Normalized vertical screen position (0 to 1).</param>
        /// <param name="sizeNorm">Relative size multiplier (based on 1080p height).</param>
        /// <param name="screenWidth">Current screen width in pixels.</param>
        /// <param name="screenHeight">Current screen height in pixels.</param>
        /// <param name="align">Text alignment (Left, Center, Right).</param>
        public void DrawText(
            string text,
            float normX,
            float normY,
            float sizeNorm,
            int screenWidth,
            int screenHeight,
            TextAlign align = TextAlign.Left)
        {
            float baseHeight = 1080f;
            float scale = (screenHeight / baseHeight) * sizeNorm;

            float textWidth = text.Length * CHAR_SIZE * scale;
            float x = normX * screenWidth;
            float y = normY * screenHeight;

            // Zarovnání podle typu
            switch (align)
            {
                case TextAlign.Center:
                    x -= textWidth / 2f;
                    break;
                case TextAlign.Right:
                    x -= textWidth;
                    break;
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(vao);
            shader.Use();
            shader.SetUniform("projection", Matrix4.CreateOrthographicOffCenter(0, screenWidth, 0, screenHeight, -1, 1));

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            shader.SetUniform("uTexture", 0);

            List<float> vertices = new();

            for (int i = 0; i < text.Length; i++)
            {
                int ascii = (int)text[i];
                if (ascii < 32 || ascii > 127)
                    continue;

                int index = ascii;
                int col = index % CHAR_COLS;
                int row = index / CHAR_COLS;

                float tw = 1.0f / CHAR_COLS;
                float th = 1.0f / CHAR_ROWS;

                float tx = col * tw;
                float ty = 1.0f - (row + 1) * th;

                float tx1 = tx;
                float tx2 = tx + tw;

                float xpos = x + i * CHAR_SIZE * scale;
                float ypos = y;

                float w = CHAR_SIZE * scale;
                float h = CHAR_SIZE * scale;

                vertices.AddRange(new float[] {
                    xpos,     ypos,     tx1,  ty,
                    xpos + w, ypos,     tx2,  ty,
                    xpos,     ypos + h, tx1,  ty + th,

                    xpos + w, ypos,     tx2,  ty,
                    xpos + w, ypos + h, tx2,  ty + th,
                    xpos,     ypos + h, tx1,  ty + th
                });
            }

            if (vertices.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Count * sizeof(float), vertices.ToArray());
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count / 4);
            }

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}