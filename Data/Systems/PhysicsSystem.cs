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
    // http://www.r-5.org/files/books/computers/algo-list/realtime-3d/Christer_Ericson-Real-Time_Collision_Detection-EN.pdf

    internal class PhysicsSystem : SystemComponent
    {
        private World _world;
        private double _accumulatedTime;
        private const float physicsTimeStep = 0.005f;
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
                // naive approach - use trees in the future
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
                                //Console.WriteLine($"collision detected {col.Penetration}");
                                target.RigidBody.WorldMatrix.Translation += col.Normal * col.Penetration;

                                Vector3 relativeVel = contact.LinearVelocity - target.LinearVelocity;

                                if (Vector3.Dot(relativeVel, col.Normal) > 0f)
                                    continue;

                                float elasticity = Math.Min(contact.Restitution, target.Restitution);
                                float impulseMag = -(1f + elasticity) * Vector3.Dot(relativeVel, col.Normal);
                                impulseMag /= (target.RigidBody.InverseMass + contact.RigidBody.InverseMass);

                                Vector3 velocityImpulse = col.Normal * impulseMag;

                                contact.AddForce(velocityImpulse);
                                target.AddForce(-velocityImpulse);

                                Vector3 relPos1 = contact.RigidBody.WorldMatrix.Translation - target.RigidBody.WorldMatrix.Translation;
                                Vector3 relPos2 = target.RigidBody.WorldMatrix.Translation - contact.RigidBody.WorldMatrix.Translation;
                                
                                Vector3 angularImpulse1 = Vector3.Cross(relPos1, velocityImpulse);
                                Vector3 angularImpulse2 = Vector3.Cross(relPos2, -velocityImpulse);
                                
                                contact.AddTorque(angularImpulse1);
                                target.AddTorque(angularImpulse2);
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
