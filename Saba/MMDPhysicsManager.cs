namespace Saba;

public class MMDPhysicsManager : IDisposable
{
    private readonly List<MMDRigidBody> _rigidBodies;
    private readonly List<MMDJoint> _joints;

    public MMDPhysics Physics { get; }

    public MMDRigidBody[] RigidBodies => [.. _rigidBodies];

    public MMDJoint[] Joints => [.. _joints];

    public MMDPhysicsManager()
    {
        Physics = new MMDPhysics();
        _rigidBodies = [];
        _joints = [];
    }

    public void AddRigidBody(MMDRigidBody rigidBody)
    {
        Physics.AddRigidBody(rigidBody);

        _rigidBodies.Add(rigidBody);
    }

    public void RemoveRigidBody(MMDRigidBody rigidBody)
    {
        Physics.RemoveRigidBody(rigidBody);

        _rigidBodies.Remove(rigidBody);
    }

    public void AddJoint(MMDJoint joint)
    {
        Physics.AddJoint(joint);

        _joints.Add(joint);
    }

    public void RemoveJoint(MMDJoint joint)
    {
        Physics.RemoveJoint(joint);

        _joints.Remove(joint);
    }

    public void Dispose()
    {
        _rigidBodies.Clear();
        _joints.Clear();

        Physics.Dispose();

        GC.SuppressFinalize(this);
    }
}
