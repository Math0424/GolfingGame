using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Project1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1.Data.Systems.Physics;
using Project1.Data.Systems;

namespace Project1.Data.Components
{
    [Flags]
    public enum RigidBodyFlags
    {
        Static,
        Dynamic,
        Kinematic,
    }

    internal class PrimitivePhysicsComponent : EntityComponent
    {
        public Vector3 AngularVelocity;
        public float AngularDampening;

        private Vector3 _prevAngularMomentum;

        public Vector3 LinearVelocity;
        public float LinearDampening;

        private Vector3 _prevLinearMomentum;

        public Vector3 CenterOfMassWorld;

        public RigidBodyFlags RigidBodyFlag;
        public int PhysicsLayer;
        public bool IsActive;

        public Vector3 Gravity;

        public RigidBody RigidBody { get; private set; }
        // bool IsMoving
        // float LinearSpeed
        // float AngularSpeed

        /// <summary>
        /// Entity collided with (int)[entity] at (Vector3)[position]
        /// </summary>
        public Action<int, Vector3> Collision;

        public PrimitivePhysicsComponent(RigidBody collider, RigidBodyFlags flags)
        {
            RigidBodyFlag = flags;
            RigidBody = collider;
            LinearDampening = 0.99999f;
            AngularDampening = 0.999f;
            Gravity = Vector3.Down;//* 9.8f;
        }

        public override void Initalize()
        {
            RigidBody.Init(_entity);
            Stop();
        }

        public void Update(float deltaTime)
        {
            if (RigidBodyFlag == RigidBodyFlags.Static)
                return;

            Matrix newPos = RigidBody.WorldMatrix;
            newPos.Translation += deltaTime * LinearVelocity * LinearDampening + (Gravity * deltaTime);
            newPos += AngularVelocity.CrossMatrix() * deltaTime * newPos;

            _prevAngularMomentum = AngularVelocity;
            _prevLinearMomentum = LinearVelocity;
            LinearVelocity *= LinearDampening;
            AngularVelocity *= AngularDampening;

            RigidBody.WorldMatrix = newPos;
        }

        public void UpdateWorldMatrix()
        {
            _entity.Position.SetWorldMatrix(RigidBody.WorldMatrix);
        }

        public Vector3 VelocityAtPoint(Vector3 point)
        {
            return LinearVelocity + Vector3.Cross(AngularVelocity, point - CenterOfMassWorld);
        }

        public void AddImpulse(Vector3 force, Vector3 position)
        {
            LinearVelocity += force * RigidBody.InverseMass;
            AngularVelocity += Vector3.Transform(Vector3.Cross(force, position), RigidBody.InverseInertiaTensor);
        }

        public void AddForce(Vector3 force, Vector3 torque)
        {
            LinearVelocity += force;
            AngularVelocity += torque;
        }

        public void AddForce(Vector3 force)
        {
            LinearVelocity += force * RigidBody.InverseMass;
        }

        public void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;

            DrawingUtils.DrawMatrix(graphics, pos.WorldMatrix);

            DrawingUtils.DrawLine(graphics, pos.Position, LinearVelocity, Color.Orange);
            DrawingUtils.DrawLine(graphics, pos.Position, AngularVelocity, Color.Pink);
        }

        public void Stop()
        {
            AngularVelocity = Vector3.Zero;
            _prevAngularMomentum = Vector3.Zero;
            LinearVelocity = Vector3.Zero;
            _prevLinearMomentum = Vector3.Zero;
        }

    }
}
