using BulletSharp;

namespace Saba;

public class MMDFilterCallback : OverlapFilterCallback
{
    private readonly BroadphaseProxy _floor;

    public MMDFilterCallback(BroadphaseProxy floor)
    {
        _floor = floor;
    }

    public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
    {
        if (proxy1 == null)
        {
            return false;
        }

        if (proxy0 == _floor || proxy1 == _floor)
        {
            return true;
        }

        bool collides1 = (proxy0.CollisionFilterGroup & proxy1.CollisionFilterMask) != 0;
        bool collides2 = (proxy1.CollisionFilterGroup & proxy0.CollisionFilterMask) != 0;

        return collides1 && collides2;
    }
}
