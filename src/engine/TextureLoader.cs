using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace ZPG
{
    /// <summary>
    /// Poskytuje metodu pro načítání textur z obrázkových souborů a jejich nahrání do OpenGL.
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// Načte 2D texturu ze souboru a nahraje ji do OpenGL. Vrací ID vytvořené textury.
        /// </summary>
        /// <param name="path">Cesta k obrázkovému souboru (např. PNG, JPG).</param>
        /// <returns>ID textury nebo 0 při chybě.</returns>
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
