using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL;

namespace ZPG
{
    public static class TextureLoader
    {
        public static int LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: Texture file not found: {path}");
                return 0;
            }

            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);

            using var img = Image.Load<Rgba32>(path);
            img.Mutate(x => x.Flip(FlipMode.Vertical));
            var pixels = new byte[img.Width * img.Height * 4];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // Wrap opakování
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Filtry a mipmapy
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
