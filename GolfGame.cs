using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Data;
using Project1.Data.Components;
using Project1.Data.Systems;
using Project1.Data.Systems.GUI;
using Project1.Data.Systems.Physics;
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
            IsMouseVisible = false;

            _world = new World(this, "Level1")
                .AddSystem<Camera>()
                .AddSystem<PlayerMovement>()
                .AddSystem<RenderingSystem>()
                .AddSystem<PhysicsSystem>()
                .AddSystem<HudSystem>();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //_world.CreateEntity()
            //    .AddComponent(new PositionComponent(Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateScale(100)))
            //    .AddComponent(new BillboardComponent("textures/shrimp", BillboardOption.EntityFacing))
            //    .AddComponent(new PhysicsComponent(PhysicsBody.Plane, RigidBodyFlags.Static));

            Random r = new Random();
            for (int i = 20; i <= 100; i++)
            {
                PrimitivePhysicsComponent comp = new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic);
                var ent = _world.CreateEntity()
                    .AddComponent(new PositionComponent((Vector3.Up * 4 * i) + new Vector3((float)r.NextDouble(), 0, (float)r.NextDouble())))
                    .AddComponent(new MeshComponent("models/sphere"))
                    .AddComponent(comp);
                ent.Position.SetLocalMatrix(Matrix.CreateScale(.8f));
                //comp.LinearVelocity = Vector3.Down * 1f * i;
                //comp.AngularVelocity = Vector3.One;
            }

            var plane = _world.CreateEntity()
                    .AddComponent(new PositionComponent(Matrix.CreateRotationX(-MathHelper.PiOver2)))// * Matrix.CreateTranslation(Vector3.Down * 5)))
                    .AddComponent(new BillboardComponent("textures/shrimp", BillboardOption.EntityFacing))
                    .AddComponent(new PrimitivePhysicsComponent(RigidBody.Plane, RigidBodyFlags.Static));
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