using Silk.NET.OpenGLES;

namespace SabaViewer.Tools;

public unsafe class Frame : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public uint Framebuffer { get; }

    public uint DepthRenderBuffer { get; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public Frame(GL gl)
    {
        _gl = gl;

        Id = _gl.GenFramebuffer();
        Framebuffer = _gl.GenTexture();
        DepthRenderBuffer = _gl.GenRenderbuffer();

        _gl.BindTexture(GLEnum.Texture2D, Framebuffer);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Create(int width, int height)
    {
        if (Width == width && Height == height)
        {
            return;
        }

        Width = width;
        Height = height;

        _gl.BindTexture(GLEnum.Texture2D, Framebuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)Width, (uint)Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, (uint)Width, (uint)Height);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, Id);
        _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, Framebuffer, 0);
        _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(Id);
        _gl.DeleteTexture(Framebuffer);
        _gl.DeleteRenderbuffer(DepthRenderBuffer);

        GC.SuppressFinalize(this);
    }
}
