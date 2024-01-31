﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Project1.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1.Engine.Systems.Physics;
using Project1.Engine.Systems;
using Project1.Engine.Systems.RenderMessages;

namespace Project1.Engine.Components
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
        public Vector3 _deltaPos;
        private Vector3 _prevPosition;

        public Vector3 AngularVelocity;
        public float AngularDampening;

        public Vector3 LinearVelocity;
        public float LinearDampening;

        public Vector3 CenterOfMassWorld;

        public int PhysicsLayer;
        public bool IsSleeping { get; private set; }

        public Vector3 Gravity;
        public float Restitution;
        public float StaticFriction;
        public float DynamicFriction;

        private float _userRadius;
        private RigidBodyFlags _userFlags;

        private float[] _velocities;
        private int velocityPos;

        public RigidBody RigidBody { get; private set; }
        // bool IsMoving
        // float LinearSpeed
        // float AngularSpeed

        /// <summary>
        /// Entity collided with (int)[entity] at (Vector3)[position]
        /// </summary>
        public Action<int, Vector3> Collision;

        public PrimitivePhysicsComponent(RigidBody collider, RigidBodyFlags flags, float radius = -1)
        {
            RigidBody = collider;
            LinearDampening = 0.6f;
            AngularDampening = 0.5f;
            Restitution = .5f;
            Gravity = Vector3.Down * 9.8f;
            StaticFriction = 0.8f;
            DynamicFriction = 0.4f;
            _userFlags = flags;
            _userRadius = radius;
            _velocities = new float[100];
            Wake();
        }

        public override void Initalize()
        {
            if (_userRadius != -1)
                RigidBody.Init(_entity.Position.WorldMatrix, _userFlags, _userRadius, 1);
            else
                RigidBody.Init(_entity.Position.WorldMatrix, _userFlags, 1, 1);
        }

        public void Wake()
        {
            for(int i = 0; i < _velocities.Length; i++)
                _velocities[i] = 1;
        }

        public void Update(float deltaTime)
        {
            if (RigidBody.RigidBodyFlags == RigidBodyFlags.Static)
            {
                Stop();
                return;
            }

            _velocities[velocityPos++ % _velocities.Length] = _deltaPos.LengthSquared();
            if (_velocities.Sum() < 0.00001f)
            {
                IsSleeping = true;
                Stop();
                return;
            }
            IsSleeping = false;

            _deltaPos = _prevPosition - RigidBody.WorldMatrix.Translation;
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

            LinearVelocity = LinearVelocity - (LinearVelocity * LinearDampening * deltaTime);
            AngularVelocity = AngularVelocity - (AngularVelocity * AngularDampening * deltaTime);

            float val = LinearVelocity.Length();
            if (val != 0f)
                LinearVelocity = Vector3.Normalize(LinearVelocity) * Math.Min(val, 20f);

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
            if (IsSleeping)
                Wake();
        }

        public void AddForce(Vector3 force)
        {
            LinearVelocity += force * RigidBody.InverseMass;
            if (IsSleeping)
                Wake();
        }

        public void DebugDraw(RenderingSystem render)
        {
            var pos = _entity.Position;

            DrawingUtils.DrawMatrix(render, pos.WorldMatrix);

            switch(RigidBody.RigidBodyType)
            {
                case RigidBodyType.Box:
                    Matrix mat = Matrix.CreateScale(((RigidBodyBox)RigidBody).HalfExtents);
                    mat = mat * RigidBody.WorldMatrix;
                    render.EnqueueMessage(new RenderMessageDrawBox(mat));
                    break;
            }

            if (RigidBody.RigidBodyFlags != RigidBodyFlags.Static)
            {
                DrawingUtils.DrawLine(render, pos.Position, LinearVelocity, Color.Orange);
                DrawingUtils.DrawLine(render, pos.Position, AngularVelocity, Color.Pink);
                DrawingUtils.DrawWorldText(render, $"AngV: {Math.Round(AngularVelocity.Length(), 2)}\nLinV: {Math.Round(LinearVelocity.Length(), 2)}", pos.Position, IsSleeping ? Color.Blue : Color.Orange);
            }
        }

        public void Stop()
        {
            _prevPosition = RigidBody.WorldMatrix.Translation;
            AngularVelocity = Vector3.Zero;
            LinearVelocity = Vector3.Zero;
        }

    }
}
