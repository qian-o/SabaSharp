using Silk.NET.Maths;

namespace Saba;

public unsafe abstract class MMDModel : IDisposable
{
    public abstract bool Load(string path, string mmdDataDir);

    public abstract int GetVertexCount();

    public abstract Vector3D<float>* GetPositions();

    public abstract Vector3D<float>* GetNormals();

    public abstract Vector2D<float>* GetUVs();

    public abstract int GetIndexCount();

    public abstract uint* GetIndices();

    public abstract MMDMaterial[] GetMaterials();

    public abstract MMDMesh[] GetMeshes();

    public abstract void Destroy();

    public abstract void Dispose();
}