using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class MMDIkSolver
{
    #region Enums
    private enum SolveAxis
    {
        X,
        Y,
        Z
    }
    #endregion

    #region Classes
    private class IkChain
    {
        public MMDNode Node { get; }

        public bool EnableAxisLimit { get; set; }

        public Vector3D<float> LimitMax { get; set; }

        public Vector3D<float> LimitMin { get; set; }

        public Vector3D<float> PrevAngle { get; set; }

        public Quaternion<float> SaveIkRot { get; set; }

        public float PlaneModeAngle { get; set; }

        public IkChain(MMDNode node)
        {
            Node = node;
        }
    }
    #endregion

    private readonly List<IkChain> _chains;

    public MMDNode? IkNode { get; set; }

    public MMDNode? IkTarget { get; set; }

    public uint IterateCount { get; set; }

    public float LimitAngle { get; set; }

    public bool Enable { get; set; }

    public bool BaseAnimEnable { get; set; }

    public MMDIkSolver()
    {
        _chains = new List<IkChain>();

        IterateCount = 1;
        LimitAngle = MathHelper.TwoPi;
        Enable = true;
        BaseAnimEnable = true;
    }

    public void AddIkChain(MMDNode node, bool isKnee)
    {
        Vector3D<float> limixMin = Vector3D<float>.Zero;
        Vector3D<float> limitMax = Vector3D<float>.Zero;

        if (isKnee)
        {
            limixMin = new Vector3D<float>(MathHelper.DegreesToRadians(0.5f), 0.0f, 0.0f);
            limitMax = new Vector3D<float>(MathHelper.DegreesToRadians(180.0f), 0.0f, 0.0f);
        }

        AddIkChain(node, isKnee, limixMin, limitMax);
    }

    public void AddIkChain(MMDNode node, bool axisLimit, Vector3D<float> limixMin, Vector3D<float> limitMax)
    {
        IkChain chain = new(node)
        {
            EnableAxisLimit = axisLimit,
            LimitMin = limixMin,
            LimitMax = limitMax,
            SaveIkRot = Quaternion<float>.Identity
        };

        _chains.Add(chain);
    }

    public void Solve()
    {
        if (!Enable)
        {
            return;
        }

        if (IkNode == null || IkTarget == null)
        {
            return;
        }

        foreach (IkChain chain in _chains)
        {
            chain.PrevAngle = Vector3D<float>.Zero;
            chain.Node.IkRotate = Quaternion<float>.Identity;
            chain.PlaneModeAngle = 0.0f;

            chain.Node.UpdateLocalTransform();
            chain.Node.UpdateGlobalTransform();
        }

        float maxDist = float.MaxValue;
        for (uint i = 0; i < IterateCount; i++)
        {
            SolveCore(i);

            Vector3D<float> targetPos = IkTarget.Global.Row4.ToVector3D();
            Vector3D<float> ikPos = IkNode.Global.Row4.ToVector3D();
            float dist = (targetPos - ikPos).Length;
            if (dist < maxDist)
            {
                maxDist = dist;
                foreach (IkChain chain in _chains)
                {
                    chain.SaveIkRot = chain.Node.IkRotate;
                }
            }
            else
            {
                foreach (IkChain chain in _chains)
                {
                    chain.Node.IkRotate = chain.SaveIkRot;

                    chain.Node.UpdateLocalTransform();
                    chain.Node.UpdateGlobalTransform();
                }
                break;
            }
        }
    }

    private void SolveCore(uint iteration)
    {

    }

    private void SolvePlane(uint iteration, int chainIdx, SolveAxis solveAxis)
    {

    }
}