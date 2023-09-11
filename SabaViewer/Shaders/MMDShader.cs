using SabaViewer.Helpers;
using Silk.NET.OpenGLES;

namespace SabaViewer.Shaders;

public class MMDShader : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public uint InPos { get; }

    public uint InNor { get; }

    public uint InUV { get; }

    public int UniWV { get; }

    public int UniWVP { get; }

    public int UniLightWVP { get; }

    public int UniAlpha { get; }

    public int UniDiffuse { get; }

    public int UniAmbinet { get; }

    public int UniSpecular { get; }

    public int UniSpecularPower { get; }

    public int UniLightColor { get; }

    public int UniLightDir { get; }

    public int UniTexMode { get; }

    public int UniTex { get; }

    public int UniTexMulFactor { get; }

    public int UniTexAddFactor { get; }

    public int UniToonTexMode { get; }

    public int UniToonTex { get; }

    public int UniToonTexMulFactor { get; }

    public int UniToonTexAddFactor { get; }

    public int UniSphereTexMode { get; }

    public int UniSphereTex { get; }

    public int UniSphereTexMulFactor { get; }

    public int UniSphereTexAddFactor { get; }

    public int UniShadowMapSplitPositions { get; }

    public int UniShadowMap0 { get; }

    public int UniShadowMap1 { get; }

    public int UniShadowMap2 { get; }

    public int UniShadowMap3 { get; }

    public int UniShadowMapEnabled { get; }

    public MMDShader(GL gl)
    {
        _gl = gl;

        Id = _gl.CreateShaderProgram("Resources/Shader/mmd.vert", "Resources/Shader/mmd.frag");

        // Attributes
        InPos = (uint)_gl.GetAttribLocation(Id, "in_Pos");
        InNor = (uint)_gl.GetAttribLocation(Id, "in_Nor");
        InUV = (uint)_gl.GetAttribLocation(Id, "in_UV");

        // Uniforms
        UniWV = _gl.GetUniformLocation(Id, "u_WV");
        UniWVP = _gl.GetUniformLocation(Id, "u_WVP");
        UniLightWVP = _gl.GetUniformLocation(Id, "u_LightWVP");

        UniAlpha = _gl.GetUniformLocation(Id, "u_Alpha");
        UniDiffuse = _gl.GetUniformLocation(Id, "u_Diffuse");
        UniAmbinet = _gl.GetUniformLocation(Id, "u_Ambient");
        UniSpecular = _gl.GetUniformLocation(Id, "u_Specular");
        UniSpecularPower = _gl.GetUniformLocation(Id, "u_SpecularPower");
        UniLightColor = _gl.GetUniformLocation(Id, "u_LightColor");
        UniLightDir = _gl.GetUniformLocation(Id, "u_LightDir");

        UniTexMode = _gl.GetUniformLocation(Id, "u_TexMode");
        UniTex = _gl.GetUniformLocation(Id, "u_Tex");
        UniTexMulFactor = _gl.GetUniformLocation(Id, "u_TexMulFactor");
        UniTexAddFactor = _gl.GetUniformLocation(Id, "u_TexAddFactor");

        UniToonTexMode = _gl.GetUniformLocation(Id, "u_ToonTexMode");
        UniToonTex = _gl.GetUniformLocation(Id, "u_ToonTex");
        UniToonTexMulFactor = _gl.GetUniformLocation(Id, "u_ToonTexMulFactor");
        UniToonTexAddFactor = _gl.GetUniformLocation(Id, "u_ToonTexAddFactor");

        UniSphereTexMode = _gl.GetUniformLocation(Id, "u_SphereTexMode");
        UniSphereTex = _gl.GetUniformLocation(Id, "u_SphereTex");
        UniSphereTexMulFactor = _gl.GetUniformLocation(Id, "u_SphereTexMulFactor");
        UniSphereTexAddFactor = _gl.GetUniformLocation(Id, "u_SphereTexAddFactor");

        UniShadowMapSplitPositions = _gl.GetUniformLocation(Id, "u_ShadowMapSplitPositions");
        UniShadowMap0 = _gl.GetUniformLocation(Id, "u_ShadowMap0");
        UniShadowMap1 = _gl.GetUniformLocation(Id, "u_ShadowMap1");
        UniShadowMap2 = _gl.GetUniformLocation(Id, "u_ShadowMap2");
        UniShadowMap3 = _gl.GetUniformLocation(Id, "u_ShadowMap3");
        UniShadowMapEnabled = _gl.GetUniformLocation(Id, "u_ShadowMapEnabled");
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);

        GC.SuppressFinalize(this);
    }
}
