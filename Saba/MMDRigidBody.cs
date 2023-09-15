using BulletSharp;
using Evergine.Mathematics;
using Saba.Helpers;
using Silk.NET.Maths;
using MathHelper = Saba.Helpers.MathHelper;

namespace Saba;

public class MMDRigidBody : IDisposable
{
    #region Enums
    private enum RigidBodyType
    {
        Kinematic,
        Dynamic,
        Aligned
    };
    #endregion

    private readonly RigidBodyType _rigidBodyType;
    private readonly CollisionShape _shape;
    private readonly MMDMotionState? _activeMotionState;
    private readonly MMDMotionState? _kinematicMotionState;
    private readonly Matrix4X4<float> _offsetMat;

    public MMDNode? Node { get; }

    public string? Name => Node?.Name;

    public ushort Group { get; }

    public ushort GroupMask { get; }

    public RigidBody RigidBody { get; }

    public MMDRigidBody(PmxRigidBody pmxRigidBody, MMDModel model, MMDNode? node)
    {
        _rigidBodyType = (RigidBodyType)pmxRigidBody.Op;

        Node = node;
        Group = pmxRigidBody.Group;
        GroupMask = pmxRigidBody.CollisionGroup;

        switch (pmxRigidBody.Shape)
        {
            case PmxShape.Sphere:
                _shape = new SphereShape(pmxRigidBody.ShapeSize.X);
                break;
            case PmxShape.Box:
                _shape = new BoxShape(pmxRigidBody.ShapeSize.X, pmxRigidBody.ShapeSize.Y, pmxRigidBody.ShapeSize.Z);
                break;
            case PmxShape.Capsule:
                _shape = new CapsuleShape(pmxRigidBody.ShapeSize.X, pmxRigidBody.ShapeSize.Y);
                break;
            default: break;
        }

        if (_shape == null)
        {
            throw new Exception("Failed to create collision shape.");
        }

        float mass = 0.0f;
        Vector3 localInertia = Vector3.Zero;

        if (pmxRigidBody.Op != PmxOperation.Static)
        {
            mass = pmxRigidBody.Mass;
        }

        if (mass != 0)
        {
            _shape.CalculateLocalInertia(mass, out localInertia);
        }

        Matrix4X4<float> rx = Matrix4X4.CreateRotationX(pmxRigidBody.Rotate.X, new Vector3D<float>(1.0f, 0.0f, 0.0f));
        Matrix4X4<float> ry = Matrix4X4.CreateRotationY(pmxRigidBody.Rotate.Y, new Vector3D<float>(0.0f, 1.0f, 0.0f));
        Matrix4X4<float> rz = Matrix4X4.CreateRotationZ(pmxRigidBody.Rotate.Z, new Vector3D<float>(0.0f, 0.0f, 1.0f));
        Matrix4X4<float> rotMat = rz * rx * ry;
        Matrix4X4<float> translateMat = Matrix4X4.CreateTranslation(pmxRigidBody.Translate) * rotMat;

        Matrix4X4<float> rbMat = (rotMat * translateMat).InvZ();

        MMDNode kinematicNode = node ?? model.GetNodes().First();

        _offsetMat = rbMat * kinematicNode.Global.Invert();

        MotionState? motionState;
        if (pmxRigidBody.Op == PmxOperation.Static)
        {
            _kinematicMotionState = new KinematicMotionState(kinematicNode, _offsetMat);

            motionState = _kinematicMotionState;
        }
        else
        {
            if (node != null)
            {
                if (pmxRigidBody.Op == PmxOperation.Dynamic)
                {
                    _activeMotionState = new DynamicMotionState(kinematicNode, _offsetMat);
                    _kinematicMotionState = new KinematicMotionState(kinematicNode, _offsetMat);
                }
                else if (pmxRigidBody.Op == PmxOperation.DynamicAndBoneMerge)
                {
                    _activeMotionState = new DynamicAndBoneMergeMotionState(kinematicNode, _offsetMat);
                    _kinematicMotionState = new KinematicMotionState(kinematicNode, _offsetMat);
                }
            }
            else
            {
                _activeMotionState = new DefaultMotionState(_offsetMat);
                _kinematicMotionState = new KinematicMotionState(kinematicNode, _offsetMat);
            }

            motionState = _activeMotionState;
        }

        RigidBodyConstructionInfo rbInfo = new(mass, motionState, _shape, localInertia)
        {
            LinearDamping = pmxRigidBody.TranslateDimmer,
            AngularDamping = pmxRigidBody.RotateDimmer,
            Restitution = pmxRigidBody.Repulsion,
            Friction = pmxRigidBody.Friction,
            AdditionalDamping = true
        };

        RigidBody = new RigidBody(rbInfo);
        RigidBody.SetSleepingThresholds(0.01f, MathHelper.DegreesToRadians(0.1f));
        RigidBody.ActivationState = ActivationState.DisableDeactivation;

        if (pmxRigidBody.Op == PmxOperation.Static)
        {
            RigidBody.CollisionFlags |= CollisionFlags.KinematicObject;
        }
    }

    public void SetActivation(bool activation)
    {
        if (_rigidBodyType != RigidBodyType.Kinematic)
        {
            if (activation)
            {
                RigidBody.CollisionFlags &= ~CollisionFlags.KinematicObject;
                RigidBody.MotionState = _activeMotionState;
            }
            else
            {
                RigidBody.CollisionFlags |= CollisionFlags.KinematicObject;
                RigidBody.MotionState = _kinematicMotionState;
            }
        }
        else
        {
            RigidBody.MotionState = _kinematicMotionState;
        }
    }

    public void ResetTransform()
    {
        _activeMotionState?.Reset();
    }

    public void Reset(MMDPhysics physics)
    {
        // 未完成（待定）
    }

    public void ReflectGlobalTransform()
    {
        _activeMotionState?.ReflectGlobalTransform();

        _kinematicMotionState?.ReflectGlobalTransform();
    }

    public void CalcLocalTransform()
    {
        if (Node != null)
        {
            if (Node.Parent is MMDNode parent)
            {
                Matrix4X4<float> local = Node.Global * parent.Global.Invert();

                Node.Local = local;
            }
            else
            {
                Node.Local = Node.Global;
            }
        }
    }

    public Matrix4X4<float> GetTransform()
    {
        return RigidBody.CenterOfMassTransform.ToMatrix4X4().InvZ();
    }

    public void Dispose()
    {
        _shape.Dispose();
        _activeMotionState?.Dispose();
        _kinematicMotionState?.Dispose();
        RigidBody.Dispose();

        GC.SuppressFinalize(this);
    }
}