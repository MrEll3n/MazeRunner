using OpenTK.Graphics.OpenGL;

namespace ZPG
{
    /// <summary>
    /// Viewport umožňuje namapovat normalizované souřadnice zařízení (-1, 1) do části kontorolky pomocí 
    /// normalizovaných souřadnice okna (0, 1) a zpět. 
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// normalizovaná souřadnice okna (0 = horní okraj)
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// normalizovaná souřadnice okna (0 = levý okraj
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// šířka (1 = celá šířka kontrolky)
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// výška (1 = celá výška kontrolky)
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// propojená kontrolka (potřeba kvůli rozměrům)
        /// </summary>
        public Window Window { get; set; }


        public Viewport() { }

        /// <summary>
        /// nastaví tento viewport jako platný
        /// </summary>
        public void Set()
        {
            GL.Viewport((int)(Left * Window.Width),
                (int)((1-(Top+Height)) * Window.Height),
                (int)(Width * Window.Width),
                (int)(Height * Window.Height)
            );
        }

        /// <summary>
        /// vyčistí oblast tohoto viewportu
        /// </summary>
        public void Clear()
        {
            // Scissort test nastaví platnou část bufferu - na mazání se nevztahuje GL.viewport
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor((int)(Left * Window.Width),
                (int)((1-Top-Height) * Window.Height),
                (int)(Width * Window.Width),
                (int)(Height * Window.Height)
            );
            GL.ClearColor(0,0,0,0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// převede soužadnice okna na normalizované souřadnice zařízení podle umístění a velikosti viewportu
        /// </summary>
        /// <param name="x">x souřadnice v pixelech</param>
        /// <param name="y">y souřadnice v pixelech</param>
        /// <returns>souřadnice v normalizovaném prostoru zařízení (-1, 1)</returns>
        public Vector3 WindowViewport(int x, int y)
        {
            return new Vector3(
                ((double)x / Window.Width - Left) / Width * 2 - 1,
                -(((double)y / Window.Height - Top) / Height * 2 - 1),
                0

            );
        }
    }
}
