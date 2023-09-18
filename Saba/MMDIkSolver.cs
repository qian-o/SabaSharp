using Saba.Helpers;
using System.Numerics;

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

        public Vector3 LimitMax { get; set; }

        public Vector3 LimitMin { get; set; }

        public Vector3 PrevAngle { get; set; }

        public Quaternion SaveIkRot { get; set; }

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

    public void AddIkChain(MMDNode node, bool isKnee = false)
    {
        Vector3 limixMin = Vector3.Zero;
        Vector3 limitMax = Vector3.Zero;

        if (isKnee)
        {
            limixMin = new Vector3(MathHelper.DegreesToRadians(0.5f), 0.0f, 0.0f);
            limitMax = new Vector3(MathHelper.DegreesToRadians(180.0f), 0.0f, 0.0f);
        }

        AddIkChain(node, isKnee, limixMin, limitMax);
    }

    public void AddIkChain(MMDNode node, bool axisLimit, Vector3 limixMin, Vector3 limitMax)
    {
        IkChain chain = new(node)
        {
            EnableAxisLimit = axisLimit,
            LimitMin = limixMin,
            LimitMax = limitMax,
            SaveIkRot = Quaternion.Identity
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
            chain.PrevAngle = Vector3.Zero;
            chain.Node.IkRotate = Quaternion.Identity;
            chain.PlaneModeAngle = 0.0f;

            chain.Node.UpdateLocalTransform();
            chain.Node.UpdateGlobalTransform();
        }

        float maxDist = float.MaxValue;
        for (uint i = 0; i < IterateCount; i++)
        {
            SolveCore(i);

            Vector3 targetPos = IkTarget.Global.GetRow(4).ToVector3();
            Vector3 ikPos = IkNode.Global.GetRow(4).ToVector3();
            float dist = (targetPos - ikPos).Length();
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

    public void SaveBaseAnimation()
    {
        BaseAnimEnable = Enable;
    }

    public void LoadBaseAnimation()
    {
        Enable = BaseAnimEnable;
    }

    public void ClearBaseAnimation()
    {
        BaseAnimEnable = true;
    }

    private void SolveCore(uint iteration)
    {
        Vector3 ikPos = IkNode!.Global.GetRow(4).ToVector3();

        for (int i = 0; i < _chains.Count; i++)
        {
            IkChain chain = _chains[i];
            MMDNode chainNode = chain.Node;

            if (chainNode == IkTarget)
            {
                continue;
            }

            if (chain.EnableAxisLimit)
            {
                if ((chain.LimitMin.X != 0.0f || chain.LimitMax.X != 0.0f)
                    && (chain.LimitMin.Y == 0.0f || chain.LimitMax.Y == 0.0f)
                    && (chain.LimitMin.Z == 0.0f || chain.LimitMax.Z == 0.0f))
                {
                    SolvePlane(iteration, i, SolveAxis.X);

                    continue;
                }
                else if ((chain.LimitMin.Y != 0.0f || chain.LimitMax.Y != 0.0f)
                         && (chain.LimitMin.Z == 0.0f || chain.LimitMax.Z == 0.0f)
                         && (chain.LimitMin.X == 0.0f || chain.LimitMax.X == 0.0f))
                {
                    SolvePlane(iteration, i, SolveAxis.Y);

                    continue;
                }
                else if ((chain.LimitMin.Z != 0.0f || chain.LimitMax.Z != 0.0f)
                         && (chain.LimitMin.X == 0.0f || chain.LimitMax.X == 0.0f)
                         && (chain.LimitMin.Y == 0.0f || chain.LimitMax.Y == 0.0f))
                {
                    SolvePlane(iteration, i, SolveAxis.Z);

                    continue;
                }
            }

            Vector3 targetPos = IkTarget!.Global.GetRow(4).ToVector3();

            Matrix4x4 invChain = chain.Node.Global.Invert();

            Vector3 chainIkPos = Vector3.Transform(ikPos, invChain);
            Vector3 chainTargetPos = Vector3.Transform(targetPos, invChain);

            Vector3 chainIkVec = Vector3.Normalize(chainIkPos);
            Vector3 chainTargetVec = Vector3.Normalize(chainTargetPos);

            float dot = Vector3.Dot(chainTargetVec, chainIkVec);
            dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

            float angle = MathHelper.Acos(dot);
            float angleDeg = MathHelper.RadiansToDegrees(angle);
            if (angleDeg < 0.001)
            {
                continue;
            }
            angle = MathHelper.Clamp(angle, -LimitAngle, LimitAngle);
            Vector3 cross = Vector3.Normalize(Vector3.Cross(chainTargetVec, chainIkVec));
            Quaternion rot = Quaternion.CreateFromAxisAngle(cross, angle);

            Quaternion chainRot = chainNode.IkRotate * chainNode.AnimateRotate * rot;
            if (chain.EnableAxisLimit)
            {
                // 未完成（待定）
            }

            Quaternion ikRot = chainRot * Quaternion.Inverse(chainNode.AnimateRotate);
            chainNode.IkRotate = ikRot;

            chainNode.UpdateLocalTransform();
            chainNode.UpdateGlobalTransform();
        }
    }

    private void SolvePlane(uint iteration, int chainIdx, SolveAxis solveAxis)
    {
        int rotateAxisIndex = 0;
        Vector3 rotateAxis = Vector3.UnitX;
        switch (solveAxis)
        {
            case SolveAxis.X:
                rotateAxisIndex = 0;
                rotateAxis = Vector3.UnitX;
                break;
            case SolveAxis.Y:
                rotateAxisIndex = 1;
                rotateAxis = Vector3.UnitY;
                break;
            case SolveAxis.Z:
                rotateAxisIndex = 2;
                rotateAxis = Vector3.UnitZ;
                break;
            default: break;
        }

        IkChain chain = _chains[chainIdx];
        Vector3 ikPos = IkNode!.Global.GetRow(4).ToVector3();

        Vector3 targetPos = IkTarget!.Global.GetRow(4).ToVector3();

        Matrix4x4 invChain = chain.Node.Global.Invert();

        Vector3 chainIkPos = Vector3.Transform(ikPos, invChain);
        Vector3 chainTargetPos = Vector3.Transform(targetPos, invChain);

        Vector3 chainIkVec = Vector3.Normalize(chainIkPos);
        Vector3 chainTargetVec = Vector3.Normalize(chainTargetPos);

        float dot = Vector3.Dot(chainTargetVec, chainIkVec);
        dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

        float angle = MathHelper.Acos(dot);

        angle = MathHelper.Clamp(angle, -LimitAngle, LimitAngle);

        Quaternion rot1 = Quaternion.CreateFromAxisAngle(rotateAxis, angle);
        Vector3 targetVec1 = Vector3.Transform(chainTargetVec, rot1);
        float dot1 = Vector3.Dot(targetVec1, chainIkVec);

        Quaternion rot2 = Quaternion.CreateFromAxisAngle(rotateAxis, -angle);
        Vector3 targetVec2 = Vector3.Transform(chainTargetVec, rot2);
        float dot2 = Vector3.Dot(targetVec2, chainIkVec);

        float newAngle = chain.PlaneModeAngle;
        if (dot1 > dot2)
        {
            newAngle += angle;
        }
        else
        {
            newAngle -= angle;
        }

        if (iteration == 0)
        {
            if (newAngle < chain.LimitMin[rotateAxisIndex] || newAngle > chain.LimitMax[rotateAxisIndex])
            {
                if (-newAngle > chain.LimitMin[rotateAxisIndex] && -newAngle < chain.LimitMax[rotateAxisIndex])
                {
                    newAngle *= -1.0f;
                }
                else
                {
                    float halfRad = (chain.LimitMin[rotateAxisIndex] + chain.LimitMax[rotateAxisIndex]) * 0.5f;
                    if (MathHelper.Abs(halfRad - newAngle) > MathHelper.Abs(halfRad + newAngle))
                    {
                        newAngle *= -1.0f;
                    }
                }
            }
        }

        newAngle = MathHelper.Clamp(newAngle, chain.LimitMin[rotateAxisIndex], chain.LimitMax[rotateAxisIndex]);
        chain.PlaneModeAngle = newAngle;

        Quaternion ikRotM = Quaternion.CreateFromAxisAngle(rotateAxis, newAngle) * Quaternion.Inverse(chain.Node.AnimateRotate);
        chain.Node.IkRotate = ikRotM;

        chain.Node.UpdateLocalTransform();
        chain.Node.UpdateGlobalTransform();
    }
}