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
        private MainMenuXNAComponent _mainMenu;
        private GolfingGameXNAComponent _golfGame;

        public GolfGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            LoadMainMenu();
        }

        public void LoadMainMenu()
        {
            _mainMenu = new MainMenuXNAComponent(this);
            Components.Add(_mainMenu);
            _mainMenu.StartGame += LoadIntoGame;
        }

        public void LoadIntoGame(string worldName, int playerCount)
        {
            Components.Remove(_mainMenu);
            _mainMenu.Dispose();

            _golfGame = new GolfingGameXNAComponent(this, worldName, playerCount);
            Components.Add(_golfGame);
        }

        public void LoadIntoMainMenu()
        {
            _golfGame.Dispose();
            LoadMainMenu();
        }

        protected override void LoadContent()
        {
            //var ent = _world.CreateEntity()
            //        .AddComponent(new PositionComponent())
            //        .AddComponent(new MeshComponent("models/sphere"))
            //        .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic, .08f, .5f));
            //ent.Position.SetLocalMatrix(Matrix.CreateScale(.40f));
            //_world.AddSystem<WorldLoadingSystem>();
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
            if (Input.IsKeyDown(Keys.F1))
                Exit();

            base.Update(gameTime);
            Input.UpdateState();
        }
    }
}