using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.Physics
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
        //public static readonly PhysicsBody Capsule = new SpherePhysics();
        //public static readonly PhysicsBody Cylinder = new SpherePhysics();
        //public static readonly PhysicsBody Box = new SpherePhysics();

        public static RigidBody Plane => new RigidBodyPlane();

        public RigidBodyType RigidBodyType { get; protected set; }
        public Matrix WorldMatrix;

        public float Mass { get; protected set; }
        public float InverseMass { get; protected set; }
        public Matrix InertiaTensor { get; protected set; }
        public Matrix InverseInertiaTensor { get; protected set; }
        public float BoundingSphere { get; protected set; }

        public virtual void Init(Matrix worldMatrix, float radius, float mass)
        {
            WorldMatrix = worldMatrix;
            Mass = mass;
            InverseMass = mass == 0 ? 0 : 1 / Mass;
            BoundingSphere = radius;

            if (Mass >= 0.001f)
                UpdateTensorInternal();
            else
            {
                InertiaTensor = Matrix.Identity;
                InverseInertiaTensor = InertiaTensor;
            }
        }
        protected abstract void UpdateTensorInternal();
    }

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

    internal class RigidBodySphere : RigidBody
    {
        public float Radius;

        public RigidBodySphere()
        {
            RigidBodyType = RigidBodyType.Sphere;
        }

        public override void Init(Matrix worldMatrix, float radius, float mass)
        {
            Radius = radius;
            base.Init(worldMatrix, radius, mass);
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
