using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems;
using Project1.Engine.Systems.GUI;
using Project1.Engine.Systems.Physics;
using Project1.MyGame;
using System;
using System.Collections.Generic;

namespace Project1
{
    public class GolfGame : Game
    {
        private World _world;

        public GolfGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _world = new World(this, "Level1")
                .AddSystem<Camera>()
                .AddSystem<RenderingSystem>()
                .AddSystem<PhysicsSystem>()
                .AddSystem<HudSystem>();
        }

        protected override void LoadContent()
        {
            var player = _world.CreateEntity()
                .AddComponent(new PositionComponent(Vector3.Up * 20))
                .AddComponent(new MeshComponent("models/sphere"))
                .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic))
                .AddComponent(new GolfBallComponent());
            player.Position.SetLocalMatrix(Matrix.CreateScale(.75f));

            _world.AddSystem<GolfingSystem>();
            _world.GetSystem<GolfingSystem>().SetPlayer(player);

            //_world.CreateEntity()
            //    .AddComponent(new PositionComponent(Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateScale(100)))
            //    .AddComponent(new BillboardComponent("textures/shrimp", BillboardOption.EntityFacing))
            //    .AddComponent(new PhysicsComponent(PhysicsBody.Plane, RigidBodyFlags.Static));

            Random r = new Random();
            for (int i = 2; i <= 120; i++)
            {
                PrimitivePhysicsComponent comp = new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic);
                var ent = _world.CreateEntity()
                    .AddComponent(new PositionComponent(Vector3.Up * 20 + (Vector3.Up * 2 * i) + new Vector3((float)r.NextDouble() * 10, 0, (float)r.NextDouble() * 10)))
                    .AddComponent(new MeshComponent("models/sphere"))
                    .AddComponent(comp);
                ent.Position.SetLocalMatrix(Matrix.CreateScale(.75f));

                //var ent2 = _world.CreateEntity()
                //    .AddComponent(new PositionComponent(Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(ent.Position.WorldMatrix.Translation + (Vector3.Down * 2 * i))))
                //    .AddComponent(new MeshComponent("models/sphere"))
                //    .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Static));
                //ent2.Position.SetLocalMatrix(Matrix.CreateScale(.8f));

                //comp.LinearVelocity = Vector3.Down * 5;
                comp.AngularVelocity = Vector3.Forward * 5;
            }

            //var plane = _world.CreateEntity()
            //        .AddComponent(new PositionComponent(Matrix.CreateRotationX(-MathHelper.PiOver2 + 0.25f)))
            //        .AddComponent(new BillboardComponent("textures/shrimp", BillboardOption.EntityFacing))
            //        .AddComponent(new PrimitivePhysicsComponent(RigidBody.Plane, RigidBodyFlags.Static, 100));
            //plane.Position.SetLocalMatrix(Matrix.CreateScale(100f));

            var plane = _world.CreateEntity()
                    .AddComponent(new PositionComponent(Matrix.CreateRotationX(-MathHelper.PiOver2)))
                    .AddComponent(new BillboardComponent("textures/shrimp", BillboardOption.EntityFacing))
                    .AddComponent(new PrimitivePhysicsComponent(RigidBody.Plane, RigidBodyFlags.Static, 100));
            plane.Position.SetLocalMatrix(Matrix.CreateScale(100f));
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.IsKeyDown(Keys.Escape))
                Exit();

            _world.Update(gameTime);
            Input.UpdateState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _world.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}