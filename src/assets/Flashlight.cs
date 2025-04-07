using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Reprezentuje baterku připojenou k hráči. Simuluje kuželové světlo se směrovým vektorem a úhlem osvitu.
    /// </summary>
    public class Flashlight
    {
        /// <summary>
        /// Výška baterky nad pozicí hráče (v metrech).
        /// </summary>
        public float Height { get; set; } = 2.05f;

        /// <summary>
        /// Úhel deprese (naklonění světla dolů) ve stupních.
        /// </summary>
        public float DepressionDegrees { get; set; } = 2.0f;

        /// <summary>
        /// Vnitřní úhel kuželu světla (cut-off), po kterém začíná útlum.
        /// </summary>
        public float CutOffDegrees { get; set; } = 20f;

        /// <summary>
        /// Vnější úhel kuželu světla (outer cut-off), za kterým světlo zcela končí.
        /// </summary>
        public float OuterCutOffDegrees { get; set; } = 30f;

        private Player player;

        /// <summary>
        /// Inicializuje novou instanci třídy <see cref="Flashlight"/> pro daného hráče.
        /// </summary>
        /// <param name="player">Instance hráče, ke kterému je baterka připojena.</param>
        public Flashlight(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Aplikuje parametry baterky do daného shaderu.
        /// Nastavuje pozici světla, směr osvětlení, pohledovou pozici a cut-off úhly.
        /// </summary>
        /// <param name="shader">Shader, do kterého budou parametry zapsány.</param>
        public void Apply(Shader shader)
        {
            // Výpočet světelné pozice: výška nad pozicí hráče
            Vector3 lightPos = player.Position + new Vector3(0, Height, 0);

            // Výpočet směru světla s aplikací úhlu deprese (otočení směrem dolů kolem pravé osy kamery)
            Quaternion depressionRotation = Quaternion.FromAxisAngle(player.Camera.Right, MathHelper.DegreesToRadians(DepressionDegrees));
            Vector3 lightDir = Vector3.Transform(player.Camera.Front, depressionRotation);

            // Použití shaderu a nastavení uniformních proměnných pro světlo
            shader.Use();
            shader.SetUniform("lightPos", lightPos);
            shader.SetUniform("lightDir", lightDir);
            shader.SetUniform("viewPos", player.Camera.Position);
            shader.SetUniform("cutOff", MathF.Cos(MathHelper.DegreesToRadians(CutOffDegrees)));
            shader.SetUniform("outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(OuterCutOffDegrees)));
        }
    }
}
