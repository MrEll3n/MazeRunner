using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ZPG
{
    public class TeleportFadeOverlay
    {
        private float alpha = 0f;
        private float fadeSpeed = 1.5f; // jak rychle fade probíhá
        private bool fadingIn = false;
        private bool fadingOut = false;
        private bool active = false;

        private Action? onFadeInComplete;

        public void StartFade(Action onFadeInDone)
        {
            alpha = 0f;
            fadingIn = true;
            fadingOut = false;
            active = true;
            onFadeInComplete = onFadeInDone;
        }

        public void Update(float dt)
        {
            if (!active) return;

            if (fadingIn)
            {
                alpha += fadeSpeed * dt;
                if (alpha >= 1f)
                {
                    alpha = 1f;
                    fadingIn = false;
                    onFadeInComplete?.Invoke();
                    fadingOut = true;
                }
            }
            else if (fadingOut)
            {
                alpha -= fadeSpeed * dt;
                if (alpha <= 0f)
                {
                    alpha = 0f;
                    fadingOut = false;
                    active = false;
                }
            }
        }

        public void Render(int screenWidth, int screenHeight)
        {
            if (!active || alpha <= 0f) return;

            GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, screenWidth, 0, screenHeight, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Color4(1f, 1f, 1f, alpha);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(screenWidth, 0);
            GL.Vertex2(screenWidth, screenHeight);
            GL.Vertex2(0, screenHeight);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        public bool IsActive => active;
    }
}
