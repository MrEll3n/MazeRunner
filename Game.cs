using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MrEllen.ZPG 
{
    public class Game : GameWindow
    {
        int VertexBufferObject;
        float[] vertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };

        Shader shader;

        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() 
        { 
            Size = (width, height), Title = title 
        }) 
        {

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Create a new Vertex Buffer Object (VBO) and bind it
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Load the shader
            shader = new Shader("shader.vert", "shader.frag");

            // Create a new Vertex Array Object (VAO) and bind it
            int VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            // Tell OpenGL how to interpret the vertex data
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Code goes here.

            SwapBuffers();
        }
    
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            if (KeyboardState.IsKeyDown(Keys.Escape)) 
            {
                Close();
            }
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferObject);
            shader.Dispose();
        }
    }
}