using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Data;
using Project1.Data.Components;
using Project1.Data.Systems;
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
                .AddSystem<RenderingSystem>();

            //IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        List<Entity> ships = new List<Entity>();

        protected override void LoadContent()
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    ships.Add(_world.CreateEntity()
                       .AddComponent(new PositionComponent((Vector3.Right * (i * 24)) + (Vector3.Forward * (j * 12))))
                       .AddComponent(new MeshComponent("models/Destroyer")));

                    _world.CreateEntity()
                        .AddComponent(new PositionComponent((Vector3.Right * (i * 24)) + (Vector3.Forward * (j * 12)) + (Vector3.Up * 20)))
                        .AddComponent(new BillboardComponent(i % 2 == 0 ? "textures/test" : "textures/shrimp"));

                    // _world.CreateEntity()
                    //     .AddComponent(new PositionComponent((Vector3.Right * (i * 24)) + (Vector3.Forward * (j * 12)) + (Vector3.Up * 10)))
                    //     .AddComponent(new SpriteComponent(i % 2 != 0 ? "textures/test" : "textures/shrimp"));
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.IsKeyDown(Keys.Escape))
                Exit();

            foreach(var x in ships)
            {
                float y = (0.5f - (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds + x.Position.Position.X)) * 300 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector3 newPos = x.Position.Position;
                newPos.Y = y;
                x.Position.SetPosition(newPos);
            }

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