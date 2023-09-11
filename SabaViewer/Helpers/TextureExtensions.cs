using SabaViewer.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using System.Drawing;

namespace SabaViewer.Helpers;

public static unsafe class TextureExtensions
{
    public static void WriteImage(this Texture2D texture, string file)
    {
        using SKImage image = SKImage.FromEncodedData(file);

        int comp = 4;
        GLEnum color = GLEnum.Rgba;
        if (image.ColorType != SKColorType.Rgba8888)
        {
            comp = 3;
            color = GLEnum.Rgb;
        }

        texture.AllocationBuffer((uint)(image.Width * image.Height * comp), out void* pboData);

        image.ReadPixels(new SKImageInfo(image.Width, image.Height, image.ColorType), (nint)pboData, image.Width * comp, 0, 0);

        texture.FlushTexture(new Vector2D<uint>((uint)image.Width, (uint)image.Height), color, GLEnum.UnsignedByte);
    }

    public static void WriteImage(this Texture2D texture, byte* image, int width, int height, int comp = 4)
    {
        texture.AllocationBuffer((uint)(width * height * comp), out void* pboData);

        Span<byte> src = new(image, width * height * comp);
        Span<byte> dst = new(pboData, width * height * comp);

        src.CopyTo(dst);

        texture.FlushTexture(new Vector2D<uint>((uint)width, (uint)height), comp == 4 ? GLEnum.Rgba : GLEnum.Rgb, GLEnum.UnsignedByte);
    }

    public static void WriteFrame(this Texture2D texture, GL gl, int x, int y, int width, int height)
    {
        texture.AllocationBuffer((uint)(width * height * 4), out void* pboData);

        gl.ReadPixels(x, y, (uint)width, (uint)height, GLEnum.Rgba, GLEnum.UnsignedByte, pboData);

        texture.FlushTexture(new Vector2D<uint>((uint)width, (uint)height), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteLinearColor(this Texture2D texture, Color[] colors, PointF begin, PointF end)
    {
        texture.AllocationBuffer(1024 * 1024 * 4, out void* pboData);

        using SKSurface surface = SKSurface.Create(new SKImageInfo(1024, 1024, SKColorType.Rgba8888), (nint)pboData);

        using SKPaint paint = new()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High,
            Shader = SKShader.CreateLinearGradient(new SKPoint(begin.X * 1024, begin.Y * 1024), new SKPoint(end.X * 1024, end.Y * 1024), colors.Select(c => new SKColor(c.R, c.G, c.B, c.A)).ToArray(), null, SKShaderTileMode.Repeat)
        };
        surface.Canvas.DrawRect(0, 0, 1024, 1024, paint);

        texture.FlushTexture(new Vector2D<uint>(1024, 1024), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteMatrixArray(this Texture2D texture, IEnumerable<Matrix4X4<float>> matrices)
    {
        Matrix4X4<float>[] matrixArray = matrices.ToArray();

        texture.AllocationBuffer((uint)(matrixArray.Length * 16 * sizeof(float)), out void* pboData);

        Span<Matrix4X4<float>> span = new(pboData, matrixArray.Length);

        matrixArray.CopyTo(span);

        texture.FlushTexture(new Vector2D<uint>(4, (uint)matrixArray.Length), GLEnum.Rgba, GLEnum.Float);
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
        texture.AllocationBuffer(4, out void* pboData);

        Span<Vector4D<byte>> span = new(pboData, 1);

        span[0] = color;

        texture.FlushTexture(new Vector2D<uint>(1, 1), GLEnum.Rgba, GLEnum.UnsignedByte);
    }
}
