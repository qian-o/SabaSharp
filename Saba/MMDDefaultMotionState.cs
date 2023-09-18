using Saba.Helpers;
using System.Numerics;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;

namespace Saba;

public class MMDDefaultMotionState : MMDMotionState
{
    private readonly BtMatrix4x4 _initialTransform;

    private BtMatrix4x4 transform;

    public MMDDefaultMotionState(Matrix4x4 transform)
    {
        _initialTransform = transform.ToBtMatrix4x4();
    }

    public override void GetWorldTransform(out BtMatrix4x4 worldTrans)
    {
        worldTrans = transform;
    }

    public override void ReflectGlobalTransform()
    {

    }

    public override void Reset()
    {
        transform = _initialTransform;
    }

    public override void SetWorldTransform(ref BtMatrix4x4 worldTrans)
    {
        transform = worldTrans;
    }
}
