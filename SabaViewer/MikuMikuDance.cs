using Saba;
using SabaViewer.Helpers;
using SabaViewer.Shaders;
using SabaViewer.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace SabaViewer;

public unsafe class MikuMikuDance : IDisposable
{
    private readonly GL _gl;
    private readonly MMDShader _mmdShader;
    private readonly MMDEdgeShader _mmdEdgeShader;
    private readonly MMDGroundShadowShader _mmdGroundShadowShader;
    private readonly List<Material> _materials;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly Texture2D _defaultTexture;

    private MMDModel? model;

    private uint posVBO;
    private uint norVBO;
    private uint uvVBO;

    private uint ibo;

    private uint mmdVAO;
    private uint mmdEdgeVAO;
    private uint mmdGroundShadowVAO;

    public static Vector3D<float> LightColor { get; set; } = new(1.0f, 1.0f, 1.0f);

    public static Vector3D<float> LightDir { get; set; } = new(-0.5f, -1.0f, -0.5f);

    public MikuMikuDance(GL gl)
    {
        _gl = gl;
        _mmdShader = new(gl);
        _mmdEdgeShader = new(gl);
        _mmdGroundShadowShader = new(gl);
        _materials = new();
        _textures = new();
        _defaultTexture = new Texture2D(_gl);
        _defaultTexture.WriteColor(new Vector3D<byte>(255));
    }

    public void LoadModel(string path)
    {
        model = new PmxModel();
        model.Load(path, "Resources/MMD/");
    }

    public void Setup()
    {
        if (model is null)
        {
            return;
        }

        // Setup vertices
        int vtxCount = model.GetVertexCount();

        posVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector3D<float>) * vtxCount), null, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        norVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector3D<float>) * vtxCount), null, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        uvVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector2D<float>) * vtxCount), null, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        // Setup indices
        int idxCount = model.GetIndexCount();

        ibo = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);
        _gl.BufferData(GLEnum.ElementArrayBuffer, (uint)(sizeof(uint) * idxCount), model.GetIndices(), GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

        // Setup MMD VAO
        mmdVAO = _gl.GenVertexArray();
        _gl.BindVertexArray(mmdVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.VertexAttribPointer(_mmdShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InPos);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.VertexAttribPointer(_mmdShader.InNor, 3, GLEnum.Float, false, (uint)sizeof(Vector3D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InNor);

        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.VertexAttribPointer(_mmdShader.InUV, 2, GLEnum.Float, false, (uint)sizeof(Vector2D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InUV);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup MMD Edge VAO
        mmdEdgeVAO = _gl.GenVertexArray();
        _gl.BindVertexArray(mmdEdgeVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.VertexAttribPointer(_mmdEdgeShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdEdgeShader.InPos);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.VertexAttribPointer(_mmdEdgeShader.InNor, 3, GLEnum.Float, false, (uint)sizeof(Vector3D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdEdgeShader.InNor);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup MMD Ground Shadow VAO
        mmdGroundShadowVAO = _gl.GenVertexArray();
        _gl.BindVertexArray(mmdGroundShadowVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.VertexAttribPointer(_mmdGroundShadowShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3D<float>), (void*)0);
        _gl.EnableVertexAttribArray(_mmdGroundShadowShader.InPos);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup materials
        foreach (MMDMaterial mmdMat in model.GetMaterials())
        {
            Material mat = new(mmdMat);

            if (!string.IsNullOrEmpty(mmdMat.Texture))
            {
                mat.Texture = GetTexture(mmdMat.Texture);
            }

            if (!string.IsNullOrEmpty(mmdMat.SpTexture))
            {
                mat.SpTexture = GetTexture(mmdMat.SpTexture);
            }

            if (!string.IsNullOrEmpty(mmdMat.ToonTexture))
            {
                mat.ToonTexture = GetTexture(mmdMat.ToonTexture);
            }

            _materials.Add(mat);
        }
    }

    public void Update()
    {
        if (model is null)
        {
            return;
        }

        // Update vertices
        int vtxCount = model.GetVertexCount();

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector3D<float>) * vtxCount), model.GetPositions());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector3D<float>) * vtxCount), model.GetNormals());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector2D<float>) * vtxCount), model.GetUVs());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public void Draw(Camera camera, int screenWidth, int screenHeight)
    {
        if (model is null)
        {
            return;
        }

        _gl.Enable(GLEnum.DepthTest);

        foreach (MMDMesh mesh in model.GetMeshes())
        {
            MMDMaterial mmdMat = mesh.Material;
            Material mat = _materials.Find(item => item.MMDMaterial == mmdMat)!;

            if (mmdMat.Alpha == 0)
            {
                continue;
            }

            _gl.UseProgram(_mmdShader.Id);
            _gl.BindVertexArray(mmdVAO);

            _gl.SetUniform(_mmdShader.UniWVP, camera.View * camera.Projection);
            _gl.SetUniform(_mmdShader.UniWV, camera.View);

            _gl.SetUniform(_mmdShader.UniAmbinet, mmdMat.Ambient);
            _gl.SetUniform(_mmdShader.UniDiffuse, mmdMat.Diffuse);
            _gl.SetUniform(_mmdShader.UniSpecular, mmdMat.Specular);
            _gl.SetUniform(_mmdShader.UniSpecularPower, mmdMat.SpecularPower);
            _gl.SetUniform(_mmdShader.UniAlpha, mmdMat.Alpha);

            _gl.ActiveTexture(TextureUnit.Texture0 + 0);
            _gl.SetUniform(_mmdShader.UniTex, 0);
            if (mat.Texture != null)
            {
                if (!mat.Texture.HasAlpha)
                {
                    _gl.SetUniform(_mmdShader.UniTexMode, 1);
                }
                else
                {
                    _gl.SetUniform(_mmdShader.UniTexMode, 2);
                }

                _gl.SetUniform(_mmdShader.UniTexMulFactor, mmdMat.TextureMulFactor);
                _gl.SetUniform(_mmdShader.UniTexAddFactor, mmdMat.TextureAddFactor);

                mat.Texture.Enable();
            }
            else
            {
                _gl.SetUniform(_mmdShader.UniTexMode, 0);

                _defaultTexture.Enable();
            }

            _gl.ActiveTexture(TextureUnit.Texture0 + 1);
            _gl.SetUniform(_mmdShader.UniSphereTex, 1);
            if (mat.SpTexture != null)
            {
                if (mmdMat.SpTextureMode == SphereTextureMode.Mul)
                {
                    _gl.SetUniform(_mmdShader.UniSphereTexMode, 1);
                }
                else if (mmdMat.SpTextureMode == SphereTextureMode.Add)
                {
                    _gl.SetUniform(_mmdShader.UniSphereTexMode, 2);
                }

                _gl.SetUniform(_mmdShader.UniSphereTexMulFactor, mmdMat.SpTextureMulFactor);
                _gl.SetUniform(_mmdShader.UniSphereTexAddFactor, mmdMat.SpTextureAddFactor);

                mat.SpTexture.Enable();
            }
            else
            {
                _gl.SetUniform(_mmdShader.UniSphereTexMode, 0);

                _defaultTexture.Enable();
            }

            _gl.ActiveTexture(TextureUnit.Texture0 + 2);
            _gl.SetUniform(_mmdShader.UniToonTex, 2);
            if (mat.ToonTexture != null)
            {
                _gl.SetUniform(_mmdShader.UniToonTexMulFactor, mmdMat.ToonTextureMulFactor);
                _gl.SetUniform(_mmdShader.UniToonTexAddFactor, mmdMat.ToonTextureAddFactor);
                _gl.SetUniform(_mmdShader.UniToonTexMode, 1);

                mat.ToonTexture.Enable();
                _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
                _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
            }
            else
            {
                _gl.SetUniform(_mmdShader.UniToonTexMode, 0);

                _defaultTexture.Enable();
            }

            _gl.SetUniform(_mmdShader.UniLightColor, LightColor);
            _gl.SetUniform(_mmdShader.UniLightDir, Vector3D.Transform(LightDir, camera.View));

            if (mmdMat.BothFace)
            {
                _gl.Disable(GLEnum.CullFace);
            }
            else
            {
                _gl.Enable(GLEnum.CullFace);
                _gl.CullFace(GLEnum.Back);
            }

            _gl.Enable(GLEnum.Blend);
            _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            _gl.SetUniform(_mmdShader.UniShadowMapEnabled, 0);
            _gl.SetUniform(_mmdShader.UniShadowMap0, 3);
            _gl.SetUniform(_mmdShader.UniShadowMap1, 4);
            _gl.SetUniform(_mmdShader.UniShadowMap2, 5);
            _gl.SetUniform(_mmdShader.UniShadowMap3, 6);

            _gl.DrawElements(GLEnum.Triangles, mesh.VertexCount, GLEnum.UnsignedInt, (void*)(mesh.BeginIndex * sizeof(uint)));

            _gl.ActiveTexture(TextureUnit.Texture0 + 0);
            _gl.BindTexture(GLEnum.Texture2D, 0);

            _gl.ActiveTexture(TextureUnit.Texture0 + 1);
            _gl.BindTexture(GLEnum.Texture2D, 0);

            _gl.ActiveTexture(TextureUnit.Texture0 + 2);
            _gl.BindTexture(GLEnum.Texture2D, 0);

            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }
    }

    public void Dispose()
    {
        foreach (Material material in _materials)
        {
            material.Texture?.Dispose();
            material.SpTexture?.Dispose();
            material.ToonTexture?.Dispose();
        }
        _materials.Clear();

        _gl.DeleteBuffer(posVBO);
        _gl.DeleteBuffer(norVBO);
        _gl.DeleteBuffer(uvVBO);
        _gl.DeleteBuffer(ibo);

        _gl.DeleteVertexArray(mmdVAO);
        _gl.DeleteVertexArray(mmdEdgeVAO);
        _gl.DeleteVertexArray(mmdGroundShadowVAO);

        _mmdShader.Dispose();
        _mmdEdgeShader.Dispose();
        _mmdGroundShadowShader.Dispose();

        GC.SuppressFinalize(this);
    }

    private Texture2D GetTexture(string texturePath)
    {
        if (!_textures.TryGetValue(texturePath, out Texture2D? texture))
        {
            texture = new Texture2D(_gl);
            texture.WriteImage(texturePath);

            _textures.Add(texturePath, texture);
        }

        return texture;
    }
}
