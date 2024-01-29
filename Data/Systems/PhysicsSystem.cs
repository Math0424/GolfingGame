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
    // http://www.chrishecker.com/images/b/bb/Gdmphys4.pdf

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
                    if (target.RigidBodyFlag == RigidBodyFlags.Static || target.RigidBodyFlag == RigidBodyFlags.Kinematic || !target.IsActive)
                        continue;
                    
                    foreach (var contact in physicsObjects)
                    {
                        if (target != contact && Vector3.Distance(target.RigidBody.WorldMatrix.Translation, contact.RigidBody.WorldMatrix.Translation) <= target.RigidBody.BoundingSphere + contact.RigidBody.BoundingSphere)
                        {
                            Collision col;
                            CollisionSolver.Solve(target, contact, out col);
                            if (col.Containment == ContainmentType.Intersects)
                            {
                                Vector3 rc = contact.RigidBody.WorldMatrix.Translation - col.PositionWorld;
                                Vector3 rt = target.RigidBody.WorldMatrix.Translation - col.PositionWorld;

                                Vector3 velDueToRotContact = Vector3.Cross(contact.AngularVelocity, rc);
                                Vector3 velDueToRotTarget = Vector3.Cross(target.AngularVelocity, rt);
                                
                                Vector3 relativeVel = (contact.LinearVelocity + velDueToRotContact) - (target.LinearVelocity + velDueToRotTarget);

                                float contactMag = Vector3.Dot(relativeVel, col.Normal);
                                if (contactMag < 0f)
                                    continue;
                                target.RigidBody.WorldMatrix.Translation += col.Normal * col.Penetration;

                                float e = Math.Min(contact.Restitution, target.Restitution);
                                Matrix invc = contact.RigidBody.InverseInertiaTensor;
                                Matrix invt = target.RigidBody.InverseInertiaTensor;

                                Vector3 angularVelChangec = Vector3.Cross(rc, col.Normal);
                                angularVelChangec = Vector3.Transform(angularVelChangec, invc);
                                angularVelChangec = Vector3.Cross(angularVelChangec, rc);
                                float denominator = contact.RigidBody.InverseMass;// + Vector3.Dot(angularVelChangec, col.Normal);

                                Vector3 angularVelChanget = Vector3.Cross(rt, col.Normal);
                                angularVelChanget = Vector3.Transform(angularVelChanget, invt);
                                angularVelChanget = Vector3.Cross(angularVelChanget, rt);
                                denominator += target.RigidBody.InverseMass;// + Vector3.Dot(angularVelChanget, col.Normal);

                                float Jmod = (-(1 + e) * contactMag) / denominator;
                                Vector3 J = col.Normal * Jmod;

                                contact.AddForce(J);
                                target.AddForce(-J);

                                // contact.AddTorque(angularVelChangea);
                                // target.AddTorque(angularVelChangeb);

                                //Vector3 normalImpulse = col.Normal * JMag;


                                // Friction code

                                // Vector3 tangent = relativeVel - Vector3.Dot(relativeVel, col.Normal) * col.Normal;
                                // if (tangent.LengthSquared() > 0.001f)
                                //     tangent.Normalize();
                                // 
                                // float staticFriction = ((target.StaticFriction + contact.StaticFriction) * 0.5f);
                                // float dynamicFriction = ((target.DynamicFriction + contact.DynamicFriction) * 0.5f);
                                // 
                                // float frictionImpulseMag = -Vector3.Dot(relativeVel, tangent);
                                // frictionImpulseMag /= (target.RigidBody.InverseMass + contact.RigidBody.InverseMass);
                                // 
                                // Vector3 frictionImpulse;
                                // if (Math.Abs(frictionImpulseMag) < JMag * staticFriction)
                                //     frictionImpulse = -tangent * frictionImpulseMag;
                                // else
                                //     frictionImpulse = tangent * JMag * dynamicFriction;

                                // End Friction code


                                // Vector3 totalImpulse = normalImpulse; //+ frictionImpulse;
                                // 
                                // contact.AddForce(totalImpulse);
                                // target.AddForce(-totalImpulse);
                                // 
                                // // Vector3 relPos1 = contact.RigidBody.WorldMatrix.Translation - target.RigidBody.WorldMatrix.Translation;
                                // // Vector3 relPos2 = target.RigidBody.WorldMatrix.Translation - contact.RigidBody.WorldMatrix.Translation;
                                // 
                                // Vector3 angularImpulse1 = Vector3.Cross(ra, totalImpulse);
                                // Vector3 angularImpulse2 = Vector3.Cross(rb, -totalImpulse);
                                // 
                                // contact.AddTorque(angularImpulse1);
                                // target.AddTorque(angularImpulse2);
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
