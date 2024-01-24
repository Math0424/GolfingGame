using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Data;
using Project1.Data.Components;
using Project1.Data.Systems;

namespace Project1
{
    public class GolfGame : Game
    {
        private World _world;

        public GolfGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _world = new World("Level1")
                .AddSystem(new CameraSystem(true))
                .AddSystem(new RenderingSystem(this));
        }

        protected override void Initialize()
        {
            _world.Initalize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Entity player = _world.CreateEntity()
                .AddComponent(new PositionComponent())
                .AddComponent(new MeshComponent(Content, "model"));
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _world.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _world.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}