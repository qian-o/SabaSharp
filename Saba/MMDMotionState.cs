using BulletSharp;

namespace Saba;

public abstract class MMDMotionState : MotionState
{
    public abstract void Reset();

    public abstract void ReflectGlobalTransform();
}
