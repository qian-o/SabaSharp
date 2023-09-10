using Saba.Contracts;
using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class PmxModel : IModel
{
    private readonly List<Vector3D<float>> _positions;
    private readonly List<Vector3D<float>> _normals;
    private readonly List<Vector2D<float>> _uvs;
    private readonly List<VertexBoneInfo> _vertexBoneInfos;
    private readonly List<uint> _indices;
    private readonly List<Material> _materials;
    private readonly List<Mesh> _meshes;

    public PmxModel()
    {
        _positions = new List<Vector3D<float>>();
        _normals = new List<Vector3D<float>>();
        _uvs = new List<Vector2D<float>>();
        _vertexBoneInfos = new List<VertexBoneInfo>();
        _indices = new List<uint>();
        _materials = new List<Material>();
        _meshes = new List<Mesh>();
    }

    public bool Load(string path, string mmdDataDir)
    {
        Destroy();

        PmxParsing? pmx = PmxParsing.ParsingByFile(path);
        if (pmx == null)
        {
            return false;
        }

        string dir = Path.GetDirectoryName(path)!;

        // 坐标点、法线、UV、骨骼信息
        foreach (PmxVertex vertex in pmx.Vertices)
        {
            Vector3D<float> position = vertex.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f);
            Vector3D<float> normal = vertex.Normal * new Vector3D<float>(1.0f, 1.0f, -1.0f);
            Vector2D<float> uv = new(vertex.UV.X, 1.0f - vertex.UV.Y);
            _positions.Add(position);
            _normals.Add(normal);
            _uvs.Add(uv);

            VertexBoneInfo vertexBoneInfo = new();

            if (vertex.WeightType != PmxVertexWeight.SDEF)
            {
                vertexBoneInfo.BoneIndices[0] = vertex.BoneIndices[0];
                vertexBoneInfo.BoneIndices[1] = vertex.BoneIndices[1];
                vertexBoneInfo.BoneIndices[2] = vertex.BoneIndices[2];
                vertexBoneInfo.BoneIndices[3] = vertex.BoneIndices[3];

                vertexBoneInfo.BoneWeights[0] = vertex.BoneWeights[0];
                vertexBoneInfo.BoneWeights[1] = vertex.BoneWeights[1];
                vertexBoneInfo.BoneWeights[2] = vertex.BoneWeights[2];
                vertexBoneInfo.BoneWeights[3] = vertex.BoneWeights[3];
            }

            switch (vertex.WeightType)
            {
                case PmxVertexWeight.BDEF1:
                    vertexBoneInfo.SkinningType = SkinningType.Weight1;
                    break;
                case PmxVertexWeight.BDEF2:
                    vertexBoneInfo.SkinningType = SkinningType.Weight2;
                    vertexBoneInfo.BoneWeights[1] = 1.0f - vertex.BoneWeights[0];
                    break;
                case PmxVertexWeight.BDEF4:
                    vertexBoneInfo.SkinningType = SkinningType.Weight4;
                    break;
                case PmxVertexWeight.SDEF:
                    vertexBoneInfo.SkinningType = SkinningType.SDEF;
                    {
                        float w0 = vertex.BoneWeights[0];
                        float w1 = 1.0f - vertex.BoneWeights[0];

                        Vector3D<float> center = vertex.SdefC * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> r0 = vertex.SdefR0 * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> r1 = vertex.SdefR1 * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> rw = (r0 * w0) + (r1 * w1);
                        r0 = center + r0 - rw;
                        r1 = center + r1 - rw;
                        Vector3D<float> cr0 = (center + r0) * 0.5f;
                        Vector3D<float> cr1 = (center + r1) * 0.5f;

                        vertexBoneInfo.SDEF.BoneIndices[0] = vertex.BoneIndices[0];
                        vertexBoneInfo.SDEF.BoneIndices[1] = vertex.BoneIndices[1];
                        vertexBoneInfo.SDEF.BoneWeight = vertex.BoneWeights[0];
                        vertexBoneInfo.SDEF.C = center;
                        vertexBoneInfo.SDEF.R0 = cr0;
                        vertexBoneInfo.SDEF.R1 = cr1;
                    }
                    break;
                case PmxVertexWeight.QDEF:
                    vertexBoneInfo.SkinningType = SkinningType.DualQuaternion;
                    break;
                default:
                    vertexBoneInfo.SkinningType = SkinningType.Weight1;
                    break;
            }

            _vertexBoneInfos.Add(vertexBoneInfo);
        }

        // 面信息
        foreach (PmxFace face in pmx.Faces)
        {
            _indices.Add(face.Vertices[2]);
            _indices.Add(face.Vertices[1]);
            _indices.Add(face.Vertices[0]);
        }

        // 纹理
        List<string> texturePaths = new();
        foreach (PmxTexture texture in pmx.Textures)
        {
            string texPath = Path.Combine(dir, texture.Name);
            if (File.Exists(texPath))
            {
                texturePaths.Add(texPath);
            }
            else
            {
                throw new FileNotFoundException($"Texture file not found: {texPath}");
            }
        }

        // 材质信息
        uint beginIndex = 0;
        foreach (PmxMaterial material in pmx.Materials)
        {
            Material mat = new()
            {
                Name = material.Name,
                Diffuse = material.Diffuse.ToVector3D(),
                Alpha = material.Diffuse.W,
                Specular = material.Specular,
                SpecularPower = material.SpecularPower,
                Ambient = material.Ambient,
                EdgeFlag = Convert.ToByte(material.DrawMode.HasFlag(PmxDrawModeFlags.DrawEdge)),
                EdgeSize = material.EdgeSize,
                EdgeColor = material.EdgeColor,
                SpTextureMode = SphereTextureMode.None,
                BothFace = material.DrawMode.HasFlag(PmxDrawModeFlags.BothFace),
                GroundShadow = material.DrawMode.HasFlag(PmxDrawModeFlags.GroundShadow),
                ShadowCaster = material.DrawMode.HasFlag(PmxDrawModeFlags.CastSelfShadow),
                ShadowReceiver = material.DrawMode.HasFlag(PmxDrawModeFlags.RecieveSelfShadow)
            };

            // 纹理
            if (material.TextureIndex != -1)
            {
                mat.Texture = texturePaths[material.TextureIndex];
            }

            // 卡通纹理
            if (material.ToonMode == PmxToonMode.Common)
            {
                if (material.ToonTextureIndex != -1)
                {
                    string texName = material.ToonTextureIndex + 1 < 10 ? $"0{material.ToonTextureIndex + 1}" : $"{material.ToonTextureIndex + 1}";

                    mat.ToonTexture = Path.Combine(mmdDataDir, $"toon{texName}.bmp");
                }
            }
            else if (material.ToonMode == PmxToonMode.Separate)
            {
                if (material.ToonTextureIndex != -1)
                {
                    mat.ToonTexture = texturePaths[material.ToonTextureIndex];
                }
            }

            // 球形纹理
            if (material.SphereTextureIndex != -1)
            {
                mat.SpTexture = texturePaths[material.SphereTextureIndex];
                mat.SpTextureMode = SphereTextureMode.None;
                if (material.SphereMode == PmxSphereMode.Mul)
                {
                    mat.SpTextureMode = SphereTextureMode.Mul;
                }
                else if (material.SphereMode == PmxSphereMode.Add)
                {
                    mat.SpTextureMode = SphereTextureMode.Add;
                }
                else if (material.SphereMode == PmxSphereMode.SubTexture)
                {
                    // TODO: SphareTexture が SubTexture の処理
                }
            }

            _materials.Add(mat);
            _meshes.Add(new Mesh(beginIndex, (uint)material.FaceVerticesCount, mat));

            beginIndex += (uint)material.FaceVerticesCount;
        }

        return true;
    }

    public void Destroy()
    {
        _positions.Clear();
        _normals.Clear();
        _uvs.Clear();
        _vertexBoneInfos.Clear();
        _indices.Clear();
        _materials.Clear();
        _meshes.Clear();
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
