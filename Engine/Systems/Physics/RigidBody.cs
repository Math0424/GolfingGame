using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.Physics
{

    internal enum RigidBodyType
    {
        Sphere,
        Capsule,
        Cylinder,
        Box,
        Plane,
    }

    internal abstract class RigidBody
    {
        public static RigidBody Sphere => new RigidBodySphere();
        public static RigidBody Box => new RigidBodyBox();
        public static RigidBody Plane => new RigidBodyPlane();

        //public static readonly PhysicsBody Capsule = new SpherePhysics();
        //public static readonly PhysicsBody Cylinder = new SpherePhysics();

        public RigidBodyFlags RigidBodyFlags { get; protected set; }
        public RigidBodyType RigidBodyType { get; protected set; }
        public Matrix WorldMatrix;

        public Vector3 AngularVelocity { get; private set; }
        public float AngularDampening;
        
        public Vector3 LinearVelocity { get; private set; }
        public float LinearDampening;

        public Vector3 Gravity;

        public float Mass
        {
            get => _mass;
            set
            {
                _mass = value;
                InverseMass = _mass == 0 ? 0 : 1 / _mass;
                UpdateTensorInternal();
            }
        }
        protected Matrix _inertiaTensor;
        public float InverseMass { get; protected set; }
        public Matrix InverseInertiaTensor { get; protected set; }
        public float BoundingSphere { get; protected set; }

        private float _mass;

        public RigidBody()
        {
            LinearDampening = 0.5f;
            AngularDampening = 0.5f;
            Gravity = Vector3.Down * 9.8f;
        }

        public void Update(float deltaTime)
        {
            if (RigidBodyFlags != RigidBodyFlags.Dynamic)
                return;

            LinearVelocity += Gravity * deltaTime;

            LinearVelocity *= (float)Math.Pow(1 - LinearDampening, deltaTime);
            AngularVelocity *= (float)Math.Pow(1 - AngularDampening, deltaTime);

            float val = LinearVelocity.Length();
            if (val != 0f)
                LinearVelocity = Vector3.Normalize(LinearVelocity) * Math.Min(val, 20f);

            Matrix newPos = WorldMatrix;
            newPos.Translation += LinearVelocity * deltaTime;

            float angle = AngularVelocity.Length() * deltaTime;
            if (angle > 0.001f)
            {
                Vector3 axis = Vector3.Normalize(AngularVelocity);
                Quaternion rotation;
                Quaternion.CreateFromRotationMatrix(ref newPos, out rotation);
                rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * rotation);

                Matrix rotationMatrix = Matrix.CreateFromQuaternion(rotation);
                WorldMatrix = rotationMatrix;
                WorldMatrix.Translation = newPos.Translation;
            }
            else
                WorldMatrix = newPos;
        }

        public void ApplyForce(Vector3 force)
        {
            if (RigidBodyFlags == RigidBodyFlags.Static)
                return;
            LinearVelocity += force * InverseMass;
        }

        public void ApplyTorqueForce(Vector3 torque)
        {
            if (RigidBodyFlags == RigidBodyFlags.Static)
                return;
            AngularVelocity += Vector3.Transform(torque, InverseInertiaTensor);
        }

        public void ApplyImpulse(Vector3 force, Vector3 relativePos)
        {
            if (InverseMass != 0)
            {
                ApplyForce(force);
                ApplyTorqueForce(Vector3.Cross(relativePos, force));
            }
        }

        public void Stop()
        {
            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
        }

        public virtual void Init(Matrix worldMatrix, RigidBodyFlags flags, float radius, float mass)
        {
            RigidBodyFlags = flags;
            if (flags == RigidBodyFlags.Static)
            {
                _mass = 0;
                InverseMass = 0;
                _inertiaTensor = Matrix.Identity;
                InverseInertiaTensor = _inertiaTensor;
            } 
            else
            {
                _mass = mass;
                InverseMass = mass == 0 ? 0 : 1 / _mass;
                UpdateTensorInternal();
            }

            WorldMatrix = worldMatrix;
            BoundingSphere = radius;
        }
        protected abstract void UpdateTensorInternal();
    }

    /// <summary>
    /// Zero mass, no rotation plane
    /// </summary>
    internal class RigidBodyPlane : RigidBody
    {
        public RigidBodyPlane() : base()
        {
            RigidBodyType = RigidBodyType.Plane;
        }

        protected override void UpdateTensorInternal()
        {
            _inertiaTensor = new Matrix(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );
            InverseInertiaTensor = Matrix.Invert(_inertiaTensor);
        }
    }

    /// <summary>
    /// Zero mass, no rotation box
    /// </summary>
    internal class RigidBodyBox : RigidBody
    {
        public RigidBodyBox() : base()
        {
            RigidBodyType = RigidBodyType.Box;
        }

        public Vector3 HalfExtents;

        public override void Init(Matrix worldMatrix, RigidBodyFlags flags, float radius, float mass)
        {
            HalfExtents = new Vector3(worldMatrix.Right.Length(), worldMatrix.Up.Length(), worldMatrix.Forward.Length());
            worldMatrix.Forward /= worldMatrix.Forward.Length();
            worldMatrix.Up /= worldMatrix.Up.Length();
            worldMatrix.Right /= worldMatrix.Right.Length();
            base.Init(worldMatrix, flags, radius, mass);
        }

        protected override void UpdateTensorInternal()
        {
            BoundingSphere = (HalfExtents * 2).LengthSquared();
            Vector3 length = HalfExtents * 2;
            float x = 0.083f * Mass * (length.Y * length.Y + length.Z * length.Z);
            float y = 0.083f * Mass * (length.Z * length.Z + length.X * length.X);
            float z = 0.083f * Mass * (length.Y * length.Y + length.X * length.X);
            _inertiaTensor = new Matrix(
                new Vector4(x, 0, 0, 0),
                new Vector4(0, y, 0, 0),
                new Vector4(0, 0, z, 0),
                new Vector4(0, 0, 0, 1)
            );
            InverseInertiaTensor = Matrix.Invert(_inertiaTensor);
        }
    }

    internal class RigidBodySphere : RigidBody
    {
        public float Radius;

        public RigidBodySphere() : base()
        {
            RigidBodyType = RigidBodyType.Sphere;
        }

        public override void Init(Matrix worldMatrix, RigidBodyFlags flags, float radius, float mass)
        {
            Radius = radius;
            worldMatrix.Forward /= worldMatrix.Forward.Length();
            worldMatrix.Up /= worldMatrix.Up.Length();
            worldMatrix.Right /= worldMatrix.Right.Length();
            base.Init(worldMatrix, flags, radius, mass);
        }

        protected override void UpdateTensorInternal()
        {
            float val = 0.4f * Mass * Radius * Radius;
            _inertiaTensor = new Matrix(
                new Vector4(val, 0, 0, 0),
                new Vector4(0, val, 0, 0),
                new Vector4(0, 0, val, 0),
                new Vector4(0, 0, 0,   1)
            );
            InverseInertiaTensor = Matrix.Invert(_inertiaTensor);
        }
    }

}
