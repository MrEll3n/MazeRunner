using OpenTK.Graphics.OpenGL;
using System.ComponentModel;

namespace ZPG
{
    /// <summary>
    /// Jednoduchý model obsahující seznam vrcholů
    /// </summary>
    public class Model : IDisposable
    {
        /// <summary>
        /// BindingList umožňuje snadno spojit kolekci s ListBoxem
        /// </summary>
        public BindingList<Vertex> vertices { get; set; } = new();

        /// <summary>
        /// Seznam s indexy trojúhelníků
        /// </summary>
        public BindingList<Triangle> triangles { get; set; } = new();   

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
        /// Změna v modelu. Pokud došlo ke změně, přegenerují se data do VBO
        /// </summary>
        public bool Changed { get; set; } = true;
        public Model()
        {
            // Vytvoření ID pro VBO
            vbo = GL.GenBuffer();
            // Připojení VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // Alokace prázdné paměti o dané velikosti. IntPtr zero říká, že neposíláme žádná data, pošeleme je později
            GL.BufferData(BufferTarget.ArrayBuffer, vboSize * VertexGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, iboSize * TriangleGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        }

        /// <summary>
        /// Z editovatelných dat vytvoří data vhodná pro grafickou kartu (pole struktur)
        /// </summary>
        /// <returns></returns>
        VertexGL[] GenVertexGLData() { 
            return vertices.Select(v => new VertexGL(v.Position, v.Color)).ToArray();
        }


        /// <summary>
        /// Z editovatelných dat vytvoří data vhodná pro grafickou kartu (pole struktur)
        /// </summary>
        /// <returns></returns>
        TriangleGL[] GenTriangleGLData()
        {
            return triangles.Select(t => new TriangleGL(t)).ToArray();
        }


        /// <summary>
        /// Vykreslení modelu
        /// </summary>
        public void Draw()
        {

            // Připojení bufferu
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            // Pokud došlo ke změně, přegenerují se data
            if (Changed)
            {                
                var vertices = GenVertexGLData();

                // Pokud jsou data větší než aktuální buffer, zvětší se 
                if (vertices.Length > vboSize)
                {
                    vboSize *= 2;
                    GL.BufferData(BufferTarget.ArrayBuffer, vboSize * VertexGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }

                // Nahrání dat
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * VertexGL.SizeOf(), vertices);

                var triangles = GenTriangleGLData();

                if(triangles.Length > iboSize)
                {
                    iboSize *= 2;
                    GL.BufferData(BufferTarget.ElementArrayBuffer, iboSize * TriangleGL.SizeOf(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                // Nahrání dat
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, triangles.Length * TriangleGL.SizeOf(), triangles);

                Changed = false;
            }

            // Povolení poloh vrcholů a barev a nastavení odpovídajících ukazatelů do paměti
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.VertexPointer(3, VertexPointerType.Float, VertexGL.SizeOf(), IntPtr.Zero);
            GL.ColorPointer(3, ColorPointerType.Float, VertexGL.SizeOf(), (IntPtr)(3 * sizeof(float)));

            //GL.Enable(EnableCap.CullFace);
            GL.LineWidth(5);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            // Vykreslení pole vrcholů
            GL.DrawArrays(PrimitiveType.Points, 0, vertices.Count);
            GL.DrawElements(PrimitiveType.Triangles, 3*triangles.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

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
