using Microsoft.Xna.Framework;
using Project1.Data.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.Physics
{
    internal struct Collision
    {
        public ContainmentType Containment;
        public Vector3 PositionWorld;
        public Vector3 PositionRelative;
        public Vector3 Normal;
        public float Penetration;

        public override string ToString()
        {
            return $"Collision: {Containment} - at {PositionWorld} pen of {Penetration}";
        }
    }

    internal static class CollisionSolver
    {
        public static void Solve(PrimitivePhysicsComponent object1, PrimitivePhysicsComponent object2, out Collision collision)
        {
            collision = default;
            switch (object1.RigidBody.RigidBodyType)
            {
                case RigidBodyType.Sphere:
                    switch(object2.RigidBody.RigidBodyType)
                    {
                        case RigidBodyType.Sphere:
                            SolveSphereAndSphere((SpherePhysics)object1.RigidBody, (SpherePhysics)object2.RigidBody, out collision);
                            break;
                        case RigidBodyType.Plane:
                            SolvePlaneAndSphere((PlanePhysics)object2.RigidBody, (SpherePhysics)object1.RigidBody, out collision);
                            break;
                    }
                    break;
            }
        }

        public static void SolvePlaneAndSphere(PlanePhysics plane, SpherePhysics sphere, out Collision collision)
        {
            collision = default;
            float planeOffset = Vector3.Dot(plane.WorldMatrix.Translation, plane.WorldMatrix.Forward);
            Vector3 planeDirection = plane.WorldMatrix.Backward;
            Vector3 spherePos = sphere.WorldMatrix.Translation;

            float sphereDistance = Vector3.Dot(spherePos, planeDirection) - sphere.Radius - planeOffset;

            if (sphereDistance > 0 || sphereDistance < -sphere.Radius)
                return;

            collision.Normal = planeDirection;
            collision.PositionRelative = spherePos - planeDirection * (sphereDistance + sphere.Radius);
            collision.PositionWorld = plane.WorldMatrix.Translation - collision.PositionRelative;
            collision.Penetration = -sphereDistance;
            collision.Containment = ContainmentType.Intersects;
        }

        public static void SolveSphereAndSphere(SpherePhysics sphere1, SpherePhysics sphere2, out Collision collision)
        {
            collision = default;
            Vector3 midLine = sphere1.WorldMatrix.Translation - sphere2.WorldMatrix.Translation;
            float midDistance = midLine.Length();
            if (midDistance <= 0 || midDistance >= sphere2.Radius + sphere2.Radius)
                return;

            collision.Normal = midLine * (1 / midDistance);
            collision.PositionRelative = midLine * .5f;
            collision.PositionWorld = sphere1.WorldMatrix.Translation + collision.PositionRelative;
            collision.Penetration = (sphere1.Radius + sphere2.Radius - midDistance);
            collision.Containment = collision.Penetration > sphere2.Radius ? ContainmentType.Contains : ContainmentType.Intersects;
        }

    }
}
