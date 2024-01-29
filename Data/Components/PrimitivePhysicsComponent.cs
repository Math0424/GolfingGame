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
        public RigidBodyFlags RigidBodyFlag
        {
            get => _rigidBodyFlags;
            set => UpdateRigidBodyFlag(value);
        }

        public Vector3 Acceleration { get; private set; }
        private Vector3 _prevPosition;

        public Vector3 AngularVelocity;
        public float AngularDampening;

        public Vector3 LinearVelocity;
        public float LinearDampening;

        public Vector3 CenterOfMassWorld;

        public int PhysicsLayer;
        public bool IsActive;

        public Vector3 Gravity;
        public float Restitution;
        public float StaticFriction;
        public float DynamicFriction;


        private RigidBodyFlags _rigidBodyFlags;

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
            IsActive = true;
            RigidBody = collider;
            LinearDampening = 0.001f;
            AngularDampening = 0.05f;
            Restitution = .05f;
            Gravity = Vector3.Down * 9.8f;
            StaticFriction = 0.25f;
            DynamicFriction = 0.15f;
            UpdateRigidBodyFlag(flags);
        }

        public void UpdateRigidBodyFlag(RigidBodyFlags flags)
        {
            _rigidBodyFlags = flags;
            if (flags == RigidBodyFlags.Static)
                RigidBody.UpdateTensor(0);
        }

        public override void Initalize()
        {
            RigidBody.Init(_entity);
            Stop();
        }

        public void Update(float deltaTime)
        {
            if (RigidBodyFlag == RigidBodyFlags.Static)
            {
                Stop();
                return;
            }

            _prevPosition = RigidBody.WorldMatrix.Translation;

            LinearVelocity += Gravity * deltaTime;

            Matrix newPos = RigidBody.WorldMatrix;
            newPos.Translation += (LinearVelocity * deltaTime);

            float angle = AngularVelocity.Length() * deltaTime;
            if (angle > 0.001f)
            {
                Vector3 axis = Vector3.Normalize(AngularVelocity);
                Quaternion rotation;
                Quaternion.CreateFromRotationMatrix(ref newPos, out rotation);
                rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * rotation);

                Matrix rotationMatrix = Matrix.CreateFromQuaternion(rotation);
                RigidBody.WorldMatrix = rotationMatrix;
                RigidBody.WorldMatrix.Translation = newPos.Translation;
            }
            else
                RigidBody.WorldMatrix = newPos;

            LinearVelocity *= (1 - LinearDampening * deltaTime);
            AngularVelocity *= (1 - AngularDampening * deltaTime);

            float val = LinearVelocity.Length();
            if (val != 0f)
                LinearVelocity = Vector3.Normalize(LinearVelocity) * Math.Min(val, 20f);

            Acceleration = _prevPosition - RigidBody.WorldMatrix.Translation;
        }

        public void UpdateWorldMatrix()
        {
            _entity.Position.SetWorldMatrix(RigidBody.WorldMatrix);
        }

        public Vector3 VelocityAtPoint(Vector3 point)
        {
            return LinearVelocity + Vector3.Cross(AngularVelocity, point - CenterOfMassWorld);
        }

        public void AddTorque(Vector3 torque)
        {
            AngularVelocity += Vector3.Transform(torque, RigidBody.InverseInertiaTensor);
        }

        public void AddForce(Vector3 force)
        {
            LinearVelocity += force * RigidBody.InverseMass;
        }

        public void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;

            DrawingUtils.DrawMatrix(graphics, pos.WorldMatrix);

            DrawingUtils.DrawLine(graphics, pos.Position, Acceleration, Color.Salmon);
            DrawingUtils.DrawLine(graphics, pos.Position, LinearVelocity, Color.Orange);
            DrawingUtils.DrawLine(graphics, pos.Position, AngularVelocity, Color.Pink);
        }

        public void Stop()
        {
            _prevPosition = RigidBody.WorldMatrix.Translation;
            AngularVelocity = Vector3.Zero;
            LinearVelocity = Vector3.Zero;
        }

    }
}
