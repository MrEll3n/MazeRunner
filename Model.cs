using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace ZPG
{
    public abstract class Model : IDisposable
    {
        public Shader Shader;
        public Vector3 Position = new Vector3(0, 0, 0);
        public int TextureID { get; set; } = -1;

        public BindingList<Vertex> Vertices { get; set; } = new();
        public BindingList<Triangle> Triangles { get; set; } = new();

        int vbo;
        int vboSize = 10;

        int ebo;
        int eboSize = 10;

        int vao;
        // public bool Changed { get; set; } = true;

        public void Construct()
        {
            // VAO setup
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // VBO setup
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            List<float> vboData = new List<float>();
            for (int i = 0; i < Vertices.Count; i++)
                vboData.AddRange(Vertices[i].ToArray());
            GL.BufferData(BufferTarget.ArrayBuffer, vboData.Count * VertexGL.SizeOf(), vboData.ToArray(), BufferUsageHint.StaticDraw);

            // EBO setup
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            List<int> eboData = new List<int>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                eboData.Add(Triangles[i].I1);
                eboData.Add(Triangles[i].I2);
                eboData.Add(Triangles[i].I3);
            }
            GL.BufferData(BufferTarget.ElementArrayBuffer, eboData.Count * TriangleGL.SizeOf(), eboData.ToArray(), BufferUsageHint.StaticDraw);

            // Vertex attribute pointers
            int stride = VertexGL.SizeOf();

            // Position - layout(location = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // TexCoord - layout(location = 1)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Draw(Camera camera)
        {
            // Console.WriteLine($"Drawing model at {Position} with {Vertices.Count} vertices, {Triangles.Count} triangles, TextureID: {TextureID}");

            Matrix4 translate = Matrix4.CreateTranslation(Position);

            Shader.Use();
            Shader.SetUniform("projection", camera.Projection);
            Shader.SetUniform("view", camera.View);
            Shader.SetUniform("model", translate);

            if (TextureID > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                Shader.SetUniform("uTexture", 0);
            }

            GL.BindVertexArray(vao);

            /*
            if (Changed)
            {
                var vertexData = GenVertexGLData();
                var triangleData = GenTriangleGLData();

                Console.WriteLine($"Updating vertex buffer with {vertexData.Length} vertices");
                Console.WriteLine($"Updating index buffer with {triangleData.Length} triangles");

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                if (vertexData.Length > vboSize)
                {
                    vboSize *= 2;
                    GL.BufferData(BufferTarget.ArrayBuffer, vboSize * VertexGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexData.Length * VertexGL.SizeOf(), vertexData);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                if (triangleData.Length > eboSize)
                {
                    eboSize *= 2;
                    GL.BufferData(BufferTarget.ElementArrayBuffer, eboSize * TriangleGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, triangleData.Length * TriangleGL.SizeOf(), triangleData);

                Changed = false;
            }
            */

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawElements(PrimitiveType.Triangles, Triangles.Count * 3, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // Debug first UV coord
            // var debugData = GenVertexGLData();
            // Console.WriteLine("UV prvního vrcholu: " + debugData[0].texCoord);
            // Console.WriteLine("UV druhého vrcholu: " + debugData[1].texCoord);
            // Console.WriteLine("UV třetího vrcholu: " + debugData[2].texCoord);
            // Console.WriteLine("UV třetího vrcholu: " + debugData[3].texCoord);
        }

        #region Dispose

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) { }

                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(ebo);
                GL.DeleteVertexArray(vao);
                disposed = true;
            }
        }

        ~Model() => Dispose(false);

        #endregion
    }
}
