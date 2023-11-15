namespace Saba;

public class MMDMesh(uint beginIndex, uint vertexCount, MMDMaterial material)
{
    public uint BeginIndex { get; } = beginIndex;

    public uint VertexCount { get; } = vertexCount;

    public MMDMaterial Material { get; } = material;
}
