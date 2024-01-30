using Microsoft.Xna.Framework;
using Project1.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.Physics
{
    internal struct Collision
    {
        public ContainmentType Containment;
        public Vector3 PositionWorld;
        public Vector3 TargetRelative;
        public Vector3 ContactRelative;
        public Vector3 Normal;
        public float Penetration;

        public override string ToString()
        {
            return $"Collision: {Containment} - at {PositionWorld} pen of {Penetration}";
        }
    }

    internal static class CollisionSolver
    {
        public static void Solve(PrimitivePhysicsComponent target, PrimitivePhysicsComponent contact, out Collision collision)
        {
            collision = default;
            switch (target.RigidBody.RigidBodyType)
            {
                case RigidBodyType.Sphere:
                    switch(contact.RigidBody.RigidBodyType)
                    {
                        case RigidBodyType.Sphere:
                            SolveSphereAndSphere((RigidBodySphere)target.RigidBody, (RigidBodySphere)contact.RigidBody, out collision);
                            break;
                        case RigidBodyType.Plane:
                            SolvePlaneAndSphere((RigidBodySphere)target.RigidBody, (RigidBodyPlane)contact.RigidBody, out collision);
                            break;
                    }
                    break;
            }
        }

        public static void SolvePlaneAndSphere(RigidBodySphere target, RigidBodyPlane contact, out Collision collision)
        {
            collision = default;
            float planeOffset = Vector3.Dot(contact.WorldMatrix.Translation, contact.WorldMatrix.Forward);
            Vector3 planeDirection = contact.WorldMatrix.Backward;
            Vector3 spherePos = target.WorldMatrix.Translation;

            float sphereDistance = Vector3.Dot(spherePos, planeDirection) - target.Radius - planeOffset;

            if (sphereDistance > 0 || sphereDistance < -target.Radius)
                return;

            collision.Normal = planeDirection;

            collision.ContactRelative = Vector3.Zero;
            collision.TargetRelative = -collision.Normal * target.Radius;

            collision.PositionWorld = spherePos - planeDirection * (sphereDistance + target.Radius);
            collision.Penetration = -sphereDistance;
            collision.Containment = ContainmentType.Intersects;
        }

        public static void SolveSphereAndSphere(RigidBodySphere target, RigidBodySphere contact, out Collision collision)
        {
            collision = default;
            Vector3 midLine = target.WorldMatrix.Translation - contact.WorldMatrix.Translation;
            float midDistance = midLine.Length();
            if (midDistance <= 0 || midDistance >= contact.Radius + target.Radius)
                return;

            collision.Normal = midLine * (1 / midDistance);

            collision.ContactRelative = collision.Normal * contact.Radius;
            collision.TargetRelative = -collision.Normal * target.Radius;

            collision.PositionWorld = target.WorldMatrix.Translation + collision.TargetRelative;
            collision.Penetration = (target.Radius + contact.Radius - midDistance);
            collision.Containment = collision.Penetration > contact.Radius ? ContainmentType.Contains : ContainmentType.Intersects;
        }

    }
}
