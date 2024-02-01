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
        public GolfGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Components.Add(new MainMenuWorld(this));

            //_world = new World(this)
            //    .AddSystem<Camera>()
            //    .AddSystem<RenderingSystem>()
            //    .AddSystem<PhysicsSystem>()
            //    .AddSystem<HudSystem>();
        }

        protected override void LoadContent()
        {
            //var ent = _world.CreateEntity()
            //        .AddComponent(new PositionComponent())
            //        .AddComponent(new MeshComponent("models/sphere"))
            //        .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic, .08f, .5f));
            //ent.Position.SetLocalMatrix(Matrix.CreateScale(.40f));
            //
            //_world.AddSystem<WorldLoadingSystem>();
            // var hud = _world.GetSystem<HudSystem>();
            // new MainMenuGUI(hud.Root);

#if false
            _world.AddSystem<GolfingSystem>();
            _world.GetSystem<GolfingSystem>().SetPlayer(ent);
#else
            //_world.AddSystem<SpectatorMovement>();
            //var worldLoader = _world.GetSystem<WorldLoadingSystem>();
            //worldLoader.LoadWorld("worlds/world2.txt");
#endif
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
            Input.UpdateState();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}