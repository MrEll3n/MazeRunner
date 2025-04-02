using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; // pro .Mutate()
using OpenTK.Graphics.OpenGL;

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
        img.Mutate(x => x.Flip(FlipMode.Vertical)); // OpenGL má opačný Y
        var pixels = new byte[img.Width * img.Height * 4];
        img.CopyPixelDataTo(pixels);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        // Filtr
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // ✅ ZDE přidat wrap (opakování)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return texID;
    }
}
