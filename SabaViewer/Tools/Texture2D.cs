using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;

namespace SabaViewer.Tools;

public unsafe class Texture2D : IDisposable
{
    private readonly GL _gl;
    private readonly uint _pbo;
    private readonly uint _tex;

    public bool HasAlpha { get; private set; } = true;

    public Texture2D(GL gl, GLEnum wrapParam)
    {
        _gl = gl;

        _pbo = _gl.GenBuffer();
        _tex = _gl.GenTexture();

        _gl.GetFloat((GLEnum)EXT.MaxTextureMaxAnisotropyExt, out float maxAnisotropy);

        _gl.BindTexture(GLEnum.Texture2D, _tex);

        _gl.TexParameter(GLEnum.Texture2D, (GLEnum)EXT.MaxTextureMaxAnisotropyExt, maxAnisotropy);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)wrapParam);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)wrapParam);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureBaseLevel, 0);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMaxLevel, 8);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Enable()
    {
        _gl.BindTexture(GLEnum.Texture2D, _tex);
    }

    public void Disable()
    {
        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void AllocationBuffer(uint pboSize, out void* pboData, bool needRead = false)
    {
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, _pbo);

        _gl.BufferData(GLEnum.PixelUnpackBuffer, pboSize, null, GLEnum.StreamDraw);

        pboData = _gl.MapBufferRange(GLEnum.PixelUnpackBuffer, 0, pboSize, needRead ? (uint)(GLEnum.MapReadBit | GLEnum.MapWriteBit) : (uint)GLEnum.MapWriteBit);

        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void FlushTexture(Vector2D<uint> size, GLEnum internalformat, GLEnum format, GLEnum type, bool hasAlpha = true)
    {
        HasAlpha = hasAlpha;

        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, _pbo);
        _gl.BindTexture(GLEnum.Texture2D, _tex);

        _gl.UnmapBuffer(GLEnum.PixelUnpackBuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)internalformat, size.X, size.Y, 0, format, type, (void*)0);
        _gl.GenerateMipmap(GLEnum.Texture2D);

        _gl.BindTexture(GLEnum.Texture2D, 0);
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_pbo);
        _gl.DeleteTexture(_tex);

        GC.SuppressFinalize(this);
    }
}
