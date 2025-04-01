using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Viewport umožňuje namapovat normalizované souřadnice zařízení (-1, 1) do části kontrolky pomocí 
    /// normalizovaných souřadnic okna (0, 1) a zpět. 
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// normalizovaná souřadnice okna (0 = horní okraj)
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// normalizovaná souřadnice okna (0 = levý okraj)
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// šířka (1 = celá šířka kontrolky)
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// výška (1 = celá výška kontrolky)
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// propojená kontrolka (potřeba kvůli rozměrům)
        /// </summary>
        public Window Window { get; set; } = null!;

        private float _macMultiplier = 1.0f;

        public Viewport() { }

        /// <summary>
        /// Poměr stran viewportu
        /// </summary>
        public float AspectRatio => (float)Window.Size.X / Window.Size.Y;

        /// <summary>
        /// nastaví tento viewport jako platný
        /// </summary>
        public void Set()
        {
            GL.Viewport(
                0,
                0,
                (int)(Width * Window.Width),
                (int)(Height * Window.Height)
            );
        }

        /// <summary>
        /// vyčistí oblast tohoto viewportu
        /// </summary>
        public void Clear()
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        /// <summary>
        /// převede souřadnice okna na normalizované souřadnice zařízení podle umístění a velikosti viewportu
        /// </summary>
        /// <param name="x">x souřadnice v pixelech</param>
        /// <param name="y">y souřadnice v pixelech</param>
        /// <returns>souřadnice v normalizovaném prostoru zařízení (-1, 1)</returns>
        public Vector3 WindowViewport(int x, int y)
        {
            return new Vector3(
                ((float)x / Window.Width - Left) / Width * 2 - 1,
                -(((float)y / Window.Height - Top) / Height * 2 - 1),
                0
            );
        }
    }
}