using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ZPG
{
    /// <summary>
    /// Abstraktní základ pro 3D modely – obsahuje geometrii (vrcholy, trojúhelníky), pozici, texturu a logiku vykreslení.
    /// </summary>
    public abstract class Model : IDisposable
    {
        public Shader Shader;
        public Vector3 Position = new Vector3(0, 0, 0);
        public int TextureID { get; set; } = -1;

        public BindingList<Vertex> Vertices { get; set; } = new();
        public BindingList<Triangle> Triangles { get; set; } = new();

        int vbo, ebo, vao;

        public void Construct()
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vboData = new List<float>();
            foreach (var v in Vertices)
                vboData.AddRange(v.ToArray());

            GL.BufferData(BufferTarget.ArrayBuffer, vboData.Count * sizeof(float), vboData.ToArray(), BufferUsageHint.StaticDraw);

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            var eboData = new List<int>();
            foreach (var tri in Triangles)
            {
                eboData.Add(tri.I1);
                eboData.Add(tri.I2);
                eboData.Add(tri.I3);
            }

            GL.BufferData(BufferTarget.ElementArrayBuffer, eboData.Count * sizeof(int), eboData.ToArray(), BufferUsageHint.StaticDraw);

            int stride = VertexGL.SizeOf();

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

            GL.BindVertexArray(0);
        }

        protected void ComputeNormals(IList<Vertex> vertices, IList<Triangle> triangles)
        {
            foreach (var v in vertices)
                v.Normal = Vector3.Zero;

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

            foreach (var v in vertices)
                v.Normal = v.Normal.Normalized();
        }

        /// <summary>
        /// Vrací transformační matici modelu. Lze přepsat v potomcích.
        /// </summary>
        public virtual Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateTranslation(Position);
        }

        /// <summary>
        /// Vykreslí model pomocí aktuálního shaderu a kamery.
        /// </summary>
        public void Draw(Camera camera)
        {
            Matrix4 modelMatrix = GetModelMatrix();

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

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
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
