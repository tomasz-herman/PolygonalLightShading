using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PolygonalLightShading
{
    public class Texture : IDisposable
    {
        private int _handle;

        public Texture(string path, params string[] lods)
        {
            _handle = GL.GenTexture();
            Use();
            LoadTexture(path);
            int lvl = 1;
            foreach (var lod in lods)
            {
                LoadTexture(lod, lvl);
                lvl++;
            }
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);

            if(lods.Length == 0) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void LoadTexture(string path, int level = 0)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"PolygonalLightShading.Resources.{path}");
            
            var image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++) {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++) {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }
            
            GL.TexImage2D(TextureTarget.Texture2D, 
                level, 
                PixelInternalFormat.Rgba, 
                image.Width, image.Height, 
                0, 
                PixelFormat.Rgba, 
                PixelType.UnsignedByte, 
                pixels.ToArray());
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void Dispose()
        {
            GL.DeleteTexture(_handle);
            GC.SuppressFinalize(this);
        }
    }
}