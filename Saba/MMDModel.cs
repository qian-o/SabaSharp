using System.Numerics;

namespace Saba;

public unsafe abstract class MMDModel : IDisposable
{
    public abstract bool Load(string path, string mmdDataDir);

    public abstract MMDNode[] GetNodes();

    public abstract MMDMorph[] GetMorphs();

    public abstract MMDIkSolver[] GetIkSolvers();

    public abstract MMDNode? FindNode(Predicate<MMDNode> predicate);

    public abstract MMDMorph? FindMorph(Predicate<MMDMorph> predicate);

    public abstract MMDIkSolver? FindIkSolver(Predicate<MMDIkSolver> predicate);

    public abstract int GetVertexCount();

    public abstract Vector3* GetPositions();

    public abstract Vector3* GetNormals();

    public abstract Vector2* GetUVs();

    public abstract Vector3* GetUpdatePositions();

    public abstract Vector3* GetUpdateNormals();

    public abstract Vector2* GetUpdateUVs();

    public abstract int GetIndexCount();

    public abstract uint* GetIndices();

    public abstract MMDMaterial[] GetMaterials();

    public abstract MMDMesh[] GetMeshes();

    public abstract void InitializeAnimation();

    public abstract void BeginAnimation();

    public abstract void EndAnimation();

    public abstract void UpdateMorphAnimation();

    public abstract void UpdateNodeAnimation(bool afterPhysicsAnim);

    public abstract void ResetPhysics();

    public abstract void UpdatePhysicsAnimation(float elapsed);

    public abstract void Update();

    public abstract void Destroy();

    public abstract void Dispose();

    public void UpdateAllAnimation(VmdAnimation animation, float vmdFrame, float physicsElapsed)
    {
        animation.Evaluate(vmdFrame);

        UpdateMorphAnimation();

        UpdateNodeAnimation(false);

        UpdatePhysicsAnimation(physicsElapsed);

        UpdateNodeAnimation(true);
    }

    public void SaveBaseAnimation()
    {
        foreach (MMDNode node in GetNodes())
        {
            node.SaveBaseAnimation();
        }

        foreach (MMDMorph morph in GetMorphs())
        {
            morph.SaveBaseAnimation();
        }

        foreach (MMDIkSolver ikSolver in GetIkSolvers())
        {
            ikSolver.SaveBaseAnimation();
        }
    }

    public void LoadBaseAnimation()
    {
        foreach (MMDNode node in GetNodes())
        {
            node.LoadBaseAnimation();
        }

        foreach (MMDMorph morph in GetMorphs())
        {
            morph.LoadBaseAnimation();
        }

        foreach (MMDIkSolver ikSolver in GetIkSolvers())
        {
            ikSolver.LoadBaseAnimation();
        }
    }

    public void ClearBaseAnimation()
    {
        foreach (MMDNode node in GetNodes())
        {
            node.ClearBaseAnimation();
        }

        foreach (MMDMorph morph in GetMorphs())
        {
            morph.ClearBaseAnimation();
        }

        foreach (MMDIkSolver ikSolver in GetIkSolvers())
        {
            ikSolver.ClearBaseAnimation();
        }
    }
}