using BulletSharp;

namespace Saba;

public class MMDFilterCallback(BroadphaseProxy floor) : OverlapFilterCallback
{
    public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
    {
        if (proxy1 == null)
        {
            return false;
        }

        if (proxy0 == floor || proxy1 == floor)
        {
            return true;
        }

        bool collides1 = (proxy0.CollisionFilterGroup & proxy1.CollisionFilterMask) != 0;
        bool collides2 = (proxy1.CollisionFilterGroup & proxy0.CollisionFilterMask) != 0;

        return collides1 && collides2;
    }
}
