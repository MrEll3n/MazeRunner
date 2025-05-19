using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Billboard, který se při vykreslení automaticky natočí ke kameře.
    /// Lze uzamknout vybrané osy (X/Y/Z), aby se billboard otáčel jen kolem určitých os.
    /// </summary>
    public class Billboard : Quad
    {
        private Vector3 position;
        private Vector2 size;
        private bool alignX, alignY, alignZ;

        public Billboard(Vector3 position, Vector2 size, bool alignX = false, bool alignY = true, bool alignZ = false)
            : base(
                  center: Vector3.Zero,             // placeholder
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

        public override Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateTranslation(position);
        }

        public override void Draw(Camera camera)
        {
            // Směr ke kameře
            Vector3 look = camera.Position - position;

            // Uzamkni vybrané osy
            if (alignX) look.X = 0;
            if (alignY) look.Y = 0;
            if (alignZ) look.Z = 0;

            // Prevence nuly
            if (look.LengthSquared < 0.0001f)
                look = -camera.Front;

            Vector3 forward = look.Normalized();
            Vector3 right = Vector3.Cross(Vector3.UnitY, forward).Normalized();
            Vector3 up = Vector3.Cross(forward, right);

            // Sestav transformační matici
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
        
        public void SetPosition(Vector3 newPosition)
        {
            this.position = newPosition;
        }
    }
}