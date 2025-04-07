using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace ZPG
{
    /// <summary>
    /// Abstraktní základ pro 3D modely – obsahuje geometrii (vrcholy, trojúhelníky), pozici, texturu a logiku vykreslení.
    /// </summary>
    public abstract class Model : IDisposable
    {
        /// <summary>
        /// Shader, který se použije při vykreslení modelu.
        /// </summary>
        public Shader Shader;

        /// <summary>
        /// Pozice modelu ve světovém prostoru.
        /// </summary>
        public Vector3 Position = new Vector3(0, 0, 0);

        /// <summary>
        /// ID textury aplikované na model.
        /// </summary>
        public int TextureID { get; set; } = -1;

        /// <summary>
        /// Vrcholy modelu (pozice, normála, tex. souřadnice).
        /// </summary>
        public BindingList<Vertex> Vertices { get; set; } = new();

        /// <summary>
        /// Trojúhelníky tvořící povrch modelu (indexy do pole vrcholů).
        /// </summary>
        public BindingList<Triangle> Triangles { get; set; } = new();

        // OpenGL buffery
        int vbo;        // Vertex Buffer Object
        int ebo;        // Element Buffer Object
        int vao;        // Vertex Array Object

        /// <summary>
        /// Inicializuje GPU buffery (VBO, EBO, VAO) a připraví data pro vykreslení.
        /// </summary>
        public void Construct()
        {
            // VAO – uchovává vazby atributů a bufferů
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // VBO – naplnění vrcholových dat
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vboData = new List<float>();
            foreach (var v in Vertices)
                vboData.AddRange(v.ToArray());

            GL.BufferData(BufferTarget.ArrayBuffer, vboData.Count * sizeof(float), vboData.ToArray(), BufferUsageHint.StaticDraw);

            // EBO – indexy trojúhelníků
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

            int stride = VertexGL.SizeOf(); // velikost jednoho vrcholu v bajtech

            // Atributy vrcholu:
            // Pozice (vec3) → location 0
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // TexCoord (vec2) → location 1
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // Normála (vec3) → location 2
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

            GL.BindVertexArray(0); // odpojení VAO
        }

        /// <summary>
        /// Automatický výpočet normál z trojúhelníků pro nasvícení.
        /// </summary>
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
        /// Vykreslí model pomocí aktuálního shaderu a kamery.
        /// </summary>
        /// <param name="camera">Aktuální kamera pro nastavení view/projection matic.</param>
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
                Shader.SetUniform("uTexture", 0); // sampler2D uniform
            }

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, Triangles.Count * 3, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        #region IDisposable

        private bool disposed = false;

        /// <summary>
        /// Uvolní OpenGL buffery při zničení objektu.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                // uvolnění nativních zdrojů (OpenGL objekty)
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

