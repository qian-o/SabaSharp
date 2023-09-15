using Evergine.Mathematics;
using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public class DefaultMotionState : MMDMotionState
{
    private readonly Matrix4x4 _initialTransform;

    private Matrix4x4 transform;

    public DefaultMotionState(Matrix4X4<float> transform)
    {
        _initialTransform = transform.ToBtTransform();
    }

    public override void GetWorldTransform(out Matrix4x4 worldTrans)
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

    public override void SetWorldTransform(ref Matrix4x4 worldTrans)
    {
        transform = worldTrans;
    }
}
