using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Data.Components;
using Project1.Data.Systems.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    // General concepts taken from
    // https://theswissbay.ch/pdf/Gentoomen%20Library/Game%20Development/Programming/Game%20Physics%20Engine%20Development.pdf
    // https://graphics.stanford.edu/courses/cs448b-00-winter/papers/phys_model.pdf
    // http://www.r-5.org/files/books/computers/algo-list/realtime-3d/Christer_Ericson-Real-Time_Collision_Detection-EN.pdf
    // https://www.cs.cmu.edu/%7Ebaraff/sigcourse/notesd2.pdf#page=17

    internal class PhysicsSystem : SystemComponent
    {
        private World _world;
        private double _accumulatedTime;
        private const float physicsTimeStep = 0.001f;
        private Game _game;

        private bool _debugMode;

        public PhysicsSystem(World world, RenderingSystem render, Game game)
        {
            _game = game;
            _world = world;
            _debugMode = false;
            render.DebugDraw += DebugDraw;
        }

        public void DebugDraw(SpriteBatch batch, GraphicsDevice graphics, Camera cam)
        {
            if (_debugMode)
            {
                var physicsObjects = _world.GetEntityComponents<PrimitivePhysicsComponent>();
                if (physicsObjects == null)
                    return;

                foreach (var physicObject in physicsObjects)
                    physicObject.DebugDraw(ref batch, ref graphics, ref cam);
            }
        }

        public override void Update(GameTime delta)
        {
            if (Input.IsNewKeyDown(Keys.F12))
                _debugMode = !_debugMode;

            var physicsObjects = _world.GetEntityComponents<PrimitivePhysicsComponent>();
            if (physicsObjects == null)
                return;

            _accumulatedTime += delta.ElapsedGameTime.TotalSeconds;
            while(_accumulatedTime >= physicsTimeStep)
            {
                foreach (var x in physicsObjects)
                    x.Update(physicsTimeStep);

                // TODO fix this
                // naive approach - use oct tree in the future
                foreach (var target in physicsObjects)
                {
                    if (target.RigidBodyFlag == RigidBodyFlags.Static || target.RigidBodyFlag == RigidBodyFlags.Kinematic)
                        continue;
                    
                    foreach (var contact in physicsObjects)
                    {
                        if (target != contact)
                        {
                            Collision col;
                            CollisionSolver.Solve(target, contact, out col);
                            if (col.Containment == ContainmentType.Intersects)
                            {
                                Vector3 relativeVel = contact.LinearVelocity - target.LinearVelocity;
                                float elasticity = 0.8f;
                                float impuseMag = -(1 + elasticity) * Vector3.Dot(relativeVel, col.Normal) / (target.RigidBody.InverseMass + contact.RigidBody.InverseMass);

                                Vector3 velocityImpulse = col.Normal * impuseMag;

                                Vector3 relPos1 = Vector3.Transform(contact.RigidBody.WorldMatrix.Translation - target.RigidBody.WorldMatrix.Translation, contact.RigidBody.InverseInertiaTensor);
                                Vector3 relPos2 = Vector3.Transform(target.RigidBody.WorldMatrix.Translation - contact.RigidBody.WorldMatrix.Translation, target.RigidBody.InverseInertiaTensor);

                                Vector3 angularImpulse1 = Vector3.Cross(relPos1, velocityImpulse);
                                Vector3 angularImpulse2 = Vector3.Cross(relPos2, -velocityImpulse);

                                contact.AddForce(velocityImpulse, angularImpulse1);
                                target.AddForce(-velocityImpulse, angularImpulse2);
                            }
                        }
                    }
                }
                _accumulatedTime -= physicsTimeStep;
            }

            foreach (var x in physicsObjects)
                x.UpdateWorldMatrix();

        }
    }
}
