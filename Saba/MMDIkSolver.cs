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

    public void AddIkChain(MMDNode node, bool isKnee = false)
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
        Vector3D<float> ikPos = IkNode!.Global.Row4.ToVector3D();

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

            Vector3D<float> targetPos = IkTarget!.Global.Row4.ToVector3D();

            Matrix4X4<float> invChain = chain.Node.Global.Invert();

            Vector3D<float> chainIkPos = Vector3D.Transform(ikPos, invChain);
            Vector3D<float> chainTargetPos = Vector3D.Transform(targetPos, invChain);

            Vector3D<float> chainIkVec = Vector3D.Normalize(chainIkPos);
            Vector3D<float> chainTargetVec = Vector3D.Normalize(chainTargetPos);

            float dot = Vector3D.Dot(chainTargetVec, chainIkVec);
            dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

            float angle = MathHelper.Acos(dot);
            float angleDeg = MathHelper.RadiansToDegrees(angle);
            if (angleDeg < 0.001)
            {
                continue;
            }
            angle = MathHelper.Clamp(angle, -LimitAngle, LimitAngle);
            Vector3D<float> cross = Vector3D.Normalize(Vector3D.Cross(chainTargetVec, chainIkVec));
            Quaternion<float> rot = Quaternion<float>.CreateFromAxisAngle(cross, angle);

            Quaternion<float> chainRot = chainNode.IkRotate * chainNode.AnimateRotate * rot;
            if (chain.EnableAxisLimit)
            {
                // 未完成（待定）
                Matrix3X3<float> chainRotM = Matrix3X3.CreateFromQuaternion(chainRot);
                Matrix3X3.Decompose(chainRotM, out _, out _);
            }

            Quaternion<float> ikRot = chainRot * Quaternion<float>.Inverse(chainNode.AnimateRotate);
            chainNode.IkRotate = ikRot;

            chainNode.UpdateLocalTransform();
            chainNode.UpdateGlobalTransform();
        }
    }

    private void SolvePlane(uint iteration, int chainIdx, SolveAxis solveAxis)
    {
        int rotateAxisIndex = 0;
        Vector3D<float> rotateAxis = Vector3D<float>.UnitX;
        Vector3D<float> plane = new(0.0f, 1.0f, 1.0f);
        switch (solveAxis)
        {
            case SolveAxis.X:
                rotateAxisIndex = 0;
                rotateAxis = Vector3D<float>.UnitX;
                plane = new Vector3D<float>(0.0f, 1.0f, 1.0f);
                break;
            case SolveAxis.Y:
                rotateAxisIndex = 1;
                rotateAxis = Vector3D<float>.UnitY;
                plane = new Vector3D<float>(1.0f, 0.0f, 1.0f);
                break;
            case SolveAxis.Z:
                rotateAxisIndex = 2;
                rotateAxis = Vector3D<float>.UnitZ;
                plane = new Vector3D<float>(1.0f, 1.0f, 0.0f);
                break;
            default: break;
        }

        IkChain chain = _chains[chainIdx];
        Vector3D<float> ikPos = IkNode!.Global.Row4.ToVector3D();

        Vector3D<float> targetPos = IkTarget!.Global.Row4.ToVector3D();

        Matrix4X4<float> invChain = chain.Node.Global.Invert();

        Vector3D<float> chainIkPos = Vector3D.Transform(ikPos, invChain);
        Vector3D<float> chainTargetPos = Vector3D.Transform(targetPos, invChain);

        Vector3D<float> chainIkVec = Vector3D.Normalize(chainIkPos);
        Vector3D<float> chainTargetVec = Vector3D.Normalize(chainTargetPos);

        float dot = Vector3D.Dot(chainTargetVec, chainIkVec);
        dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

        float angle = MathHelper.Acos(dot);

        angle = MathHelper.Clamp(angle, -LimitAngle, LimitAngle);

        Quaternion<float> rot1 = Quaternion<float>.CreateFromAxisAngle(rotateAxis, angle);
        Vector3D<float> targetVec1 = Vector3D.Transform(chainTargetVec, rot1);
        float dot1 = Vector3D.Dot(targetVec1, chainIkVec);

        Quaternion<float> rot2 = Quaternion<float>.CreateFromAxisAngle(rotateAxis, -angle);
        Vector3D<float> targetVec2 = Vector3D.Transform(chainTargetVec, rot2);
        float dot2 = Vector3D.Dot(targetVec2, chainIkVec);

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

        Quaternion<float> ikRotM = Quaternion<float>.CreateFromAxisAngle(rotateAxis, newAngle) * Quaternion<float>.Inverse(chain.Node.AnimateRotate);
        chain.Node.IkRotate = ikRotM;

        chain.Node.UpdateLocalTransform();
        chain.Node.UpdateGlobalTransform();
    }
}