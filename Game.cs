using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MrEllen.ZPG 
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() 
        { 
            Size = (width, height), Title = title 
        }) 
        {

        }
    

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            if (KeyboardState.IsKeyDown(Keys.Escape)) 
            {
                Close();
            }
        }
    }
}