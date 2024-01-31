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
using System.IO;

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
                .AddSystem<RenderingSystem>()
                .AddSystem<PhysicsSystem>()
                .AddSystem<HudSystem>();
        }

        protected override void LoadContent()
        {
            var ent = _world.CreateEntity()
                    .AddComponent(new PositionComponent())
                    .AddComponent(new MeshComponent("models/sphere"))
                    .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic, .5f));
            ent.Position.SetLocalMatrix(Matrix.CreateScale(.45f));
            ent.GetComponent<PrimitivePhysicsComponent>().AngularDampening = .6f;

            _world.AddSystem<WorldLoadingSystem>();
            string file = Path.Combine(Content.RootDirectory, "worlds", "world1.txt");
            var worldLoader = _world.GetSystem<WorldLoadingSystem>();
            _world.GetSystem<WorldLoadingSystem>().LoadWorld(file);
            ent.Position.SetWorldMatrix(Matrix.CreateTranslation(worldLoader.PlayerLocation));

#if false
            _world.AddSystem<GolfingSystem>();
            _world.GetSystem<GolfingSystem>().SetPlayer(ent);
#else
            _world.AddSystem<SpectatorMovement>();
#endif

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