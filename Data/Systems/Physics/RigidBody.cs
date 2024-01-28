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
        public static RigidBody Sphere => new SpherePhysics();
        //public static readonly PhysicsBody Capsule = new SpherePhysics();
        //public static readonly PhysicsBody Cylinder = new SpherePhysics();
        //public static readonly PhysicsBody Box = new SpherePhysics();

        public static RigidBody Plane => new SpherePhysics();

        public RigidBodyType RigidBodyType { get; protected set; }
        public Matrix WorldMatrix;

        public float Mass { get; protected set; }
        public float InverseMass { get; protected set; }
        public Matrix InertiaTensor { get; protected set; }
        public Matrix InverseInertiaTensor { get; protected set; }

        public abstract void Init(Entity ent);
        public abstract void UpdateTensor(float mass);
    }

    internal class SpherePhysics : RigidBody
    {
        public float Radius;

        public SpherePhysics()
        {
            RigidBodyType = RigidBodyType.Sphere;
        }

        public override void Init(Entity ent)
        {
            WorldMatrix = ent.Position.WorldMatrix;
            var mesh = ent.GetComponent<MeshComponent>();
            if (mesh != null)
                Radius = mesh.Model.LooseBoundingSphereRadius;
            else
                Radius = 1;
            Radius = 1;
            UpdateTensor(1);
        }

        public override void UpdateTensor(float mass)
        {
            Mass = mass;
            InverseMass = 1 / Mass;
            float val = 0.4f * mass * Radius * Radius;
            InertiaTensor = new Matrix(
                new Vector4(val, 0, 0, 0),
                new Vector4(0, val, 0, 0),
                new Vector4(0, 0, val, 0),
                new Vector4(0, 0, 0, 0)
            );
            InverseInertiaTensor = Matrix.Invert(InertiaTensor);
        }
    }
}
