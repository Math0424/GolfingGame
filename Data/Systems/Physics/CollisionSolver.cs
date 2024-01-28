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
    }

    internal static class CollisionSolver
    {
        public static void Solve(PrimitiveComponent object1, PrimitiveComponent object2, out Collision collision)
        {
            SolveSphereAndSphere((SpherePhysics)object1.Collider, (SpherePhysics)object2.Collider, out collision);
        }

        public static void SolveSphereAndSphere(SpherePhysics sphere1, SpherePhysics sphere2, out Collision collision)
        {
            collision = default;
            Vector3 midLine = sphere1.WorldMatrix.Translation - sphere2.WorldMatrix.Translation;
            float midDistance = midLine.Length();
            if (midDistance <= 0 || midDistance >= sphere2.Radius + sphere2.Radius)
            {
                collision.Containment = ContainmentType.Disjoint;
                return;
            }
            
            collision.Normal = midLine * (1 / midDistance);
            collision.PositionRelative = midLine * .5f;
            collision.PositionWorld = sphere1.WorldMatrix.Translation + collision.PositionRelative;
            collision.Penetration = (sphere1.Radius + sphere2.Radius - midDistance);
            collision.Containment = collision.Penetration > sphere2.Radius ? ContainmentType.Contains : ContainmentType.Intersects;
        }

    }
}
