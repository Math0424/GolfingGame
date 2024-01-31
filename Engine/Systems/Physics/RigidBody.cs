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

        public float Mass { get; protected set; }
        public float InverseMass { get; protected set; }
        public Matrix InertiaTensor { get; protected set; }
        public Matrix InverseInertiaTensor { get; protected set; }
        public float BoundingSphere { get; protected set; }

        public virtual void Init(Matrix worldMatrix, RigidBodyFlags flags, float radius, float mass)
        {
            RigidBodyFlags = flags;
            if (flags == RigidBodyFlags.Static)
            {
                Mass = 0;
                InverseMass = 0;
                InertiaTensor = Matrix.Identity;
                InverseInertiaTensor = InertiaTensor;
            } 
            else
            {
                Mass = mass;
                InverseMass = mass == 0 ? 0 : 1 / Mass;
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
        public RigidBodyPlane()
        {
            RigidBodyType = RigidBodyType.Plane;
        }

        protected override void UpdateTensorInternal()
        {
            InertiaTensor = new Matrix(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );
            InverseInertiaTensor = Matrix.Invert(InertiaTensor);
        }
    }

    /// <summary>
    /// Zero mass, no rotation box
    /// </summary>
    internal class RigidBodyBox : RigidBody
    {
        public RigidBodyBox()
        {
            RigidBodyType = RigidBodyType.Box;
        }

        public Vector3 HalfExtents;

        public override void Init(Matrix worldMatrix, RigidBodyFlags flags, float radius, float mass)
        {
            HalfExtents = new Vector3(worldMatrix.Forward.Length(), worldMatrix.Up.Length(), worldMatrix.Right.Length());
            worldMatrix.Forward /= worldMatrix.Forward.Length();
            worldMatrix.Up /= worldMatrix.Up.Length();
            worldMatrix.Right /= worldMatrix.Right.Length();
            base.Init(worldMatrix, flags, radius, mass);
        }

        protected override void UpdateTensorInternal()
        {
            BoundingSphere = HalfExtents.LengthSquared();
            Vector3 length = HalfExtents * 2;
            float x = 0.083f * Mass * (length.Y * length.Y + length.Z * length.Z);
            float y = 0.083f * Mass * (length.Z * length.Z + length.X * length.X);
            float z = 0.083f * Mass * (length.Y * length.Y + length.X * length.X);
            InertiaTensor = new Matrix(
                new Vector4(x, 0, 0, 0),
                new Vector4(0, y, 0, 0),
                new Vector4(0, 0, z, 0),
                new Vector4(0, 0, 0, 1)
            );
            InverseInertiaTensor = Matrix.Invert(InertiaTensor);
        }
    }

    internal class RigidBodySphere : RigidBody
    {
        public float Radius;

        public RigidBodySphere()
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
            InertiaTensor = new Matrix(
                new Vector4(val, 0, 0, 0),
                new Vector4(0, val, 0, 0),
                new Vector4(0, 0, val, 0),
                new Vector4(0, 0, 0,   1)
            );
            InverseInertiaTensor = Matrix.Invert(InertiaTensor);
        }
    }

}
