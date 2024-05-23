using System.Drawing;
using System.Numerics;
using Saba;
using Saba.Helpers;
using SabaViewer.Helpers;
using SabaViewer.Shaders;
using SabaViewer.Tools;
using Silk.NET.OpenGLES;

namespace SabaViewer;

public unsafe class MikuMikuDance : IDisposable
{
    private readonly GL _gl;
    private readonly MMDShader _mmdShader;
    private readonly MMDEdgeShader _mmdEdgeShader;
    private readonly MMDGroundShadowShader _mmdGroundShadowShader;
    private readonly Dictionary<MMDMaterial, Material> _materials;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly Texture2D _defaultTexture;

    private MMDModel? model;
    private VmdAnimation? animation;

    private uint posVBO;
    private uint norVBO;
    private uint uvVBO;

    private uint ibo;

    private uint mmdVAO;
    private uint mmdEdgeVAO;
    private uint mmdGroundShadowVAO;

    private float saveTime = 0.0f;
    private float animTime = 0.0f;
    private float elapsed = 0.0f;

    public static Vector3 LightColor { get; set; } = new(1.0f, 1.0f, 1.0f);

    public static Vector4 ShadowColor { get; set; } = new(0.17f, 0.17f, 0.17f, 0.7f);

    public static Vector3 LightDir { get; set; } = new(-0.5f, -1.0f, -0.5f);

    public Vector3 Translate { get; set; } = Vector3.Zero;

    public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix4x4 Transform => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Translate);

    public bool IsPlaying { get; set; } = false;

    public bool EnablePhysical { get; set; } = true;

    public bool EnableShadow { get; set; } = true;

    public MikuMikuDance(GL gl, string modelPath, string? vmdPath = null)
    {
        _gl = gl;
        _mmdShader = new(gl);
        _mmdEdgeShader = new(gl);
        _mmdGroundShadowShader = new(gl);
        _materials = [];
        _textures = [];
        _defaultTexture = new Texture2D(_gl);
        _defaultTexture.WriteColor(Color.White);

        LoadModel(modelPath, vmdPath);
        Setup();
    }

    public void Update(float time)
    {
        // compute elapsed time
        {
            float elapsedTime = time - saveTime;

            if (elapsedTime > 1.0f / 30.0f)
            {
                elapsedTime = 1.0f / 30.0f;
            }

            if (IsPlaying)
            {
                animTime += elapsedTime;
            }

            if (EnablePhysical)
            {
                elapsed = elapsedTime;
            }
            else
            {
                elapsed = 0.0f;
            }

            saveTime = time;
        }

        if (model is null)
        {
            return;
        }

        if (animation is not null)
        {
            model.BeginAnimation();
            model.UpdateAllAnimation(animation, animTime * 30.0f, elapsed);
            model.EndAnimation();

            model.Update();
        }

        // Update vertices
        int vtxCount = model.GetVertexCount();

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector3) * vtxCount), model.GetUpdatePositions());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector3) * vtxCount), model.GetUpdateNormals());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(sizeof(Vector2) * vtxCount), model.GetUpdateUVs());
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public void Draw(Camera camera, int screenWidth, int screenHeight)
    {
        if (model is null)
        {
            return;
        }

        Matrix4x4 transform = Transform;

        _gl.Enable(GLEnum.DepthTest);

        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        MMDMesh[] meshes = model.GetMeshes();

        // Draw model
        {
            _gl.UseProgram(_mmdShader.Id);
            _gl.BindVertexArray(mmdVAO);

            _gl.SetUniform(_mmdShader.UniWVP, transform * camera.View * camera.Projection);
            _gl.SetUniform(_mmdShader.UniWV, transform * camera.View);

            foreach (MMDMesh mesh in meshes)
            {
                MMDMaterial mmdMat = mesh.Material;
                Material mat = _materials[mmdMat];

                if (mmdMat.Alpha == 0.0f)
                {
                    continue;
                }
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

                    _gl.BindTexture(GLEnum.Texture2D, mat.Texture.Id);
                }
                else
                {
                    _gl.SetUniform(_mmdShader.UniTexMode, 0);
                    _gl.BindTexture(GLEnum.Texture2D, _defaultTexture.Id);
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

                    _gl.BindTexture(GLEnum.Texture2D, mat.SpTexture.Id);
                }
                else
                {
                    _gl.SetUniform(_mmdShader.UniSphereTexMode, 0);
                    _gl.BindTexture(GLEnum.Texture2D, _defaultTexture.Id);
                }

                _gl.ActiveTexture(TextureUnit.Texture0 + 2);
                _gl.SetUniform(_mmdShader.UniToonTex, 2);
                if (mat.ToonTexture != null)
                {
                    _gl.SetUniform(_mmdShader.UniToonTexMulFactor, mmdMat.ToonTextureMulFactor);
                    _gl.SetUniform(_mmdShader.UniToonTexAddFactor, mmdMat.ToonTextureAddFactor);
                    _gl.SetUniform(_mmdShader.UniToonTexMode, 1);

                    _gl.BindTexture(GLEnum.Texture2D, mat.ToonTexture.Id);
                    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
                    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
                }
                else
                {
                    _gl.SetUniform(_mmdShader.UniToonTexMode, 0);
                    _gl.BindTexture(GLEnum.Texture2D, _defaultTexture.Id);
                }

                _gl.SetUniform(_mmdShader.UniLightColor, LightColor);
                _gl.SetUniform(_mmdShader.UniLightDir, LightDir);

                if (mmdMat.BothFace)
                {
                    _gl.Disable(GLEnum.CullFace);
                }
                else
                {
                    _gl.Enable(GLEnum.CullFace);
                    _gl.CullFace(GLEnum.Back);
                }

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
            }

            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }

        // Draw edge
        {
            _gl.Enable(GLEnum.CullFace);
            _gl.CullFace(GLEnum.Front);

            _gl.UseProgram(_mmdEdgeShader.Id);
            _gl.BindVertexArray(mmdEdgeVAO);

            _gl.SetUniform(_mmdEdgeShader.UniWVP, transform * camera.View * camera.Projection);
            _gl.SetUniform(_mmdEdgeShader.UniWV, transform * camera.View);
            _gl.SetUniform(_mmdEdgeShader.UniScreenSize, new Vector2(screenWidth, screenHeight));

            foreach (MMDMesh mesh in meshes)
            {
                MMDMaterial mmdMat = mesh.Material;

                if (mmdMat.EdgeFlag == 0)
                {
                    continue;
                }

                if (mmdMat.Alpha == 0.0f)
                {
                    continue;
                }

                _gl.SetUniform(_mmdEdgeShader.UniEdgeSize, mmdMat.EdgeSize);
                _gl.SetUniform(_mmdEdgeShader.UniEdgeColor, mmdMat.EdgeColor);

                _gl.DrawElements(GLEnum.Triangles, mesh.VertexCount, GLEnum.UnsignedInt, (void*)(mesh.BeginIndex * sizeof(uint)));
            }

            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }

        // Draw ground shadow
        {
            if (EnableShadow)
            {
                _gl.Enable(GLEnum.PolygonOffsetFill);
                _gl.PolygonOffset(-1.0f, -1.0f);

                Matrix4x4 shadow = Matrix4x4.CreateShadow(-LightDir, new Plane(0.0f, 1.0f, 0.0f, 0.0f));
                Matrix4x4 shadowMatrix = transform * shadow * camera.View * camera.Projection;

                if (ShadowColor.W < 1.0f)
                {
                    _gl.Enable(GLEnum.Blend);
                    _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

                    _gl.Enable(GLEnum.StencilTest);
                    _gl.StencilFuncSeparate(GLEnum.FrontAndBack, GLEnum.Notequal, 1, 1);
                    _gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);
                }
                else
                {
                    _gl.Disable(GLEnum.Blend);
                }

                _gl.Disable(GLEnum.CullFace);

                _gl.UseProgram(_mmdGroundShadowShader.Id);
                _gl.BindVertexArray(mmdGroundShadowVAO);

                _gl.SetUniform(_mmdGroundShadowShader.UniWVP, shadowMatrix);
                _gl.SetUniform(_mmdGroundShadowShader.UniShadowColor, ShadowColor);

                foreach (MMDMesh mesh in meshes)
                {
                    MMDMaterial mmdMat = mesh.Material;

                    if (!mmdMat.GroundShadow)
                    {
                        continue;
                    }

                    if (mmdMat.Alpha == 0.0f)
                    {
                        continue;
                    }

                    _gl.DrawElements(GLEnum.Triangles, mesh.VertexCount, GLEnum.UnsignedInt, (void*)(mesh.BeginIndex * sizeof(uint)));
                }

                _gl.BindVertexArray(0);
                _gl.UseProgram(0);
            }
        }

        _gl.Disable(GLEnum.StencilTest);
        _gl.Disable(GLEnum.PolygonOffsetFill);
        _gl.Disable(GLEnum.Blend);
        _gl.Disable(GLEnum.DepthTest);
    }

    public void Dispose()
    {
        foreach (KeyValuePair<MMDMaterial, Material> pair in _materials)
        {
            pair.Value.Texture?.Dispose();
            pair.Value.SpTexture?.Dispose();
            pair.Value.ToonTexture?.Dispose();
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

    private void LoadModel(string modelPath, string? vmdPath = null)
    {
        model = new PmxModel();
        model.Load(modelPath, "Resources/MMD/".FormatFilePath());

        model.InitializeAnimation();

        if (!string.IsNullOrEmpty(vmdPath))
        {
            animation = new VmdAnimation();
            animation.Load(vmdPath, model);

            animation.SyncPhysics(0.0f);
        }

        model.Update();
    }

    private void Setup()
    {
        if (model is null)
        {
            return;
        }

        // Setup vertices
        int vtxCount = model.GetVertexCount();

        posVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector3) * vtxCount), null, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        norVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector3) * vtxCount), null, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        uvVBO = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(sizeof(Vector2) * vtxCount), null, GLEnum.DynamicDraw);
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
        _gl.VertexAttribPointer(_mmdShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InPos);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.VertexAttribPointer(_mmdShader.InNor, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InNor);

        _gl.BindBuffer(GLEnum.ArrayBuffer, uvVBO);
        _gl.VertexAttribPointer(_mmdShader.InUV, 2, GLEnum.Float, false, (uint)sizeof(Vector2), (void*)0);
        _gl.EnableVertexAttribArray(_mmdShader.InUV);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup MMD Edge VAO
        mmdEdgeVAO = _gl.GenVertexArray();
        _gl.BindVertexArray(mmdEdgeVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.VertexAttribPointer(_mmdEdgeShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(_mmdEdgeShader.InPos);

        _gl.BindBuffer(GLEnum.ArrayBuffer, norVBO);
        _gl.VertexAttribPointer(_mmdEdgeShader.InNor, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(_mmdEdgeShader.InNor);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup MMD Ground Shadow VAO
        mmdGroundShadowVAO = _gl.GenVertexArray();
        _gl.BindVertexArray(mmdGroundShadowVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, posVBO);
        _gl.VertexAttribPointer(_mmdGroundShadowShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(_mmdGroundShadowShader.InPos);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);

        _gl.BindVertexArray(0);

        // Setup materials
        foreach (MMDMaterial mmdMat in model.GetMaterials())
        {
            Material mat = new();

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

            _materials.Add(mmdMat, mat);
        }
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
