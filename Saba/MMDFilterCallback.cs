using BulletSharp;

namespace Saba;

public class MMDFilterCallback : OverlapFilterCallback
{
    public List<BroadphaseProxy> NonFilterProxy { get; } = new();

    public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
    {
        if (proxy0 == null || proxy1 == null)
        {
            return false;
        }

        if (NonFilterProxy.Any(item => item.Uid == proxy0.Uid) || NonFilterProxy.Any(item => item.Uid == proxy1.Uid))
        {
            return true;
        }

        return (proxy0.CollisionFilterGroup & proxy1.CollisionFilterMask) != 0 &&
                   (proxy1.CollisionFilterGroup & proxy0.CollisionFilterMask) != 0;
    }
}
