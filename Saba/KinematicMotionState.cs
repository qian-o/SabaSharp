using Evergine.Mathematics;
using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class KinematicMotionState : MMDMotionState
{
    private readonly MMDNode _node;
    private readonly Matrix4X4<float> _offset;

    public KinematicMotionState(MMDNode node, Matrix4X4<float> offset)
    {
        _node = node;
        _offset = offset;
    }

    public override void GetWorldTransform(out Matrix4x4 worldTrans)
    {
        Matrix4X4<float> m = _node != null ? _offset * _node.Global : _offset;

        worldTrans = m.InvZ().ToBtTransform();
    }

    public override void ReflectGlobalTransform()
    {

    }

    public override void Reset()
    {

    }

    public override void SetWorldTransform(ref Matrix4x4 worldTrans)
    {

    }
}
