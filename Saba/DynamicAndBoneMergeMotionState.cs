using Saba.Helpers;
using System.Numerics;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;

namespace Saba;

public class DynamicAndBoneMergeMotionState : MMDMotionState
{
    private readonly MMDNode _node;
    private readonly Matrix4x4 _offset;
    private readonly bool _override;
    private readonly Matrix4x4 _invOffset;

    private BtMatrix4x4 transform;

    public DynamicAndBoneMergeMotionState(MMDNode node, Matrix4x4 offset, bool @override = true)
    {
        _node = node;
        _offset = offset;
        _override = @override;
        _invOffset = offset.Invert();

        Reset();
    }

    public override void GetWorldTransform(out BtMatrix4x4 worldTrans)
    {
        worldTrans = transform;
    }

    public override void ReflectGlobalTransform()
    {
        Matrix4x4 world = transform.ToMatrix4x4();
        Matrix4x4 btGlobal = _invOffset * world.InvZ();
        Matrix4x4 global = _node.Global;
        btGlobal.M41 = global.M41;
        btGlobal.M42 = global.M42;
        btGlobal.M43 = global.M43;
        btGlobal.M44 = global.M44;

        if (_override)
        {
            _node.Global = btGlobal;

            _node.UpdateChildTransform();
        }
    }

    public override void Reset()
    {
        Matrix4x4 global = (_offset * _node.Global).InvZ();

        transform = global.ToBtMatrix4x4();
    }

    public override void SetWorldTransform(ref BtMatrix4x4 worldTrans)
    {
        transform = worldTrans;
    }
}
