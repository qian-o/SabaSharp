namespace Saba;

public class MMDMesh
{
    public uint BeginIndex { get; }

    public uint VertexCount { get; }

    public MMDMaterial Material { get; }

    public MMDMesh(uint beginIndex, uint vertexCount, MMDMaterial material)
    {
        BeginIndex = beginIndex;
        VertexCount = vertexCount;
        Material = material;
    }
}
