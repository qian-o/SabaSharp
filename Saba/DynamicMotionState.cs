using Evergine.Mathematics;
using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class DynamicMotionState : MMDMotionState
{
    private readonly MMDNode _node;
    private readonly Matrix4X4<float> _offset;
    private readonly bool _override;
    private readonly Matrix4X4<float> _invOffset;

    private Matrix4x4 transform;

    public DynamicMotionState(MMDNode node, Matrix4X4<float> offset, bool @override = true)
    {
        _node = node;
        _offset = offset;
        _override = @override;
        _invOffset = offset.Invert();

        Reset();
    }

    public override void GetWorldTransform(out Matrix4x4 worldTrans)
    {
        worldTrans = transform;
    }

    public override void ReflectGlobalTransform()
    {
        Matrix4X4<float> world = transform.ToMatrix4X4();
        Matrix4X4<float> btGlobal = _invOffset * world.InvZ();

        if (_override)
        {
            _node.Global = btGlobal;

            _node.UpdateChildTransform();
        }
    }

    public override void Reset()
    {
        Matrix4X4<float> global = (_offset * _node.Global).InvZ();

        transform = global.ToBtTransform();
    }

    public override void SetWorldTransform(ref Matrix4x4 worldTrans)
    {
        transform = worldTrans;
    }
}
