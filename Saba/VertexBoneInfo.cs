using Silk.NET.Maths;

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

#region Classes
public class SDEF
{
    public int[] BoneIndices { get; } = new int[2];

    public float BoneWeight { get; set; }

    public Vector3D<float> C { get; set; }

    public Vector3D<float> R0 { get; set; }

    public Vector3D<float> R1 { get; set; }
}
#endregion

public class VertexBoneInfo
{
    public SkinningType SkinningType { get; set; }

    public int[] BoneIndices { get; } = new int[4];

    public float[] BoneWeights { get; } = new float[4];

    public SDEF SDEF { get; } = new SDEF();
}
