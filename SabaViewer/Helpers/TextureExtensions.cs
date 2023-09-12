using SabaViewer.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using StbImageSharp;
using System.Drawing;

namespace SabaViewer.Helpers;

public static unsafe class TextureExtensions
{
    public static void WriteImage(this Texture2D texture, string file)
    {
        ImageResult image = ImageResult.FromMemory(File.ReadAllBytes(file), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* ptr = image.Data)
        {
            texture.FlushTexture(ptr, new Vector2D<uint>((uint)image.Width, (uint)image.Height), GLEnum.Rgba, GLEnum.UnsignedByte, image.SourceComp == ColorComponents.RedGreenBlueAlpha);
        }
    }

    public static void WriteImage(this Texture2D texture, byte* image, int width, int height)
    {
        texture.FlushTexture(image, new Vector2D<uint>((uint)width, (uint)height), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteFrame(this Texture2D texture, GL gl, int x, int y, int width, int height)
    {
        byte[] bytes = new byte[width * height * 4];

        fixed (byte* ptr = bytes)
        {
            gl.ReadPixels(x, y, (uint)width, (uint)height, GLEnum.Rgba, GLEnum.UnsignedByte, ptr);

            texture.FlushTexture(ptr, new Vector2D<uint>((uint)width, (uint)height), GLEnum.Rgba, GLEnum.UnsignedByte);
        }
    }

    public static void WriteLinearColor(this Texture2D texture, Color[] colors, PointF begin, PointF end)
    {
        byte[] bytes = new byte[1024 * 1024 * 4];

        fixed (byte* ptr = bytes)
        {
            using SKSurface surface = SKSurface.Create(new SKImageInfo(1024, 1024, SKColorType.Rgba8888), (nint)ptr);

            using SKPaint paint = new()
            {
                IsAntialias = true,
                IsDither = true,
                FilterQuality = SKFilterQuality.High,
                Shader = SKShader.CreateLinearGradient(new SKPoint(begin.X * 1024, begin.Y * 1024), new SKPoint(end.X * 1024, end.Y * 1024), colors.Select(c => new SKColor(c.R, c.G, c.B, c.A)).ToArray(), null, SKShaderTileMode.Repeat)
            };
            surface.Canvas.DrawRect(0, 0, 1024, 1024, paint);

            texture.FlushTexture(ptr, new Vector2D<uint>(1024, 1024), GLEnum.Rgba, GLEnum.UnsignedByte);
        }
    }

    public static void WriteMatrixArray(this Texture2D texture, IEnumerable<Matrix4X4<float>> matrices)
    {
        Matrix4X4<float>[] matrixArray = matrices.ToArray();

        byte[] bytes = new byte[matrixArray.Length * 16 * sizeof(float)];

        fixed (byte* ptr = bytes)
        {
            Span<Matrix4X4<float>> span = new(ptr, matrixArray.Length);

            matrixArray.CopyTo(span);

            texture.FlushTexture(ptr, new Vector2D<uint>(4, (uint)matrixArray.Length), GLEnum.Rgba, GLEnum.Float);
        }
    }

    public static void WriteColor(this Texture2D texture, Color color)
    {
        WriteColor(texture, new Vector4D<byte>(color.R, color.G, color.B, color.A));
    }

    public static void WriteColor(this Texture2D texture, Vector3D<byte> color)
    {
        WriteColor(texture, new Vector4D<byte>(color, 255));
    }

    public static void WriteColor(this Texture2D texture, Vector4D<byte> color)
    {
        byte[] bytes = new byte[4];

        fixed (byte* ptr = bytes)
        {
            Span<Vector4D<byte>> span = new(ptr, 1);

            span[0] = color;

            texture.FlushTexture(ptr, new Vector2D<uint>(1, 1), GLEnum.Rgba, GLEnum.UnsignedByte);
        }
    }
}
