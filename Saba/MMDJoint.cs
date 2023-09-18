using BulletSharp;
using Saba.Helpers;
using System.Numerics;
using BtVector3 = Evergine.Mathematics.Vector3;

namespace Saba;

public class MMDJoint : IDisposable
{
    public TypedConstraint Constraint { get; }

    public MMDJoint(PmxJoint pmxJoint, MMDRigidBody rigidBodyA, MMDRigidBody rigidBodyB)
    {
        Matrix4x4 t0 = Matrix4x4.CreateFromYawPitchRoll(pmxJoint.Rotate.Y, pmxJoint.Rotate.X, pmxJoint.Rotate.Z) * Matrix4x4.CreateTranslation(pmxJoint.Translate);

        Matrix4x4 t1 = rigidBodyA.RigidBody.WorldTransform.ToMatrix4x4().Invert();
        Matrix4x4 t2 = rigidBodyB.RigidBody.WorldTransform.ToMatrix4x4().Invert();
        t1 = t0 * t1;
        t2 = t0 * t2;

        Generic6DofSpringConstraint constraint = new(rigidBodyA.RigidBody, rigidBodyB.RigidBody, t1.ToBtMatrix4x4(), t2.ToBtMatrix4x4(), true)
        {
            LinearLowerLimit = new BtVector3(pmxJoint.TranslateLowerLimit.X,
                                             pmxJoint.TranslateLowerLimit.Y,
                                             pmxJoint.TranslateLowerLimit.Z),
            LinearUpperLimit = new BtVector3(pmxJoint.TranslateUpperLimit.X,
                                             pmxJoint.TranslateUpperLimit.Y,
                                             pmxJoint.TranslateUpperLimit.Z),
            AngularLowerLimit = new BtVector3(pmxJoint.RotateLowerLimit.X,
                                              pmxJoint.RotateLowerLimit.Y,
                                              pmxJoint.RotateLowerLimit.Z),
            AngularUpperLimit = new BtVector3(pmxJoint.RotateUpperLimit.X,
                                              pmxJoint.RotateUpperLimit.Y,
                                              pmxJoint.RotateUpperLimit.Z)
        };

        if (pmxJoint.SpringTranslate.X != 0.0f)
        {
            constraint.EnableSpring(0, true);
            constraint.SetStiffness(0, pmxJoint.SpringTranslate.X);
        }

        if (pmxJoint.SpringTranslate.Y != 0.0f)
        {
            constraint.EnableSpring(1, true);
            constraint.SetStiffness(1, pmxJoint.SpringTranslate.Y);
        }

        if (pmxJoint.SpringTranslate.Z != 0.0f)
        {
            constraint.EnableSpring(2, true);
            constraint.SetStiffness(2, pmxJoint.SpringTranslate.Z);
        }

        if (pmxJoint.SpringRotate.X != 0.0f)
        {
            constraint.EnableSpring(3, true);
            constraint.SetStiffness(3, pmxJoint.SpringRotate.X);
        }

        if (pmxJoint.SpringRotate.Y != 0.0f)
        {
            constraint.EnableSpring(4, true);
            constraint.SetStiffness(4, pmxJoint.SpringRotate.Y);
        }

        if (pmxJoint.SpringRotate.Z != 0.0f)
        {
            constraint.EnableSpring(5, true);
            constraint.SetStiffness(5, pmxJoint.SpringRotate.Z);
        }

        Constraint = constraint;
    }

    public void Dispose()
    {
        Constraint.Dispose();

        GC.SuppressFinalize(this);
    }
}
