using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ZPG
{
    public class HudRenderer
    {
        private readonly int textureId;
        private readonly Shader shader;
        private int vao, vbo;
        private const int CHAR_SIZE = 32;
        private const int CHAR_COLS = 8;
        private const int CHAR_ROWS = 8;

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

        public void DrawText(string text, float x, float y, float scale, int screenWidth, int screenHeight)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(vao);
            shader.Use();
            shader.SetUniform("projection", Matrix4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1));

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            shader.SetUniform("uTexture", 0);

            List<float> vertices = new();

            for (int i = 0; i < text.Length; i++)
            {
                int ascii = text[i] - 32;
                if (ascii < 0 || ascii >= CHAR_COLS * CHAR_ROWS)
                    continue;

                int col = ascii % CHAR_COLS;
                int row = ascii / CHAR_COLS;

                float tx = col / (float)CHAR_COLS;
                float ty = row / (float)CHAR_ROWS;
                float ts = 1.0f / CHAR_COLS;

                float xpos = x + i * CHAR_SIZE * scale;
                float ypos = y;

                float w = CHAR_SIZE * scale;
                float h = CHAR_SIZE * scale;

                // 2 triangles (6 vertices)
                vertices.AddRange(new float[] {
                    xpos,     ypos + h, tx,       ty + ts,
                    xpos + w, ypos,     tx + ts,  ty,
                    xpos,     ypos,     tx,       ty,

                    xpos,     ypos + h, tx,       ty + ts,
                    xpos + w, ypos + h, tx + ts,  ty + ts,
                    xpos + w, ypos,     tx + ts,  ty,
                });
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Count * sizeof(float), vertices.ToArray());

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count / 4);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}