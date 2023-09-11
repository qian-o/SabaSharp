using SabaViewer.Helpers;
using Silk.NET.OpenGLES;

namespace SabaViewer.Shaders;

public class MMDGroundShadowShader : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public int InPos { get; }

    public int UniWVP { get; }

    public int UniShadowColor { get; }

    public MMDGroundShadowShader(GL gl)
    {
        _gl = gl;

        Id = _gl.CreateShaderProgram("Resources/Shader/mmd_ground_shadow.vert", "Resources/Shader/mmd_ground_shadow.frag");
    
        // Attributes
        InPos = _gl.GetAttribLocation(Id, "in_Pos");

        // Uniforms
        UniWVP = _gl.GetUniformLocation(Id, "u_WVP");
        UniShadowColor = _gl.GetUniformLocation(Id, "u_ShadowColor");
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);

        GC.SuppressFinalize(this);
    }
}
