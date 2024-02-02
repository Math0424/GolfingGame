using Microsoft.Xna.Framework;
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

    public enum PhysicsLayer
    {
        Default,
        Trigger,
    }

    internal class PrimitivePhysicsComponent : EntityComponent
    {
        public Vector3 _deltaPos;
        private Vector3 _prevPosition;

        public PhysicsLayer PhysicsLayer;
        public bool IsEnabled;
        public bool IsSleeping { get; private set; }
        public int EntityId => _entity.Id;

        public float Restitution;
        public float StaticFriction;
        public float DynamicFriction;

        private float _userRadius;
        private float _userMass;
        private RigidBodyFlags _userFlags;

        private float[] _velocities;
        private int velocityPos;

        public RigidBody RigidBody { get; private set; }

        /// <summary>
        /// Entity collided with (int)[entity] at (Vector3)[position]
        /// </summary>
        public Action<int, Vector3> Collision;

        public PrimitivePhysicsComponent(RigidBody collider, RigidBodyFlags flags, float mass = 1, float radius = -1)
        {
            RigidBody = collider;
            Restitution = .5f;
            StaticFriction = 0.04f;
            DynamicFriction = 0.01f;
            _userFlags = flags;
            _userMass = mass;
            _userRadius = radius;
            _velocities = new float[100];
            IsSleeping = true;
            IsEnabled = true;
        }

        public override void Initalize()
        {
            if (_userRadius != -1)
                RigidBody.Init(_entity.Position.WorldMatrix, _userFlags, _userRadius, _userMass);
            else
                RigidBody.Init(_entity.Position.WorldMatrix, _userFlags, 1, _userMass);
            Wake();
        }

        public void Wake()
        {
            if (RigidBody.RigidBodyFlags == RigidBodyFlags.Static)
                return;
            IsSleeping = false;
            for (int i = 0; i < _velocities.Length; i++)
                _velocities[i] = 1;
        }

        public void Update(float deltaTime)
        {
            if (!IsEnabled)
                return;

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

            RigidBody.Update(deltaTime);
        }

        public void UpdateWorldMatrix()
        {
            _entity.Position.SetWorldMatrix(RigidBody.WorldMatrix);
        }

        public Vector3 VelocityAtPoint(Vector3 point)
        {
            return RigidBody.LinearVelocity + Vector3.Cross(RigidBody.AngularVelocity, point - RigidBody.WorldMatrix.Translation);
        }

        public void AddTorque(Vector3 torque)
        {
            RigidBody.ApplyTorqueForce(torque);
            if (IsSleeping)
                Wake();
        }

        public void AddForce(Vector3 force)
        {
            RigidBody.ApplyForce(force);
            if (IsSleeping)
                Wake();
        }

        public void AddImpulse(Vector3 force, Vector3 relPos)
        {
            RigidBody.ApplyImpulse(force, relPos);
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
                case RigidBodyType.Sphere:
                    render.EnqueueMessage(new RenderMessageDrawSphere(RigidBody.WorldMatrix.Translation, ((RigidBodySphere)RigidBody).Radius));
                    break;
            }

            if (RigidBody.RigidBodyFlags != RigidBodyFlags.Static)
            {
                DrawingUtils.DrawLine(render, pos.Position, RigidBody.LinearVelocity, Color.Orange);
                DrawingUtils.DrawLine(render, pos.Position, RigidBody.AngularVelocity, Color.Pink);
                DrawingUtils.DrawWorldText(render, $"ID: {_entity.Id}\n AngV: {Math.Round(RigidBody.AngularVelocity.Length(), 2)}\nLinV: {Math.Round(RigidBody.LinearVelocity.Length(), 2)}", pos.Position, IsSleeping ? Color.Blue : Color.Orange);
            }
            else
            {
                DrawingUtils.DrawWorldText(render, $"ID: {_entity.Id}", pos.Position, Color.Yellow);
            }
        }

        public void Stop()
        {
            _prevPosition = RigidBody.WorldMatrix.Translation;
            RigidBody.Stop();
        }

    }
}
