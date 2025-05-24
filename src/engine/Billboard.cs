using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// A screen-aligned quad (billboard) that automatically rotates to face the camera.
    /// You can lock specific axes (X/Y/Z) to restrict the billboard's rotation.
    /// </summary>
    public class Billboard : Quad
    {
        private Vector3 position;
        private Vector2 size;
        private bool alignX, alignY, alignZ;

        /// <summary>
        /// Creates a new billboard object with configurable axis alignment.
        /// </summary>
        /// <param name="position">The world-space position of the billboard's center.</param>
        /// <param name="size">The 2D size (width, height) of the billboard.</param>
        /// <param name="alignX">If true, lock rotation along the X axis.</param>
        /// <param name="alignY">If true, lock rotation along the Y axis (vertical default).</param>
        /// <param name="alignZ">If true, lock rotation along the Z axis.</param>
        public Billboard(Vector3 position, Vector2 size, bool alignX = false, bool alignY = true, bool alignZ = false)
            : base(
                  center: Vector3.Zero,             // Placeholder (overridden by position)
                  rightDir: Vector3.UnitX,
                  upDir: Vector3.UnitY,
                  width: size.X,
                  height: size.Y,
                  flip: false,
                  uvRepeat: false
              )
        {
            this.position = position;
            this.size = size;
            this.alignX = alignX;
            this.alignY = alignY;
            this.alignZ = alignZ;
        }

        /// <summary>
        /// Returns the model matrix used for positioning the billboard in the world.
        /// This override ignores rotation as the billboard rotates manually in Draw().
        /// </summary>
        public override Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateTranslation(position);
        }

        /// <summary>
        /// Draws the billboard quad so it always faces the camera, using axis constraints.
        /// </summary>
        /// <param name="camera">The active camera viewing the scene.</param>
        public override void Draw(Camera camera)
        {
            // Vector from billboard to camera
            Vector3 look = camera.Position - position;

            // Lock axes if specified
            if (alignX) look.X = 0;
            if (alignY) look.Y = 0;
            if (alignZ) look.Z = 0;

            // Prevent zero vector
            if (look.LengthSquared < 0.0001f)
                look = -camera.Front;

            Vector3 forward = look.Normalized();
            Vector3 right = Vector3.Cross(Vector3.UnitY, forward).Normalized();
            Vector3 up = Vector3.Cross(forward, right);

            // Construct rotation matrix (world orientation)
            Matrix4 rotation = new Matrix4(
                new Vector4(right.X, right.Y, right.Z, 0),
                new Vector4(up.X, up.Y, up.Z, 0),
                new Vector4(forward.X, forward.Y, forward.Z, 0),
                new Vector4(0, 0, 0, 1)
            );

            Matrix4 modelMatrix = rotation * Matrix4.CreateTranslation(position);

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

        /// <summary>
        /// Updates the world-space position of the billboard.
        /// </summary>
        /// <param name="newPosition">The new center position.</param>
        public void SetPosition(Vector3 newPosition)
        {
            this.position = newPosition;
        }
    }
}