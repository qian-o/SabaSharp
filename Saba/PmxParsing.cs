using Saba.Helpers;
using Silk.NET.Maths;
using System.Text;

namespace Saba;

#region Enums
public enum PmxVertexWeight : byte
{
    BDEF1,
    BDEF2,
    BDEF4,
    SDEF,
    QDEF
}

public enum PmxDrawModeFlags : byte
{
    BothFace = 0x01,
    GroundShadow = 0x02,
    CastSelfShadow = 0x04,
    RecieveSelfShadow = 0x08,
    DrawEdge = 0x10,
    VertexColor = 0x20,
    DrawPoint = 0x40,
    DrawLine = 0x80
}

public enum PmxSphereMode : byte
{
    None,
    Mul,
    Add,
    SubTexture
}

public enum PmxToonMode : byte
{
    Separate,
    Common
}

public enum PmxBoneFlags : ushort
{
    TargetShowMode = 0x0001,
    AllowRotate = 0x0002,
    AllowTranslate = 0x0004,
    Visible = 0x0008,
    AllowControl = 0x0010,
    IK = 0x0020,
    AppendLocal = 0x0080,
    AppendRotate = 0x0100,
    AppendTranslate = 0x0200,
    FixedAxis = 0x0400,
    LocalAxis = 0x800,
    DeformAfterPhysics = 0x1000,
    DeformOuterParent = 0x2000
}

public enum PmxMorphType : byte
{
    Group,
    Position,
    Bone,
    UV,
    AddUV1,
    AddUV2,
    AddUV3,
    AddUV4,
    Material,
    Flip,
    Impluse
}

public enum PmxMorphCategory : byte
{
    System,
    Eyebrow,
    Eye,
    Mouth,
    Other
}

public enum PmxOpType : byte
{
    Mul,
    Add
}

public enum PmxTargetType : byte
{
    BoneIndex,
    MorphIndex
}

public enum PmxFrameType : byte
{
    DefaultFrame,
    SpecialFrame
}

public enum PmxShape : byte
{
    Sphere,
    Box,
    Capsule
}

public enum PmxOperation : byte
{
    Static,
    Dynamic,
    DynamicAndBoneMerge
}

public enum PmxJointType : byte
{
    SpringDOF6,
    DOF6,
    P2P,
    ConeTwist,
    Slider,
    Hinge
}

public enum PmxSoftBodyType : byte
{
    TriMesh,
    Rope
}

public enum PmxSoftBodyMask : byte
{
    BLink = 0x01,
    Cluster = 0x02,
    HybridLink = 0x04
}

public enum PmxAeroModel
{
    V_TwoSided,
    V_OneSided,
    F_TwoSided,
    F_OneSided
}
#endregion

#region Classes
public class PmxHeader
{
    public string Magic { get; }

    public float Version { get; }

    public byte DataSize { get; }

    public Encoding Encoding { get; }

    public byte AdditionalUV { get; }

    public byte VertexIndexSize { get; }

    public byte TextureIndexSize { get; }

    public byte MaterialIndexSize { get; }

    public byte BoneIndexSize { get; }

    public byte MorphIndexSize { get; }

    public byte RigidBodyIndexSize { get; }

    public PmxHeader(BinaryReader binaryReader)
    {
        Magic = binaryReader.ReadString(4);
        Version = binaryReader.ReadSingle();
        DataSize = binaryReader.ReadByte();
        Encoding = binaryReader.ReadByte() == 0 ? Encoding.Unicode : Encoding.UTF8;
        AdditionalUV = binaryReader.ReadByte();
        VertexIndexSize = binaryReader.ReadByte();
        TextureIndexSize = binaryReader.ReadByte();
        MaterialIndexSize = binaryReader.ReadByte();
        BoneIndexSize = binaryReader.ReadByte();
        MorphIndexSize = binaryReader.ReadByte();
        RigidBodyIndexSize = binaryReader.ReadByte();
    }
}

public class PmxInfo
{
    public string ModelName { get; }

    public string ModelNameEn { get; }

    public string Comment { get; }

    public string CommentEn { get; }

    public PmxInfo(BinaryReader binaryReader, PmxHeader header)
    {
        ModelName = binaryReader.ReadString(header.Encoding);
        ModelNameEn = binaryReader.ReadString(header.Encoding);
        Comment = binaryReader.ReadString(header.Encoding);
        CommentEn = binaryReader.ReadString(header.Encoding);
    }
}

public class PmxVertex
{
    public Vector3D<float> Position { get; }

    public Vector3D<float> Normal { get; }

    public Vector2D<float> UV { get; }

    public Vector4D<float>[] AdditionalUV { get; } = new Vector4D<float>[4];

    public PmxVertexWeight WeightType { get; }

    public int[] BoneIndices { get; } = new int[4];

    public float[] BoneWeights { get; } = new float[4];

    public Vector3D<float> SdefC { get; }

    public Vector3D<float> SdefR0 { get; }

    public Vector3D<float> SdefR1 { get; }

    public float EdgeScale { get; }

    public PmxVertex(BinaryReader binaryReader, PmxHeader header)
    {
        Position = binaryReader.ReadVector3D();
        Normal = binaryReader.ReadVector3D();
        UV = binaryReader.ReadVector2D();

        for (byte i = 0; i < header.AdditionalUV; i++)
        {
            AdditionalUV[i] = binaryReader.ReadVector4D();
        }

        WeightType = (PmxVertexWeight)binaryReader.ReadByte();

        switch (WeightType)
        {
            case PmxVertexWeight.BDEF1:
                BoneIndices[0] = binaryReader.ReadIndex(header.BoneIndexSize);
                break;
            case PmxVertexWeight.BDEF2:
                BoneIndices[0] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[1] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneWeights[0] = binaryReader.ReadSingle();
                break;
            case PmxVertexWeight.BDEF4:
                BoneIndices[0] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[1] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[2] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[3] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneWeights[0] = binaryReader.ReadSingle();
                BoneWeights[1] = binaryReader.ReadSingle();
                BoneWeights[2] = binaryReader.ReadSingle();
                BoneWeights[3] = binaryReader.ReadSingle();
                break;
            case PmxVertexWeight.SDEF:
                BoneIndices[0] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[1] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneWeights[0] = binaryReader.ReadSingle();
                SdefC = binaryReader.ReadVector3D();
                SdefR0 = binaryReader.ReadVector3D();
                SdefR1 = binaryReader.ReadVector3D();
                break;
            case PmxVertexWeight.QDEF:
                BoneIndices[0] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[1] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[2] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneIndices[3] = binaryReader.ReadIndex(header.BoneIndexSize);
                BoneWeights[0] = binaryReader.ReadSingle();
                BoneWeights[1] = binaryReader.ReadSingle();
                BoneWeights[2] = binaryReader.ReadSingle();
                BoneWeights[3] = binaryReader.ReadSingle();
                break;
            default: break;
        }

        EdgeScale = binaryReader.ReadSingle();
    }
}

public class PmxFace
{
    public uint[] Vertices { get; } = new uint[3];

    public PmxFace(BinaryReader binaryReader, PmxHeader header)
    {
        switch (header.VertexIndexSize)
        {
            case 1:
                {
                    Vertices[0] = binaryReader.ReadByte();
                    Vertices[1] = binaryReader.ReadByte();
                    Vertices[2] = binaryReader.ReadByte();
                }
                break;
            case 2:
                {
                    Vertices[0] = binaryReader.ReadUInt16();
                    Vertices[1] = binaryReader.ReadUInt16();
                    Vertices[2] = binaryReader.ReadUInt16();
                }
                break;
            case 4:
                {
                    Vertices[0] = binaryReader.ReadUInt32();
                    Vertices[1] = binaryReader.ReadUInt32();
                    Vertices[2] = binaryReader.ReadUInt32();
                }
                break;
            default: break;
        }
    }
}

public class PmxTexture
{
    public string Name { get; }

    public PmxTexture(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
    }
}

public class PmxMaterial
{
    public string Name { get; }

    public string NameEn { get; }

    public Vector4D<float> Diffuse { get; }

    public Vector3D<float> Specular { get; }

    public float SpecularPower { get; }

    public Vector3D<float> Ambient { get; }

    public PmxDrawModeFlags DrawMode { get; }

    public Vector4D<float> EdgeColor { get; }

    public float EdgeSize { get; }

    public int TextureIndex { get; }

    public int SphereTextureIndex { get; }

    public PmxSphereMode SphereMode { get; }

    public PmxToonMode ToonMode { get; }

    public int ToonTextureIndex { get; }

    public string Memo { get; }

    public int FaceCount { get; }

    public PmxMaterial(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        Diffuse = binaryReader.ReadVector4D();
        Specular = binaryReader.ReadVector3D();
        SpecularPower = binaryReader.ReadSingle();
        Ambient = binaryReader.ReadVector3D();
        DrawMode = (PmxDrawModeFlags)binaryReader.ReadByte();
        EdgeColor = binaryReader.ReadVector4D();
        EdgeSize = binaryReader.ReadSingle();
        TextureIndex = binaryReader.ReadIndex(header.TextureIndexSize);
        SphereTextureIndex = binaryReader.ReadIndex(header.TextureIndexSize);
        SphereMode = (PmxSphereMode)binaryReader.ReadByte();
        ToonMode = (PmxToonMode)binaryReader.ReadByte();

        if (ToonMode == PmxToonMode.Separate)
        {
            ToonTextureIndex = binaryReader.ReadIndex(header.TextureIndexSize);
        }
        else if (ToonMode == PmxToonMode.Common)
        {
            ToonTextureIndex = binaryReader.ReadByte();
        }
        else
        {
            ToonTextureIndex = -1;
        }

        Memo = binaryReader.ReadString(header.Encoding);
        FaceCount = binaryReader.ReadInt32();
    }
}

public class PmxBone
{
    public class IKLink
    {
        public int BoneIndex { get; }

        public bool EnableLimit { get; }

        public Vector3D<float> LowerLimit { get; }

        public Vector3D<float> UpperLimit { get; }

        public IKLink(BinaryReader binaryReader, PmxHeader header)
        {
            BoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
            EnableLimit = binaryReader.ReadBoolean();

            if (EnableLimit)
            {
                LowerLimit = binaryReader.ReadVector3D();
                UpperLimit = binaryReader.ReadVector3D();
            }
        }
    }

    public string Name { get; }

    public string NameEn { get; }

    public Vector3D<float> Position { get; }

    public int ParentBoneIndex { get; }

    public int DeformDepth { get; }

    public PmxBoneFlags BoneFlags { get; }

    public Vector3D<float> PositionOffset { get; }

    public int LinkBoneIndex { get; }

    public int AppendBoneIndex { get; }

    public float AppendWeight { get; }

    public Vector3D<float> FixedAxis { get; }

    public Vector3D<float> LocalAxisX { get; }

    public Vector3D<float> LocalAxisZ { get; }

    public int KeyValue { get; }

    public int IKTargetBoneIndex { get; }

    public int IKIterationCount { get; }

    public float IKLimit { get; }

    public IKLink[] IKLinks { get; } = Array.Empty<IKLink>();

    public PmxBone(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        Position = binaryReader.ReadVector3D();
        ParentBoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
        DeformDepth = binaryReader.ReadInt32();
        BoneFlags = (PmxBoneFlags)binaryReader.ReadUInt16();

        if (BoneFlags.HasFlag(PmxBoneFlags.TargetShowMode))
        {
            LinkBoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
        }
        else
        {
            PositionOffset = binaryReader.ReadVector3D();
        }

        if (BoneFlags.HasFlag(PmxBoneFlags.AppendRotate) || BoneFlags.HasFlag(PmxBoneFlags.AppendTranslate))
        {
            AppendBoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
            AppendWeight = binaryReader.ReadSingle();
        }

        if (BoneFlags.HasFlag(PmxBoneFlags.FixedAxis))
        {
            FixedAxis = binaryReader.ReadVector3D();
        }

        if (BoneFlags.HasFlag(PmxBoneFlags.LocalAxis))
        {
            LocalAxisX = binaryReader.ReadVector3D();
            LocalAxisZ = binaryReader.ReadVector3D();
        }

        if (BoneFlags.HasFlag(PmxBoneFlags.DeformOuterParent))
        {
            KeyValue = binaryReader.ReadInt32();
        }

        if (BoneFlags.HasFlag(PmxBoneFlags.IK))
        {
            IKTargetBoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
            IKIterationCount = binaryReader.ReadInt32();
            IKLimit = binaryReader.ReadSingle();

            IKLinks = new IKLink[binaryReader.ReadInt32()];

            for (int i = 0; i < IKLinks.Length; i++)
            {
                IKLinks[i] = new IKLink(binaryReader, header);
            }
        }
    }
}

public class PmxMorph
{
    public class PositionMorph
    {
        public int VertexIndex { get; }

        public Vector3D<float> Position { get; }

        public PositionMorph(BinaryReader binaryReader, PmxHeader header)
        {
            VertexIndex = binaryReader.ReadIndex(header.VertexIndexSize);
            Position = binaryReader.ReadVector3D();
        }
    }

    public class UVMorph
    {
        public int VertexIndex { get; }

        public Vector4D<float> UV { get; }

        public UVMorph(BinaryReader binaryReader, PmxHeader header)
        {
            VertexIndex = binaryReader.ReadIndex(header.VertexIndexSize);
            UV = binaryReader.ReadVector4D();
        }
    }

    public class BoneMorph
    {
        public int BoneIndex { get; }

        public Vector3D<float> Position { get; }

        public Quaternion<float> Quaternion { get; }

        public BoneMorph(BinaryReader binaryReader, PmxHeader header)
        {
            BoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
            Position = binaryReader.ReadVector3D();
            Quaternion = binaryReader.ReadQuaternion();
        }
    }

    public class MaterialMorph
    {
        public int MaterialIndex { get; }

        public PmxOpType OpType { get; }

        public Vector4D<float> Diffuse { get; }

        public Vector3D<float> Specular { get; }

        public float SpecularPower { get; }

        public Vector3D<float> Ambient { get; }

        public Vector4D<float> EdgeColor { get; }

        public float EdgeSize { get; }

        public Vector4D<float> TextureCoefficient { get; }

        public Vector4D<float> SphereTextureCoefficient { get; }

        public Vector4D<float> ToonTextureCoefficient { get; }

        public MaterialMorph(BinaryReader binaryReader, PmxHeader header)
        {
            MaterialIndex = binaryReader.ReadIndex(header.MaterialIndexSize);
            OpType = (PmxOpType)binaryReader.ReadByte();
            Diffuse = binaryReader.ReadVector4D();
            Specular = binaryReader.ReadVector3D();
            SpecularPower = binaryReader.ReadSingle();
            Ambient = binaryReader.ReadVector3D();
            EdgeColor = binaryReader.ReadVector4D();
            EdgeSize = binaryReader.ReadSingle();
            TextureCoefficient = binaryReader.ReadVector4D();
            SphereTextureCoefficient = binaryReader.ReadVector4D();
            ToonTextureCoefficient = binaryReader.ReadVector4D();
        }
    }

    public class GroupMorph
    {
        public int MorphIndex { get; }

        public float Weight { get; }

        public GroupMorph(BinaryReader binaryReader, PmxHeader header)
        {
            MorphIndex = binaryReader.ReadIndex(header.MorphIndexSize);
            Weight = binaryReader.ReadSingle();
        }
    }

    public class FlipMorph
    {
        public int MorphIndex { get; }

        public float Weight { get; }

        public FlipMorph(BinaryReader binaryReader, PmxHeader header)
        {
            MorphIndex = binaryReader.ReadIndex(header.MorphIndexSize);
            Weight = binaryReader.ReadSingle();
        }
    }

    public class ImpulseMorph
    {
        public int RigidBodyIndex { get; }

        public bool Local { get; }

        public Vector3D<float> Velocity { get; }

        public Vector3D<float> Torque { get; }

        public ImpulseMorph(BinaryReader binaryReader, PmxHeader header)
        {
            RigidBodyIndex = binaryReader.ReadIndex(header.RigidBodyIndexSize);
            Local = binaryReader.ReadBoolean();
            Velocity = binaryReader.ReadVector3D();
            Torque = binaryReader.ReadVector3D();
        }
    }

    public string Name { get; }

    public string NameEn { get; }

    public PmxMorphCategory ControlPanel { get; }

    public PmxMorphType MorphType { get; }

    public PositionMorph[] PositionMorphs { get; } = Array.Empty<PositionMorph>();

    public UVMorph[] UVMorphs { get; } = Array.Empty<UVMorph>();

    public BoneMorph[] BoneMorphs { get; } = Array.Empty<BoneMorph>();

    public MaterialMorph[] MaterialMorphs { get; } = Array.Empty<MaterialMorph>();

    public GroupMorph[] GroupMorphs { get; } = Array.Empty<GroupMorph>();

    public FlipMorph[] FlipMorphs { get; } = Array.Empty<FlipMorph>();

    public ImpulseMorph[] ImpulseMorphs { get; } = Array.Empty<ImpulseMorph>();

    public PmxMorph(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        ControlPanel = (PmxMorphCategory)binaryReader.ReadByte();
        MorphType = (PmxMorphType)binaryReader.ReadByte();

        switch (MorphType)
        {
            case PmxMorphType.Group:
                {
                    GroupMorphs = new GroupMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < GroupMorphs.Length; i++)
                    {
                        GroupMorphs[i] = new GroupMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.Position:
                {
                    PositionMorphs = new PositionMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < PositionMorphs.Length; i++)
                    {
                        PositionMorphs[i] = new PositionMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.Bone:
                {
                    BoneMorphs = new BoneMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < BoneMorphs.Length; i++)
                    {
                        BoneMorphs[i] = new BoneMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.UV:
            case PmxMorphType.AddUV1:
            case PmxMorphType.AddUV2:
            case PmxMorphType.AddUV3:
            case PmxMorphType.AddUV4:
                {
                    UVMorphs = new UVMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < UVMorphs.Length; i++)
                    {
                        UVMorphs[i] = new UVMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.Material:
                {
                    MaterialMorphs = new MaterialMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < MaterialMorphs.Length; i++)
                    {
                        MaterialMorphs[i] = new MaterialMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.Flip:
                {
                    FlipMorphs = new FlipMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < FlipMorphs.Length; i++)
                    {
                        FlipMorphs[i] = new FlipMorph(binaryReader, header);
                    }
                }
                break;
            case PmxMorphType.Impluse:
                {
                    ImpulseMorphs = new ImpulseMorph[binaryReader.ReadInt32()];

                    for (int i = 0; i < ImpulseMorphs.Length; i++)
                    {
                        ImpulseMorphs[i] = new ImpulseMorph(binaryReader, header);
                    }
                }
                break;
            default: break;
        }
    }
}

public class PmxDisplayFrame
{
    public class Target
    {
        public PmxTargetType TargetType { get; }

        public int Index { get; }

        public Target(BinaryReader binaryReader, PmxHeader header)
        {
            TargetType = (PmxTargetType)binaryReader.ReadByte();
            Index = binaryReader.ReadIndex(TargetType == PmxTargetType.BoneIndex ? header.BoneIndexSize : header.MorphIndexSize);
        }
    }

    public string Name { get; }

    public string NameEn { get; }

    public PmxFrameType FrameType { get; }

    public Target[] Targets { get; } = Array.Empty<Target>();

    public PmxDisplayFrame(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        FrameType = (PmxFrameType)binaryReader.ReadByte();

        Targets = new Target[binaryReader.ReadInt32()];

        for (int i = 0; i < Targets.Length; i++)
        {
            Targets[i] = new Target(binaryReader, header);
        }
    }
}

public class PmxRigidBody
{
    public string Name { get; }

    public string NameEn { get; }

    public int BoneIndex { get; }

    public byte Group { get; }

    public ushort CollisionGroup { get; }

    public PmxShape Shape { get; }

    public Vector3D<float> ShapeSize { get; }

    public Vector3D<float> Translate { get; }

    public Vector3D<float> Rotate { get; }

    public float Mass { get; }

    public float TranslateDimmer { get; }

    public float RotateDimmer { get; }

    public float Repulsion { get; }

    public float Friction { get; }

    public PmxOperation Op { get; }

    public PmxRigidBody(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        BoneIndex = binaryReader.ReadIndex(header.BoneIndexSize);
        Group = binaryReader.ReadByte();
        CollisionGroup = binaryReader.ReadUInt16();
        Shape = (PmxShape)binaryReader.ReadByte();
        ShapeSize = binaryReader.ReadVector3D();
        Translate = binaryReader.ReadVector3D();
        Rotate = binaryReader.ReadVector3D();
        Mass = binaryReader.ReadSingle();
        TranslateDimmer = binaryReader.ReadSingle();
        RotateDimmer = binaryReader.ReadSingle();
        Repulsion = binaryReader.ReadSingle();
        Friction = binaryReader.ReadSingle();
        Op = (PmxOperation)binaryReader.ReadByte();
    }
}

public class PmxJoint
{
    public string Name { get; }

    public string NameEn { get; }

    public PmxJointType Type { get; }

    public int RigidBodyIndexA { get; }

    public int RigidBodyIndexB { get; }

    public Vector3D<float> Translate { get; }

    public Vector3D<float> Rotate { get; }

    public Vector3D<float> TranslateLimitMin { get; }

    public Vector3D<float> TranslateLimitMax { get; }

    public Vector3D<float> RotateLimitMin { get; }

    public Vector3D<float> RotateLimitMax { get; }

    public Vector3D<float> SpringTranslate { get; }

    public Vector3D<float> SpringRotate { get; }

    public PmxJoint(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        Type = (PmxJointType)binaryReader.ReadByte();
        RigidBodyIndexA = binaryReader.ReadIndex(header.RigidBodyIndexSize);
        RigidBodyIndexB = binaryReader.ReadIndex(header.RigidBodyIndexSize);
        Translate = binaryReader.ReadVector3D();
        Rotate = binaryReader.ReadVector3D();
        TranslateLimitMin = binaryReader.ReadVector3D();
        TranslateLimitMax = binaryReader.ReadVector3D();
        RotateLimitMin = binaryReader.ReadVector3D();
        RotateLimitMax = binaryReader.ReadVector3D();
        SpringTranslate = binaryReader.ReadVector3D();
        SpringRotate = binaryReader.ReadVector3D();
    }
}

public class PmxSoftBody
{
    public class AnchorRigidBody
    {
        public int RigidBodyIndex { get; }

        public int VertexIndex { get; }

        public bool NearMode { get; }

        public AnchorRigidBody(BinaryReader binaryReader, PmxHeader header)
        {
            RigidBodyIndex = binaryReader.ReadIndex(header.RigidBodyIndexSize);
            VertexIndex = binaryReader.ReadIndex(header.VertexIndexSize);
            NearMode = binaryReader.ReadBoolean();
        }
    }

    public string Name { get; }

    public string NameEn { get; }

    public PmxSoftBodyType Type { get; }

    public int MaterialIndex { get; }

    public byte Group { get; }

    public ushort CollisionGroup { get; }

    public PmxSoftBodyMask Flag { get; }

    public int BLinkLength { get; }

    public int NumClusters { get; }

    public float TotalMass { get; }

    public float CollisionMargin { get; }

    public PmxAeroModel AeroModel { get; }

    public float VCF { get; }

    public float DP { get; }

    public float DG { get; }

    public float LF { get; }

    public float PR { get; }

    public float VC { get; }

    public float DF { get; }

    public float MT { get; }

    public float CHR { get; }

    public float KHR { get; }

    public float SHR { get; }

    public float AHR { get; }

    public float SRHR_CL { get; }

    public float SKHR_CL { get; }

    public float SSHR_CL { get; }

    public float SR_SPLT_CL { get; }

    public float SK_SPLT_CL { get; }

    public float SS_SPLT_CL { get; }

    public int V_IT { get; }

    public int P_IT { get; }

    public int D_IT { get; }

    public int C_IT { get; }

    public float LST { get; }

    public float AST { get; }

    public float VST { get; }

    public AnchorRigidBody[] AnchorRigidbodies { get; } = Array.Empty<AnchorRigidBody>();

    public int[] PinVertexIndices { get; } = Array.Empty<int>();

    public PmxSoftBody(BinaryReader binaryReader, PmxHeader header)
    {
        Name = binaryReader.ReadString(header.Encoding);
        NameEn = binaryReader.ReadString(header.Encoding);
        Type = (PmxSoftBodyType)binaryReader.ReadByte();
        MaterialIndex = binaryReader.ReadIndex(header.MaterialIndexSize);
        Group = binaryReader.ReadByte();
        CollisionGroup = binaryReader.ReadUInt16();
        Flag = (PmxSoftBodyMask)binaryReader.ReadByte();
        BLinkLength = binaryReader.ReadInt32();
        NumClusters = binaryReader.ReadInt32();
        TotalMass = binaryReader.ReadSingle();
        CollisionMargin = binaryReader.ReadSingle();
        AeroModel = (PmxAeroModel)binaryReader.ReadInt32();
        VCF = binaryReader.ReadSingle();
        DP = binaryReader.ReadSingle();
        DG = binaryReader.ReadSingle();
        LF = binaryReader.ReadSingle();
        PR = binaryReader.ReadSingle();
        VC = binaryReader.ReadSingle();
        DF = binaryReader.ReadSingle();
        MT = binaryReader.ReadSingle();
        CHR = binaryReader.ReadSingle();
        KHR = binaryReader.ReadSingle();
        SHR = binaryReader.ReadSingle();
        AHR = binaryReader.ReadSingle();
        SRHR_CL = binaryReader.ReadSingle();
        SKHR_CL = binaryReader.ReadSingle();
        SSHR_CL = binaryReader.ReadSingle();
        SR_SPLT_CL = binaryReader.ReadSingle();
        SK_SPLT_CL = binaryReader.ReadSingle();
        SS_SPLT_CL = binaryReader.ReadSingle();
        V_IT = binaryReader.ReadInt32();
        P_IT = binaryReader.ReadInt32();
        D_IT = binaryReader.ReadInt32();
        C_IT = binaryReader.ReadInt32();
        LST = binaryReader.ReadSingle();
        AST = binaryReader.ReadSingle();
        VST = binaryReader.ReadSingle();

        AnchorRigidbodies = new AnchorRigidBody[binaryReader.ReadInt32()];

        for (int i = 0; i < AnchorRigidbodies.Length; i++)
        {
            AnchorRigidbodies[i] = new AnchorRigidBody(binaryReader, header);
        }

        PinVertexIndices = new int[binaryReader.ReadInt32()];

        for (int i = 0; i < PinVertexIndices.Length; i++)
        {
            PinVertexIndices[i] = binaryReader.ReadIndex(header.VertexIndexSize);
        }
    }
}
#endregion

public class PmxParsing
{
    public PmxHeader Header { get; }

    public PmxInfo Info { get; }

    public PmxVertex[] Vertices { get; }

    public PmxFace[] Faces { get; }

    public PmxTexture[] Textures { get; }

    public PmxMaterial[] Materials { get; }

    public PmxBone[] Bones { get; }

    public PmxMorph[] Morphs { get; }

    public PmxDisplayFrame[] DisplayFrames { get; }

    public PmxRigidBody[] RigidBodies { get; }

    public PmxJoint[] Joints { get; }

    public PmxSoftBody[] SoftBodies { get; }

    internal PmxParsing(PmxHeader header,
                        PmxInfo info,
                        PmxVertex[] vertices,
                        PmxFace[] faces,
                        PmxTexture[] textures,
                        PmxMaterial[] materials,
                        PmxBone[] bones,
                        PmxMorph[] morphs,
                        PmxDisplayFrame[] displayFrames,
                        PmxRigidBody[] rigidBodies,
                        PmxJoint[] joints,
                        PmxSoftBody[] softBodies)
    {
        Header = header;
        Info = info;
        Vertices = vertices;
        Faces = faces;
        Textures = textures;
        Materials = materials;
        Bones = bones;
        Morphs = morphs;
        DisplayFrames = displayFrames;
        RigidBodies = rigidBodies;
        Joints = joints;
        SoftBodies = softBodies;
    }

    public static PmxParsing? ParsingByFile(string path)
    {
        using BinaryReader binaryReader = new(File.OpenRead(path));

        PmxHeader header = ReadHeader(binaryReader);

        if (!header.Magic.ToUpper().Contains("PMX") || header.Version != 2)
        {
            return null;
        }

        return new PmxParsing(header,
                              ReadInfo(binaryReader, header),
                              ReadVertices(binaryReader, header),
                              ReadFaces(binaryReader, header),
                              ReadTextures(binaryReader, header),
                              ReadMaterials(binaryReader, header),
                              ReadBones(binaryReader, header),
                              ReadMorphs(binaryReader, header),
                              ReadDisplayFrames(binaryReader, header),
                              ReadRigidBodies(binaryReader, header),
                              ReadJoints(binaryReader, header),
                              ReadSoftBodies(binaryReader, header));
    }

    private static PmxHeader ReadHeader(BinaryReader binaryReader)
    {
        return new PmxHeader(binaryReader);
    }

    private static PmxInfo ReadInfo(BinaryReader binaryReader, PmxHeader header)
    {
        return new PmxInfo(binaryReader, header);
    }

    private static PmxVertex[] ReadVertices(BinaryReader binaryReader, PmxHeader header)
    {
        PmxVertex[] vertices = new PmxVertex[binaryReader.ReadInt32()];

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new PmxVertex(binaryReader, header);
        }

        return vertices;
    }

    private static PmxFace[] ReadFaces(BinaryReader binaryReader, PmxHeader header)
    {
        PmxFace[] faces = new PmxFace[binaryReader.ReadInt32() / 3];

        for (int i = 0; i < faces.Length; i++)
        {
            faces[i] = new PmxFace(binaryReader, header);
        }

        return faces;
    }

    private static PmxTexture[] ReadTextures(BinaryReader binaryReader, PmxHeader header)
    {
        PmxTexture[] textures = new PmxTexture[binaryReader.ReadInt32()];

        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new PmxTexture(binaryReader, header);
        }

        return textures;
    }

    private static PmxMaterial[] ReadMaterials(BinaryReader binaryReader, PmxHeader header)
    {
        PmxMaterial[] materials = new PmxMaterial[binaryReader.ReadInt32()];

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = new PmxMaterial(binaryReader, header);
        }

        return materials;
    }

    private static PmxBone[] ReadBones(BinaryReader binaryReader, PmxHeader header)
    {
        PmxBone[] bones = new PmxBone[binaryReader.ReadInt32()];

        for (int i = 0; i < bones.Length; i++)
        {
            bones[i] = new PmxBone(binaryReader, header);
        }

        return bones;
    }

    private static PmxMorph[] ReadMorphs(BinaryReader binaryReader, PmxHeader header)
    {
        PmxMorph[] morphs = new PmxMorph[binaryReader.ReadInt32()];

        for (int i = 0; i < morphs.Length; i++)
        {
            morphs[i] = new PmxMorph(binaryReader, header);
        }

        return morphs;
    }

    private static PmxDisplayFrame[] ReadDisplayFrames(BinaryReader binaryReader, PmxHeader header)
    {
        PmxDisplayFrame[] displayFrames = new PmxDisplayFrame[binaryReader.ReadInt32()];

        for (int i = 0; i < displayFrames.Length; i++)
        {
            displayFrames[i] = new PmxDisplayFrame(binaryReader, header);
        }

        return displayFrames;
    }

    private static PmxRigidBody[] ReadRigidBodies(BinaryReader binaryReader, PmxHeader header)
    {
        PmxRigidBody[] rigidbodies = new PmxRigidBody[binaryReader.ReadInt32()];

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i] = new PmxRigidBody(binaryReader, header);
        }

        return rigidbodies;
    }

    private static PmxJoint[] ReadJoints(BinaryReader binaryReader, PmxHeader header)
    {
        PmxJoint[] joints = new PmxJoint[binaryReader.ReadInt32()];

        for (int i = 0; i < joints.Length; i++)
        {
            joints[i] = new PmxJoint(binaryReader, header);
        }

        return joints;
    }

    private static PmxSoftBody[] ReadSoftBodies(BinaryReader binaryReader, PmxHeader header)
    {
        if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
        {
            return Array.Empty<PmxSoftBody>();
        }

        PmxSoftBody[] softBodies = new PmxSoftBody[binaryReader.ReadInt32()];

        for (int i = 0; i < softBodies.Length; i++)
        {
            softBodies[i] = new PmxSoftBody(binaryReader, header);
        }

        return softBodies;
    }
}