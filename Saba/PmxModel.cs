using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class PmxModel : MMDModel
{
    #region Enums
    private enum MorphType
    {
        None,
        Position,
        UV,
        Material,
        Bone,
        Group
    }
    #endregion

    #region Classes
    private class PmxMorph : MMDMorph
    {
        public MorphType MorphType { get; set; }

        public int DataIndex { get; set; }
    }

    private class PositionMorph
    {
        public int Index { get; set; }

        public Vector3D<float> Position { get; set; }
    }

    private class PositionMorphData
    {
        public List<PositionMorph> MorphVertices { get; } = new();
    }

    private class UVMorph
    {
        public int Index { get; set; }

        public Vector4D<float> UV { get; set; }
    }

    private class UVMorphData
    {
        public List<UVMorph> MorphUVs { get; } = new();
    }

    private class MaterialFactor
    {
        public Vector3D<float> Diffuse { get; set; }

        public float Alpha { get; set; }

        public Vector3D<float> Specular { get; set; }

        public float SpecularPower { get; set; }

        public Vector3D<float> Ambient { get; set; }

        public Vector4D<float> EdgeColor { get; set; }

        public float EdgeSize { get; set; }

        public Vector4D<float> TextureCoefficient { get; set; }

        public Vector4D<float> SphereTextureCoefficient { get; set; }

        public Vector4D<float> ToonTextureCoefficient { get; set; }

        public MaterialFactor(Saba.PmxMorph.MaterialMorph pmxMat)
        {
            Diffuse = pmxMat.Diffuse.ToVector3D();
            Alpha = pmxMat.Diffuse.W;
            Specular = pmxMat.Specular;
            SpecularPower = pmxMat.SpecularPower;
            Ambient = pmxMat.Ambient;
            EdgeColor = pmxMat.EdgeColor;
            EdgeSize = pmxMat.EdgeSize;
            TextureCoefficient = pmxMat.TextureCoefficient;
            SphereTextureCoefficient = pmxMat.SphereTextureCoefficient;
            ToonTextureCoefficient = pmxMat.ToonTextureCoefficient;
        }

        public void Mul(MaterialFactor val, float weight)
        {
            Diffuse = Vector3D.Lerp(Diffuse, Diffuse * val.Diffuse, weight);
            Alpha = MathHelper.Lerp(Alpha, Alpha * val.Alpha, weight);
            Specular = Vector3D.Lerp(Specular, Specular * val.Specular, weight);
            SpecularPower = MathHelper.Lerp(SpecularPower, SpecularPower * val.SpecularPower, weight);
            Ambient = Vector3D.Lerp(Ambient, Ambient * val.Ambient, weight);
            EdgeColor = Vector4D.Lerp(EdgeColor, EdgeColor * val.EdgeColor, weight);
            EdgeSize = MathHelper.Lerp(EdgeSize, EdgeSize * val.EdgeSize, weight);
            TextureCoefficient = Vector4D.Lerp(TextureCoefficient, TextureCoefficient * val.TextureCoefficient, weight);
            SphereTextureCoefficient = Vector4D.Lerp(SphereTextureCoefficient, SphereTextureCoefficient * val.SphereTextureCoefficient, weight);
            ToonTextureCoefficient = Vector4D.Lerp(ToonTextureCoefficient, ToonTextureCoefficient * val.ToonTextureCoefficient, weight);
        }

        public void Add(MaterialFactor val, float weight)
        {
            Diffuse += val.Diffuse * weight;
            Alpha += val.Alpha * weight;
            Specular += val.Specular * weight;
            SpecularPower += val.SpecularPower * weight;
            Ambient += val.Ambient * weight;
            EdgeColor += val.EdgeColor * weight;
            EdgeSize += val.EdgeSize * weight;
            TextureCoefficient += val.TextureCoefficient * weight;
            SphereTextureCoefficient += val.SphereTextureCoefficient * weight;
            ToonTextureCoefficient += val.ToonTextureCoefficient * weight;
        }
    }

    private class MaterialMorphData
    {
        public List<Saba.PmxMorph.MaterialMorph> MaterialMorphs { get; } = new();
    }

    private class BoneMorph
    {
        public MMDNode Node { get; }

        public Vector3D<float> Position { get; set; }

        public Quaternion<float> Rotate { get; set; }

        public BoneMorph(MMDNode node)
        {
            Node = node;
        }
    }

    private class BoneMorphData
    {
        public List<BoneMorph> BoneMorphs { get; } = new();
    }

    private class GroupMorphData
    {
        public List<Saba.PmxMorph.GroupMorph> GroupMorphs { get; } = new();
    }
    #endregion

    private readonly List<Vector3D<float>> _positions;
    private readonly List<Vector3D<float>> _normals;
    private readonly List<Vector2D<float>> _uvs;
    private readonly List<VertexBoneInfo> _vertexBoneInfos;
    private readonly List<uint> _indices;
    private readonly List<MMDMaterial> _materials;
    private readonly List<MMDMesh> _meshes;
    private readonly List<PmxNode> _nodes;
    private readonly List<PmxNode> _sortedNodes;
    private readonly List<MMDIkSolver> _ikSolvers;
    private readonly List<PmxMorph> _morphs;
    private readonly List<PositionMorphData> _positionMorphDatas;
    private readonly List<UVMorphData> _uvMorphDatas;
    private readonly List<MaterialMorphData> _materialMorphDatas;
    private readonly List<BoneMorphData> _boneMorphDatas;
    private readonly List<GroupMorphData> _groupMorphDatas;

    private Vector3D<float>[] morphPositions = Array.Empty<Vector3D<float>>();
    private Vector4D<float>[] morphUVs = Array.Empty<Vector4D<float>>();
    private Matrix4X4<float>[] transforms = Array.Empty<Matrix4X4<float>>();

    public PmxModel()
    {
        _positions = new List<Vector3D<float>>();
        _normals = new List<Vector3D<float>>();
        _uvs = new List<Vector2D<float>>();
        _vertexBoneInfos = new List<VertexBoneInfo>();
        _indices = new List<uint>();
        _materials = new List<MMDMaterial>();
        _meshes = new List<MMDMesh>();
        _nodes = new List<PmxNode>();
        _sortedNodes = new List<PmxNode>();
        _ikSolvers = new List<MMDIkSolver>();
        _morphs = new List<PmxMorph>();
        _positionMorphDatas = new List<PositionMorphData>();
        _uvMorphDatas = new List<UVMorphData>();
        _materialMorphDatas = new List<MaterialMorphData>();
        _boneMorphDatas = new List<BoneMorphData>();
        _groupMorphDatas = new List<GroupMorphData>();
    }

    public override bool Load(string path, string mmdDataDir)
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
            Vector2D<float> uv = new(vertex.UV.X, -vertex.UV.Y);
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

        Array.Resize(ref morphPositions, _positions.Count);
        Array.Resize(ref morphUVs, _uvs.Count);

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
            MMDMaterial mat = new()
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
            _meshes.Add(new MMDMesh(beginIndex, (uint)material.FaceVerticesCount, mat));

            beginIndex += (uint)material.FaceVerticesCount;
        }

        // 骨骼信息
        foreach (PmxBone bone in pmx.Bones)
        {
            _nodes.Add(new PmxNode() { Index = _nodes.Count, Name = bone.Name });
        }

        for (int i = 0; i < pmx.Bones.Length; i++)
        {
            int boneIndex = pmx.Bones.Length - i - 1;
            PmxBone bone = pmx.Bones[boneIndex];
            PmxNode node = _nodes[boneIndex];

            // 检查节点是否循环
            bool isLoop = false;
            if (bone.ParentBoneIndex != -1)
            {
                MMDNode? parent = _nodes[bone.ParentBoneIndex];
                while (parent != null)
                {
                    if (parent == node)
                    {
                        isLoop = true;

                        Console.WriteLine($"This bone hierarchy is a loop: bone= {bone.Name}");

                        break;
                    }

                    parent = parent.Parent;
                }
            }

            // 检查父级节点索引位置
            if (bone.ParentBoneIndex != -1)
            {
                if (bone.ParentBoneIndex >= boneIndex)
                {
                    Console.WriteLine($"The parent index of this node is big: bone= {bone.Name}");
                }
            }

            if (bone.ParentBoneIndex != -1 && !isLoop)
            {
                PmxBone parentBone = pmx.Bones[bone.ParentBoneIndex];
                MMDNode parent = _nodes[bone.ParentBoneIndex];

                parent.AddChild(node);

                Vector3D<float> localPos = bone.Position - parentBone.Position;
                localPos.Z *= -1.0f;
                node.Translate = localPos;
            }
            else
            {
                Vector3D<float> localPos = bone.Position;
                localPos.Z *= -1.0f;
                node.Translate = localPos;
            }

            Matrix4X4<float> init = Matrix4X4.CreateTranslation(bone.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f));

            node.Global = init;
            node.CalculateInverseInitTransform();

            node.DeformDepth = bone.DeformDepth;
            node.IsDeformAfterPhysics = bone.BoneFlags.HasFlag(PmxBoneFlags.DeformAfterPhysics);
            node.IsAppendRotate = bone.BoneFlags.HasFlag(PmxBoneFlags.AppendRotate);
            node.IsAppendTranslate = bone.BoneFlags.HasFlag(PmxBoneFlags.AppendTranslate);

            if ((node.IsAppendRotate || node.IsAppendTranslate) && bone.AppendBoneIndex != -1)
            {
                if (bone.AppendBoneIndex >= boneIndex)
                {
                    Console.WriteLine($"The parent(morph assignment) index of this node is big: bone= {bone.Name}");
                }
                node.AppendNode = _nodes[bone.AppendBoneIndex];
                node.IsAppendLocal = bone.BoneFlags.HasFlag(PmxBoneFlags.AppendLocal);
                node.AppendWeight = bone.AppendWeight;
            }
            node.SaveInitialTRS();
        }

        Array.Resize(ref transforms, _nodes.Count);
        _sortedNodes.AddRange(_nodes.OrderBy(item => item.DeformDepth));

        // IK
        for (int i = 0; i < pmx.Bones.Length; i++)
        {
            PmxBone bone = pmx.Bones[i];
            if (bone.BoneFlags.HasFlag(PmxBoneFlags.IK))
            {
                MMDIkSolver solver = new();
                PmxNode ikNode = _nodes[i];

                solver.IkNode = ikNode;
                ikNode.IkSolver = solver;

                _ikSolvers.Add(solver);

                if (bone.IKTargetBoneIndex < 0 || bone.IKTargetBoneIndex >= _nodes.Count)
                {
                    Console.WriteLine($"IK target bone index is invalid: bone= {bone.Name}");

                    continue;
                }

                solver.IkTarget = _nodes[bone.IKTargetBoneIndex];

                foreach (PmxBone.IKLink ikLink in bone.IKLinks)
                {
                    PmxNode linkNode = _nodes[ikLink.BoneIndex];
                    if (ikLink.EnableLimit)
                    {
                        Vector3D<float> limitMax = ikLink.LimitMin * new Vector3D<float>(-1.0f);
                        Vector3D<float> limitMin = ikLink.LimitMax * new Vector3D<float>(-1.0f);

                        solver.AddIkChain(linkNode, true, limitMin, limitMax);
                    }
                    else
                    {
                        solver.AddIkChain(linkNode);
                    }
                    linkNode.EnableIK = true;
                }

                solver.IterateCount = (uint)bone.IKIterationCount;
                solver.LimitAngle = bone.IKLimit;
            }
        }

        // Morph
        foreach (Saba.PmxMorph pmxMorph in pmx.Morphs)
        {
            PmxMorph morph = new()
            {
                Name = pmxMorph.Name,
                Weight = 0.0f,
                MorphType = MorphType.None
            };

            if (pmxMorph.MorphType == PmxMorphType.Position)
            {
                morph.MorphType = MorphType.Position;
                morph.DataIndex = _positionMorphDatas.Count;

                PositionMorphData morphData = new();
                foreach (Saba.PmxMorph.PositionMorph vtx in pmxMorph.PositionMorphs)
                {
                    PositionMorph morphVtx = new()
                    {
                        Index = vtx.VertexIndex,
                        Position = vtx.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f)
                    };

                    morphData.MorphVertices.Add(morphVtx);
                }
                _positionMorphDatas.Add(morphData);
            }
            else if (pmxMorph.MorphType == PmxMorphType.UV)
            {
                morph.MorphType = MorphType.UV;
                morph.DataIndex = _uvMorphDatas.Count;

                UVMorphData morphData = new();
                foreach (Saba.PmxMorph.UVMorph uv in pmxMorph.UVMorphs)
                {
                    UVMorph morphUV = new()
                    {
                        Index = uv.VertexIndex,
                        UV = uv.UV
                    };

                    morphData.MorphUVs.Add(morphUV);
                }
                _uvMorphDatas.Add(morphData);
            }
            else if (pmxMorph.MorphType == PmxMorphType.Material)
            {
                morph.MorphType = MorphType.Material;
                morph.DataIndex = _materialMorphDatas.Count;

                MaterialMorphData morphData = new();
                morphData.MaterialMorphs.AddRange(pmxMorph.MaterialMorphs);
                _materialMorphDatas.Add(morphData);
            }
            else if (pmxMorph.MorphType == PmxMorphType.Bone)
            {
                morph.MorphType = MorphType.Bone;
                morph.DataIndex = _boneMorphDatas.Count;

                BoneMorphData boneMorphData = new();
                foreach (Saba.PmxMorph.BoneMorph pmxBoneMorph in pmxMorph.BoneMorphs)
                {
                    BoneMorph boneMorph = new(_nodes[pmxBoneMorph.BoneIndex])
                    {
                        Position = pmxBoneMorph.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f)
                    };

                    Matrix3X3<float> invZ = Matrix3X3.CreateScale(new Vector3D<float>(1.0f, 1.0f, -1.0f));
                    Matrix3X3<float> rot0 = Matrix3X3.CreateFromQuaternion(pmxBoneMorph.Quaternion);
                    Matrix3X3<float> rot1 = invZ * rot0 * invZ;

                    boneMorph.Rotate = Quaternion<float>.CreateFromRotationMatrix(rot1);

                    boneMorphData.BoneMorphs.Add(boneMorph);
                }
                _boneMorphDatas.Add(boneMorphData);
            }
            else if (pmxMorph.MorphType == PmxMorphType.Group)
            {
                morph.MorphType = MorphType.Group;
                morph.DataIndex = _groupMorphDatas.Count;

                GroupMorphData groupMorphData = new();
                groupMorphData.GroupMorphs.AddRange(pmxMorph.GroupMorphs);
                _groupMorphDatas.Add(groupMorphData);
            }
            else
            {
                Console.WriteLine($"Not Supported Morp Type({pmxMorph.MorphType}): [{pmxMorph.Name}]");
            }
        }

        return true;
    }

    public override MMDNode[] GetNodes()
    {
        return _nodes.ToArray();
    }

    public override MMDMorph[] GetMorphs()
    {
        return _morphs.ToArray();
    }

    public override MMDIkSolver[] GetIkSolvers()
    {
        return _ikSolvers.ToArray();
    }

    public override MMDNode? FindNode(Predicate<MMDNode> predicate)
    {
        return _nodes.Find(predicate);
    }

    public override MMDMorph? FindMorph(Predicate<MMDMorph> predicate)
    {
        return _morphs.Find(predicate);
    }

    public override MMDIkSolver? FindIkSolver(Predicate<MMDIkSolver> predicate)
    {
        return _ikSolvers.Find(predicate);
    }

    public override int GetVertexCount()
    {
        return _positions.Count;
    }

    public override unsafe Vector3D<float>* GetPositions()
    {
        fixed (Vector3D<float>* ptr = _positions.ToArray())
        {
            return ptr;
        }
    }

    public override unsafe Vector3D<float>* GetNormals()
    {
        fixed (Vector3D<float>* ptr = _normals.ToArray())
        {
            return ptr;
        }
    }

    public override unsafe Vector2D<float>* GetUVs()
    {
        fixed (Vector2D<float>* ptr = _uvs.ToArray())
        {
            return ptr;
        }
    }

    public override int GetIndexCount()
    {
        return _indices.Count;
    }

    public override unsafe uint* GetIndices()
    {
        fixed (uint* ptr = _indices.ToArray())
        {
            return ptr;
        }
    }

    public override MMDMaterial[] GetMaterials()
    {
        return _materials.ToArray();
    }

    public override MMDMesh[] GetMeshes()
    {
        return _meshes.ToArray();
    }

    public override void InitializeAnimation()
    {
        ClearBaseAnimation();

        foreach (PmxNode node in _nodes)
        {
            node.AnimTranslate = Vector3D<float>.Zero;
            node.AnimRotate = Quaternion<float>.Identity;
        }

        BeginAnimation();

        foreach (PmxNode node in _nodes)
        {
            node.UpdateLocalTransform();
        }

        foreach (PmxMorph morph in _morphs)
        {
            morph.Weight = 0.0f;
        }

        foreach (MMDIkSolver ikSolver in _ikSolvers)
        {
            ikSolver.Enable = true;
        }

        foreach (PmxNode node in _nodes.Where(item => item.Parent != null))
        {
            node.UpdateGlobalTransform();
        }

        foreach (PmxNode node in _sortedNodes)
        {
            if (node.AppendNode != null)
            {
                node.UpdateAppendTransform();
                node.UpdateGlobalTransform();
            }

            if (node.IkSolver != null)
            {
                node.IkSolver.Solve();

                node.UpdateGlobalTransform();
            }
        }

        foreach (PmxNode node in _nodes.Where(item => item.Parent == null))
        {
            node.UpdateGlobalTransform();
        }

        EndAnimation();
    }

    public override void BeginAnimation()
    {
        foreach (PmxNode node in _nodes)
        {
            node.BeginUpdateTransform();
        }

        for (int i = 0; i < morphPositions.Length; i++)
        {
            morphPositions[i] = Vector3D<float>.Zero;
        }

        for (int i = 0; i < morphUVs.Length; i++)
        {
            morphUVs[i] = Vector4D<float>.Zero;
        }
    }

    public override void EndAnimation()
    {
        foreach (PmxNode node in _nodes)
        {
            node.EndUpdateTransform();
        }
    }

    public override void UpdateMorphAnimation()
    {
        throw new NotImplementedException();
    }

    public override void UpdateNodeAnimation(bool afterPhysicsAnim)
    {
        throw new NotImplementedException();
    }

    public override void UpdatePhysicsAnimation(float elapsed)
    {
        throw new NotImplementedException();
    }

    public override void Destroy()
    {
        _positions.Clear();
        _normals.Clear();
        _uvs.Clear();
        _vertexBoneInfos.Clear();
        _indices.Clear();
        _materials.Clear();
        _meshes.Clear();
        _nodes.Clear();
        _sortedNodes.Clear();
        _ikSolvers.Clear();
        _morphs.Clear();
        _positionMorphDatas.Clear();
        _uvMorphDatas.Clear();
        _materialMorphDatas.Clear();
        _boneMorphDatas.Clear();
        _groupMorphDatas.Clear();

        morphPositions = Array.Empty<Vector3D<float>>();
        morphUVs = Array.Empty<Vector4D<float>>();
        transforms = Array.Empty<Matrix4X4<float>>();
    }

    public override void Dispose()
    {
        Destroy();

        GC.SuppressFinalize(this);
    }
}
