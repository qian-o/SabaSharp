namespace Saba;

public class Mesh
{
    public uint BeginIndex { get; }

    public uint VertexCount { get; }

    public Material Material { get; }

    public Mesh(uint beginIndex, uint vertexCount, Material material)
    {
        BeginIndex = beginIndex;
        VertexCount = vertexCount;
        Material = material;
    }
}
