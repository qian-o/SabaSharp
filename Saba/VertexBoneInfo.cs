using System.Numerics;

namespace Saba;

#region Enums
public enum SkinningType
{
    Weight1,
    Weight2,
    Weight4,
    SDEF,
    DualQuaternion
}
#endregion

#region Structs
public unsafe struct SDEF
{
    public fixed int BoneIndices[2];

    public float BoneWeight;

    public Vector3 C;

    public Vector3 R0;

    public Vector3 R1;
}
#endregion

public unsafe struct VertexBoneInfo
{
    public SkinningType SkinningType;

    public fixed int BoneIndices[4];

    public fixed float BoneWeights[4];

    public SDEF SDEF;
}
