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

    internal class PrimitiveComponent : EntityComponent
    {
        public Vector3 AngularMomentum;
        public float AngularDampening;

        private Vector3 _prevAngularMomentum;

        public Vector3 LinearMomentum;
        public float LinearDampening;

        private Vector3 _prevLinearMomentum;

        public Vector3 CenterOfMassWorld;

        public RigidBodyFlags RigidBodyFlag;
        public int PhysicsLayer;
        public bool IsActive;

        private Vector3 _impulseForce;
        private Vector3 _impulseTorque;

        public RigidBody Collider { get; private set; }
        // bool IsMoving
        // float LinearSpeed
        // float AngularSpeed

        /// <summary>
        /// Entity collided with (int)[entity] at (Vector3)[position]
        /// </summary>
        public Action<int, Vector3> Collision;

        public PrimitiveComponent(RigidBody collider, RigidBodyFlags flags)
        {
            RigidBodyFlag = flags;
            Collider = collider;
            LinearDampening = 0.99999f;
            AngularDampening = 0.999f;
        }

        public override void Initalize()
        {
            Collider.Init(_entity);
            Stop();
        }

        public void Update(float deltaTime)
        {
            Matrix newPos = Collider.WorldMatrix;
            switch (RigidBodyFlag)
            {
                case RigidBodyFlags.Static:
                    break;
                case RigidBodyFlags.Dynamic:
                    newPos += Matrix.Multiply(AngularMomentum.CrossMatrix(), deltaTime) * newPos;
                    newPos.Translation += deltaTime * LinearMomentum * LinearDampening;
                    break;
                case RigidBodyFlags.Kinematic:
                    break;
            }
            _prevAngularMomentum = AngularMomentum;
            _prevLinearMomentum = LinearMomentum;
            LinearMomentum *= LinearDampening;
            AngularMomentum *= AngularDampening;

            Collider.WorldMatrix = newPos;
        }

        public void UpdateWorldMatrix()
        {
            _entity.Position.SetWorldMatrix(Collider.WorldMatrix);
        }

        public Vector3 VelocityAtPoint(Vector3 point)
        {
            return LinearMomentum + Vector3.Cross(AngularMomentum, point - CenterOfMassWorld);
        }

        public void ApplyImpulse(Vector3 force, Vector3 position)
        {
            _impulseForce += force * Collider.InverseMass;
            _impulseTorque += Vector3.Transform(Vector3.Cross(force, position), Collider.InverseInertiaTensor);
        }

        public void AddForce(Vector3 force, Vector3 torque)
        {
            _impulseForce += force * Collider.InverseMass;
            _impulseTorque += Vector3.Transform(torque, Collider.InverseInertiaTensor);
        }

        public void AddForce(Vector3 force)
        {
            _impulseForce += force * Collider.InverseMass;
        }

        public void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;

            DrawingUtils.DrawMatrix(graphics, pos.WorldMatrix);

            DrawingUtils.DrawLine(graphics, pos.Position, LinearMomentum, Color.Orange);
            DrawingUtils.DrawLine(graphics, pos.Position, AngularMomentum, Color.Pink);
        }

        public void Stop()
        {
            _impulseForce = Vector3.Zero;
            _impulseTorque = Vector3.Zero;
            AngularMomentum = Vector3.Zero;
            _prevAngularMomentum = Vector3.Zero;
            LinearMomentum = Vector3.Zero;
            _prevLinearMomentum = Vector3.Zero;
        }

    }
}
