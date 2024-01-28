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

            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;
                PrimitiveComponent comp = new PrimitiveComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic);
                _world.CreateEntity()
                    .AddComponent(new PositionComponent(Vector3.Up * 5 * i))
                    .AddComponent(new MeshComponent("models/Destroyer"))
                    .AddComponent(comp);
                comp.LinearMomentum = Vector3.Down * 1f * i;
                //comp.AngularMomentum = Vector3.One;
            }
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