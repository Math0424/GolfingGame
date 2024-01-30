using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Data.Components;
using Project1.Data.Systems.Physics;
using Project1.Data.Systems.RenderMessages;
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

        private RenderingSystem _render;

        public PhysicsSystem(World world, RenderingSystem render, Game game)
        {
            _game = game;
            _world = world;
            _debugMode = false;
            _render = render;
            _render.DoDraw += DebugDraw;
        }

        public void DebugDraw()
        {
            if (_debugMode)
            {
                var physicsObjects = _world.GetEntityComponents<PrimitivePhysicsComponent>();
                if (physicsObjects == null)
                    return;

                foreach (var physicObject in physicsObjects)
                    physicObject.DebugDraw(_render);
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
                    if (target.RigidBodyMovementType == RigidBodyFlags.Static || target.RigidBodyMovementType == RigidBodyFlags.Kinematic || target.IsSleeping)
                        continue;
                    
                    foreach (var contact in physicsObjects)
                    {
                        if (target != contact && Vector3.Distance(target.RigidBody.WorldMatrix.Translation, contact.RigidBody.WorldMatrix.Translation) <= target.RigidBody.BoundingSphere + contact.RigidBody.BoundingSphere)
                        {
                            Collision col;
                            CollisionSolver.Solve(target, contact, out col);
                            if (col.Containment == ContainmentType.Intersects)
                            {
                                Vector3 rc = col.ContactRelative;
                                Vector3 rt = col.TargetRelative;

                                // _render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, col.PositionWorld, Color.Pink));
                                // _render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, rc, Color.Red)); // up
                                // //_render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, rt, Color.Green)); //down
                                // _render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, col.Normal, Color.Orange)); //up

                                // target sphere
                                // contant plane

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
                                Vector3 vRc = Vector3.Cross(angularVelChangec, rc);
                                float denominator = contact.RigidBody.InverseMass + Vector3.Dot(vRc, col.Normal);

                                Vector3 angularVelChanget = Vector3.Cross(rt, col.Normal);
                                angularVelChanget = Vector3.Transform(angularVelChanget, invt);
                                Vector3 vRt = Vector3.Cross(angularVelChanget, rt);
                                denominator += target.RigidBody.InverseMass + Vector3.Dot(vRt, col.Normal);

                                float Jmod = -(1 + e) * contactMag / denominator;

                                // Friction code

                                Vector3 tangent = relativeVel - Vector3.Dot(relativeVel, col.Normal) * col.Normal;
                                if (tangent.LengthSquared() > 0.1f)
                                {
                                    tangent.Normalize();

                                    float staticFriction = ((target.StaticFriction + contact.StaticFriction) * 0.5f);
                                    float dynamicFriction = ((target.DynamicFriction + contact.DynamicFriction) * 0.5f);

                                    Vector3 fangularVelChangec = Vector3.Cross(rc, tangent);
                                    fangularVelChangec = Vector3.Transform(fangularVelChangec, invc);
                                    Vector3 fvRc = Vector3.Cross(fangularVelChangec, rc);
                                    float fdenominator = contact.RigidBody.InverseMass + Vector3.Dot(fvRc, tangent);

                                    Vector3 fangularVelChanget = Vector3.Cross(rt, tangent);
                                    fangularVelChanget = Vector3.Transform(fangularVelChanget, invt);
                                    Vector3 fvRt = Vector3.Cross(fangularVelChanget, rt);
                                    fdenominator += target.RigidBody.InverseMass + Vector3.Dot(fvRt, tangent);

                                    float Jfmod = -Vector3.Dot(relativeVel, tangent) / fdenominator;

                                    Vector3 frictionImpulse;
                                    if (Jfmod >= Jmod * staticFriction)
                                        frictionImpulse = tangent * Jfmod;
                                    else
                                        frictionImpulse = tangent * -Jmod * dynamicFriction;

                                    contact.AddForce(frictionImpulse);
                                    target.AddForce(-frictionImpulse);
                                    contact.AddTorque(Vector3.Cross(frictionImpulse, rc));
                                    target.AddTorque(-Vector3.Cross(frictionImpulse, rt));
                                }

                                // End Friction code

                                Vector3 J = col.Normal * Jmod;
                                contact.AddForce(J);
                                target.AddForce(-J);

                                contact.AddTorque(angularVelChangec);
                                target.AddTorque(angularVelChanget);
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
