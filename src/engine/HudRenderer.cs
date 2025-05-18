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
        private const int CHAR_COLS = 16;
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
                    // Triangle 1 (Top-left, Top-right, Bottom-left)
                    xpos,     ypos,     tx1,  ty,
                    xpos + w, ypos,     tx2,  ty,
                    xpos,     ypos + h, tx1,  ty + th,

                    // Triangle 2 (Bottom-left, Top-right, Bottom-right)
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