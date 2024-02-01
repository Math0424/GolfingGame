using Microsoft.Xna.Framework;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems;
using Project1.Engine.Systems.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class MainMenuWorld : GameComponent, IDrawable
    {
        private Game _game;
        private World _menuWorld;
        private Camera _camera;
        private float yaw = 0;
        public MainMenuWorld(Game game) : base(game)
        {
            _menuWorld = new World(game)
                .AddSystem<Camera>()
                .AddSystem<RenderingSystem>()
                .AddSystem<HudSystem>();
            _camera = _menuWorld.GetSystem<Camera>();
        }

        public void LoadWorld(string world, int playerCount)
        {
            Console.WriteLine($"Starting {world ?? "null"} with {playerCount} players");
        }

        public override void Initialize()
        {
            var hud = _menuWorld.GetSystem<HudSystem>();

            string[] worlds = Directory.GetFiles(Path.Combine(Game.Content.RootDirectory, "worlds"));
            var menu = new MainMenuGUI(hud.Root, worlds);
            menu.StartGame += LoadWorld;

            _menuWorld.CreateEntity()
                .AddComponent(new PositionComponent())
                .AddComponent(new MeshComponent("Models/Sphere"));

            Random r = new Random();
            for(int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 5; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    float depth = (Math.Abs(j) + Math.Abs(i) + (float)r.NextDouble() * 2) - 1;
                    Matrix mat = Matrix.CreateScale(3, 10, 3) * Matrix.CreateTranslation(i * 3, -6.3f - depth, j * 3);
                    _menuWorld.CreateEntity()
                        .AddComponent(new PositionComponent(Matrix.Identity, mat))
                        .AddComponent(new MeshComponent("Models/Cube", "Shaders/GroundShader"));
                }
            }

            _menuWorld.CreateEntity()
                .AddComponent(new PositionComponent(Matrix.CreateScale(3, 10, 3) * Matrix.CreateTranslation(0, -6.3f, 0)))
                .AddComponent(new MeshComponent("Models/Cube", "Shaders/GroundShader"));
        }

        public int DrawOrder => 0;
        public bool Visible => true;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public void Draw(GameTime gameTime)
        {
            _menuWorld.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            _menuWorld.Update(gameTime);

            yaw += .5f;
            Matrix m = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), 0, 0) * 
                Matrix.CreateTranslation(_camera.Backward * 10);
            _camera.SetWorldMatrix(m);
        }

        protected override void Dispose(bool disposing)
        {
            _menuWorld.Close();
            base.Dispose(disposing);
        }

    }
}
