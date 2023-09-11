using SabaViewer.Helpers;
using Silk.NET.OpenGLES;

namespace SabaViewer.Shaders;

public class MMDEdgeShader : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public uint InPos { get; }

    public uint InNor { get; }

    public int UniWV { get; }

    public int UniWVP { get; }

    public int UniScreenSize { get; }

    public int UniEdgeSize { get; }

    public int UniEdgeColor { get; }

    public MMDEdgeShader(GL gl)
    {
        _gl = gl;

        Id = _gl.CreateShaderProgram("Resources/Shader/mmd_edge.vert", "Resources/Shader/mmd_edge.frag");

        // Attributes
        InPos = (uint)_gl.GetAttribLocation(Id, "in_Pos");
        InNor = (uint)_gl.GetAttribLocation(Id, "in_Nor");

        // Uniforms
        UniWV = _gl.GetUniformLocation(Id, "u_WV");
        UniWVP = _gl.GetUniformLocation(Id, "u_WVP");
        UniScreenSize = _gl.GetUniformLocation(Id, "u_ScreenSize");
        UniEdgeSize = _gl.GetUniformLocation(Id, "u_EdgeSize");
        UniEdgeColor = _gl.GetUniformLocation(Id, "u_EdgeColor");
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);

        GC.SuppressFinalize(this);
    }
}
