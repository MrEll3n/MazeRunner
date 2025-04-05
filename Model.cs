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
            GL.BufferData(BufferTarget.ArrayBuffer, vboData.Count * sizeof(float), vboData.ToArray(), BufferUsageHint.StaticDraw);

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
            GL.BufferData(BufferTarget.ElementArrayBuffer, eboData.Count * sizeof(int), eboData.ToArray(), BufferUsageHint.StaticDraw);

            int stride = VertexGL.SizeOf();

            // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // TexCoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // Normal
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

            GL.BindVertexArray(0);
        }

        protected void ComputeNormals(IList<Vertex> vertices, IList<Triangle> triangles)
        {
            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Normal = Vector3.Zero;

            foreach (var tri in triangles)
            {
                Vector3 v0 = vertices[tri.I1].Position;
                Vector3 v1 = vertices[tri.I2].Position;
                Vector3 v2 = vertices[tri.I3].Position;

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector3 faceNormal = Vector3.Cross(edge1, edge2).Normalized();

                vertices[tri.I1].Normal += faceNormal;
                vertices[tri.I2].Normal += faceNormal;
                vertices[tri.I3].Normal += faceNormal;
            }

            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Normal = vertices[i].Normal.Normalized();
        }

        public void Draw(Camera camera)
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(Position);

            Shader.Use();
            Shader.SetUniform("projection", camera.Projection);
            Shader.SetUniform("view", camera.View);
            Shader.SetUniform("model", modelMatrix);

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
