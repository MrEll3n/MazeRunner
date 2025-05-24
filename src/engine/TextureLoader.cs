using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace ZPG
{
    /// <summary>
    /// Provides a method for loading image textures from files and uploading them to OpenGL.
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// Loads a 2D texture from an image file and uploads it to OpenGL. Returns the generated texture ID.
        /// </summary>
        /// <param name="path">Path to the image file (e.g., PNG, JPG).</param>
        /// <returns>The texture ID or 0 if loading failed.</returns>
        public static int LoadTexture(string path)
        {
            // Kontrola existence souboru
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: Texture file not found: {path}");
                return 0;
            }

            // Vytvoření a aktivace nové OpenGL textury
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);

            // Načtení obrázku a jeho převrácení vertikálně (OpenGL očekává spodní řádek jako první)
            using var img = Image.Load<Rgba32>(path);
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            // Převod pixelů na pole bajtů (RGBA, 4 bajty na pixel)
            var pixels = new byte[img.Width * img.Height * 4];
            img.CopyPixelDataTo(pixels);

            // Nahrání pixelů do GPU
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // Nastavení způsobu opakování textury (wrap)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Nastavení filtrace a mipmap
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
