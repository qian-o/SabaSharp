using Saba.Helpers;
using System.Numerics;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;

namespace Saba;

public class KinematicMotionState(MMDNode node, Matrix4x4 offset) : MMDMotionState
{
    public override void GetWorldTransform(out BtMatrix4x4 worldTrans)
    {
        Matrix4x4 m = node != null ? offset * node.Global : offset;

        worldTrans = m.InvZ().ToBtMatrix4x4();
    }

    public override void ReflectGlobalTransform()
    {

    }

    public override void Reset()
    {

    }

    public override void SetWorldTransform(ref BtMatrix4x4 worldTrans)
    {

    }
}
