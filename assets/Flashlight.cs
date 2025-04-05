using OpenTK.Mathematics;

namespace ZPG
{
    public class Flashlight
    {
        public float Height { get; set; } = 2.05f;
        public float DepressionDegrees { get; set; } = 2.0f;
        public float CutOffDegrees { get; set; } = 20f;
        public float OuterCutOffDegrees { get; set; } = 30f;

        private Player player;

        public Flashlight(Player player)
        {
            this.player = player;
        }

        public void Apply(Shader shader)
        {
            Vector3 lightPos = player.Position + new Vector3(0, Height, 0);

            Quaternion depressionRotation = Quaternion.FromAxisAngle(player.Camera.Right, MathHelper.DegreesToRadians(DepressionDegrees));
            Vector3 lightDir = Vector3.Transform(player.Camera.Front, depressionRotation);

            shader.Use();
            shader.SetUniform("lightPos", lightPos);
            shader.SetUniform("lightDir", lightDir);
            shader.SetUniform("viewPos", player.Camera.Position);
            shader.SetUniform("cutOff", MathF.Cos(MathHelper.DegreesToRadians(CutOffDegrees)));
            shader.SetUniform("outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(OuterCutOffDegrees)));
        }
    }
}
