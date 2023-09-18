using Saba.Helpers;
using System.Numerics;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;

namespace Saba;

public class KinematicMotionState : MMDMotionState
{
    private readonly MMDNode _node;
    private readonly Matrix4x4 _offset;

    public KinematicMotionState(MMDNode node, Matrix4x4 offset)
    {
        _node = node;
        _offset = offset;
    }

    public override void GetWorldTransform(out BtMatrix4x4 worldTrans)
    {
        Matrix4x4 m = _node != null ? _offset * _node.Global : _offset;

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
