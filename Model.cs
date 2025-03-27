using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ZPG
{
    /// <summary>
    /// Jednoduchý model obsahující seznam vrcholů
    /// </summary>
    public class Model : IDisposable
    {

        public Shader Shader;

        public Vector3 position = new Vector3(0,0,0);

        /// <summary>
        /// BindingList umožňuje snadno spojit kolekci s ListBoxem
        /// </summary>
        public BindingList<Vertex> Vertices { get; set; } = new();

        /// <summary>
        /// Seznam s indexy trojúhelníků
        /// </summary>
        public BindingList<Triangle> Triangles { get; set; } = new();   

        /// <summary>
        /// ID vertex buffer objectu (VBO)
        /// </summary>
        int vbo;
        
        /// <summary>
        /// Výchozí velikost VBO
        /// </summary>
        int vboSize = 10;

        int ibo;
        int iboSize = 10;

        /// <summary>
        /// ID vertex array objectu
        /// </summary>
        int vao;

        /// <summary>
        /// Změna v modelu. Pokud došlo ke změně, přegenerují se data do VBO
        /// </summary>
        public bool Changed { get; set; } = true;
        public Model()
        {
            // vytvoření a připojení VAO
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // Vytvoření a připojení VBO
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // Alokace prázdné paměti o dané velikosti. IntPtr zero říká, že neposíláme žádná data, pošeleme je později
            GL.BufferData(BufferTarget.ArrayBuffer, vboSize * VertexGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // vytvoření a připojení IBO
            ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, iboSize * TriangleGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // namapování pointerů na lokace v Shaderu
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexGL.SizeOf(), IntPtr.Zero);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexGL.SizeOf(), (IntPtr)(3 * sizeof(float)));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Z editovatelných dat vytvoří data vhodná pro grafickou kartu (pole struktur)
        /// </summary>
        /// <returns></returns>
        VertexGL[] GenVertexGLData() { 
            return Vertices.Select(v => new VertexGL(v.Position, v.Color)).ToArray();
        }


        /// <summary>
        /// Z editovatelných dat vytvoří data vhodná pro grafickou kartu (pole struktur)
        /// </summary>
        /// <returns></returns>
        TriangleGL[] GenTriangleGLData()
        {
            return Triangles.Select(t => new TriangleGL(t)).ToArray();
        }


        /// <summary>
        /// Vykreslení modelu
        /// </summary>
        public void Draw(Camera camera)
        {
            Matrix4 translate = Matrix4.CreateTranslation((float)position.X, (float)position.Y, (float)position.Z);

            Shader.Use();
            Shader.SetUniform("projection", camera.Projection);
            Shader.SetUniform("view", camera.View);
            Shader.SetUniform("model", translate);

            // Připojení bufferu
            GL.BindVertexArray(vao);

            // Pokud došlo ke změně, přegenerují se data
            if (Changed)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                var Vertices = GenVertexGLData();

                // Pokud jsou data větší než aktuální buffer, zvětší se 
                if (Vertices.Length > vboSize)
                {
                    vboSize *= 2;
                    GL.BufferData(BufferTarget.ArrayBuffer, vboSize * VertexGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }

                // Nahrání dat
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0, Vertices.Length * VertexGL.SizeOf(), Vertices);

                var Triangles = GenTriangleGLData();

                if (Triangles.Length > iboSize)
                {
                    iboSize *= 2;
                    GL.BufferData(BufferTarget.ElementArrayBuffer, iboSize * TriangleGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                // Nahrání dat
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, Triangles.Length * TriangleGL.SizeOf(), Triangles);

                Changed = false;
            }

            //GL.Enable(EnableCap.CullFace);
            GL.LineWidth(5);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            // Vykreslení pole vrcholů
            GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Count);
            GL.DrawElements(PrimitiveType.Triangles, 3 * Vertices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        #region Dispose - uvolnění paměti
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
                if (disposing)
                {

                }
                GL.DeleteBuffer(vbo);
            }
        }

        ~Model() => Dispose(false);
        #endregion
    }
}
