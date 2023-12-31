﻿using BulletSharp;
using Evergine.Mathematics;

namespace Saba;

public class MMDPhysics : IDisposable
{
    private readonly BroadphaseInterface _broadphase;
    private readonly DefaultCollisionConfiguration _collisionConfig;
    private readonly CollisionDispatcher _dispatcher;
    private readonly SequentialImpulseConstraintSolver _solver;
    private readonly CollisionShape _groundShape;
    private readonly MotionState _groundMS;
    private readonly RigidBody _groundRB;
    private readonly OverlapFilterCallback _filterCallback;

    public float FPS { get; set; }

    public int MaxSubStepCount { get; set; }

    public DiscreteDynamicsWorld DynamicsWorld { get; }

    public MMDPhysics()
    {
        FPS = 120.0f;
        MaxSubStepCount = 10;

        _broadphase = new DbvtBroadphase();
        _collisionConfig = new DefaultCollisionConfiguration();
        _dispatcher = new CollisionDispatcher(_collisionConfig);

        _solver = new SequentialImpulseConstraintSolver();

        DynamicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConfig)
        {
            Gravity = new Vector3(0.0f, -9.8f * 10.0f, 0.0f)
        };

        _groundShape = new StaticPlaneShape(Vector3.UnitY, 0.0f);

        _groundMS = new DefaultMotionState(Matrix4x4.Identity);

        RigidBodyConstructionInfo groundInfo = new(0.0f, _groundMS, _groundShape, Vector3.Zero);
        _groundRB = new RigidBody(groundInfo);

        DynamicsWorld.AddRigidBody(_groundRB);

        DynamicsWorld.PairCache.SetOverlapFilterCallback(_filterCallback = new MMDFilterCallback(_groundRB.BroadphaseProxy));
    }

    public void Update(float time)
    {
        DynamicsWorld.StepSimulation(time, MaxSubStepCount, 1.0f / FPS);
    }

    public void AddRigidBody(MMDRigidBody rigidBody)
    {
        DynamicsWorld.AddRigidBody(rigidBody.RigidBody, 1 << rigidBody.Group, rigidBody.GroupMask);
    }

    public void RemoveRigidBody(MMDRigidBody rigidBody)
    {
        DynamicsWorld.RemoveRigidBody(rigidBody.RigidBody);
    }

    public void AddJoint(MMDJoint joint)
    {
        DynamicsWorld.AddConstraint(joint.Constraint);
    }

    public void RemoveJoint(MMDJoint joint)
    {
        DynamicsWorld.RemoveConstraint(joint.Constraint);
    }

    public void Dispose()
    {
        _broadphase.Dispose();
        _collisionConfig.Dispose();
        _dispatcher.Dispose();
        _solver.Dispose();
        _groundShape.Dispose();
        _groundMS.Dispose();
        _groundRB.Dispose();
        _filterCallback.Dispose();

        DynamicsWorld.Dispose();

        GC.SuppressFinalize(this);
    }
}