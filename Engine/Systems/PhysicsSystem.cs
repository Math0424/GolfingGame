using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Engine.Components;
using Project1.Engine.Systems.Physics;
using Project1.Engine.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems
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

        private bool _debugMode;
        private RenderingSystem _render;

        public PhysicsSystem(World world, RenderingSystem render)
        {
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
                // broad phase and narrow phase
                foreach (var t in physicsObjects)
                {
                    if (t.PhysicsLayer == PhysicsLayer.Trigger || 
                        t.RigidBody.RigidBodyFlags != RigidBodyFlags.Dynamic || 
                        t.IsSleeping)
                        continue;
                    
                    foreach (var c in physicsObjects)
                    {
                        if (t != c) //&& Vector3.Distance(target.RigidBody.WorldMatrix.Translation, contact.RigidBody.WorldMatrix.Translation) <= target.RigidBody.BoundingSphere + contact.RigidBody.BoundingSphere)
                        {
                            Collision col;
                            CollisionSolver.Solve(t, c, out col);
                            if (col.Containment == ContainmentType.Intersects)
                            {
                                t.Collision?.Invoke(c.EntityId, col.PositionWorld);
                                c.Collision?.Invoke(t.EntityId, col.PositionWorld);
                                if (c.PhysicsLayer == PhysicsLayer.Trigger)
                                    continue;

                                Vector3 rc = col.ContactRelative;
                                Vector3 rt = col.TargetRelative;

                                var contact = c.RigidBody;
                                var target = t.RigidBody;

                                // _render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, col.PositionWorld, Color.Pink));
                                // //_render.EnqueueMessage(new RenderMessageDrawLine(Vector3.Zero, col.Normal, Color.Orange));
                                // _render.EnqueueMessage(new RenderMessageDrawLine(contact.RigidBody.WorldMatrix.Translation, contact.RigidBody.WorldMatrix.Translation + rc, Color.Red)); // up
                                // _render.EnqueueMessage(new RenderMessageDrawLine(target.RigidBody.WorldMatrix.Translation, target.RigidBody.WorldMatrix.Translation + rt, Color.Green)); //down

                                // target sphere
                                // contant plane

                                Vector3 velDueToRotContact = Vector3.Cross(contact.AngularVelocity, rc);
                                Vector3 velDueToRotTarget = Vector3.Cross(target.AngularVelocity, rt);
                                Vector3 surfaceVel = (contact.LinearVelocity + velDueToRotContact) - (target.LinearVelocity + velDueToRotTarget);

                                float contactMag = Vector3.Dot(surfaceVel, col.Normal);
                                if (contactMag < 0f)
                                    continue;
                                target.WorldMatrix.Translation += col.Normal * col.Penetration;

                                float e = Math.Min(c.Restitution, t.Restitution);
                                Matrix invc = contact.InverseInertiaTensor;
                                Matrix invt = target.InverseInertiaTensor;

                                Vector3 angularVelChanget = Vector3.Cross(rt, col.Normal);
                                angularVelChanget = Vector3.Transform(angularVelChanget, invt);
                                Vector3 vRt = Vector3.Cross(angularVelChanget, rt);
                                float denominator = target.InverseMass + Vector3.Dot(vRt, col.Normal);

                                float Jmod;
                                if (contact.RigidBodyFlags == RigidBodyFlags.Dynamic)
                                {
                                    Vector3 angularVelChangec = Vector3.Cross(rc, col.Normal);
                                    angularVelChangec = Vector3.Transform(angularVelChangec, invc);
                                    Vector3 vRc = Vector3.Cross(angularVelChangec, rc);
                                    denominator += contact.InverseMass + Vector3.Dot(vRc, col.Normal);

                                    Jmod = -(1 + e) * contactMag / denominator;
                                    Vector3 J = col.Normal * Jmod;

                                    c.AddForce(J);
                                    t.AddForce(-J);
                                    c.AddTorque(angularVelChangec);
                                    t.AddTorque(angularVelChanget);
                                } 
                                else
                                {
                                    Jmod = -(1 + e) * contactMag / denominator;
                                    Vector3 J = col.Normal * Jmod;
                                    t.AddForce(-J);
                                    t.AddTorque(angularVelChanget);
                                }


                                // Friction code

                                velDueToRotContact = Vector3.Cross(contact.AngularVelocity, rc);
                                velDueToRotTarget = Vector3.Cross(target.AngularVelocity, rt);
                                surfaceVel = (contact.LinearVelocity + velDueToRotContact) - (target.LinearVelocity + velDueToRotTarget);
                                
                                Vector3 tangent = Vector3.Cross(surfaceVel, col.Normal);
                                Vector3 tangent2 = Vector3.Cross(tangent, col.Normal);
                                if (surfaceVel.LengthSquared() > 0.01f)
                                {
                                    tangent.Normalize();
                                    tangent2.Normalize();
                                
                                    Vector3 frictionVector = tangent * Vector3.Dot(surfaceVel, tangent) + tangent2 * Vector3.Dot(surfaceVel, tangent2);

                                    if (Vector3.Dot(surfaceVel, col.Normal) > -0.1f)
                                        frictionVector *= ((t.DynamicFriction + c.DynamicFriction) * 0.5f);
                                    else
                                        frictionVector *= ((t.StaticFriction + c.StaticFriction) * 0.5f);

                                    if (frictionVector.LengthSquared() > .0001f)
                                        t.AddImpulse(frictionVector, rt);
                                }

                                // End Friction code
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


    /*
     * 
Vector3 fangularVelChangec = Vector3.Cross(rc, p1);
fangularVelChangec = Vector3.Transform(fangularVelChangec, invc);
Vector3 fvRc = Vector3.Cross(fangularVelChangec, rc);
float fdenominator = contact.InverseMass + Vector3.Dot(fvRc, p1);
                                
Vector3 fangularVelChanget = Vector3.Cross(rt, p1);
fangularVelChanget = Vector3.Transform(fangularVelChanget, invt);
Vector3 fvRt = Vector3.Cross(fangularVelChanget, rt);
fdenominator += target.InverseMass + Vector3.Dot(fvRt, p1);
float Jfmod = -Vector3.Dot(relVel, p1) / fdenominator;
                                
Vector3 frictionImpulse;
if (Jfmod >= Jmod * staticFriction)
    frictionImpulse = p1 * Jfmod;
else
    frictionImpulse = p1 * -Jmod * dynamicFriction;

fangularVelChangec = Vector3.Cross(rc, p2);
fangularVelChangec = Vector3.Transform(fangularVelChangec, invc);
fvRc = Vector3.Cross(fangularVelChangec, rc);
fdenominator = contact.InverseMass + Vector3.Dot(fvRc, p2);

fangularVelChanget = Vector3.Cross(rt, p2);
fangularVelChanget = Vector3.Transform(fangularVelChanget, invt);
fvRt = Vector3.Cross(fangularVelChanget, rt);
fdenominator += target.InverseMass + Vector3.Dot(fvRt, p2);
Jfmod = -Vector3.Dot(relVel, p2) / fdenominator;

if (Jfmod >= Jmod * staticFriction)
    frictionImpulse += p2 * Jfmod;
else
    frictionImpulse += p2 * -Jmod * dynamicFriction;

c.AddImpulse(frictionImpulse, rc);
t.AddImpulse(-frictionImpulse, rt);
     */
}
