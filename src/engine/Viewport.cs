using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Viewport maps normalized device coordinates (-1 to 1) to a portion of the window using normalized window coordinates (0 to 1) and vice versa.
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// Normalized top coordinate of the window (0 = top edge).
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// Normalized left coordinate of the window (0 = left edge).
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// Width of the viewport (1 = full width of the window).
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Height of the viewport (1 = full height of the window).
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Associated window used to determine pixel dimensions.
        /// </summary>
        public Window Window { get; set; } = null!;

        public Viewport() { }

        /// <summary>
        /// Aspect ratio of the viewport.
        /// </summary>
        public float AspectRatio => (float)Window.Size.X / (float)Window.Size.Y;

        /// <summary>
        /// Applies this viewport to the OpenGL rendering context.
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
        /// Clears the current viewport area with a transparent color.
        /// </summary>
        public void Clear()
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Converts window pixel coordinates to normalized device coordinates based on viewport position and size.
        /// </summary>
        /// <param name="x">X coordinate in pixels.</param>
        /// <param name="y">Y coordinate in pixels.</param>
        /// <returns>Coordinates in normalized device space (-1 to 1).</returns>
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
