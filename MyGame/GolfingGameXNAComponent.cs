using Microsoft.Xna.Framework;
using Project1.Engine;
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
    internal class GolfingGameXNAComponent : GameComponent, IDrawable
    {
        private Game _game;
        private string _startingWorld;
        private int _playerCount;
        private World _world;
        private string[] _worlds;
        private int _currentWorld;

        public GolfingGameXNAComponent(Game game, string startingWorld, int playerCount) : base(game)
        {
            _game = game;
            _playerCount = playerCount;
            _startingWorld = startingWorld;

            _world = new World(_game)
                .AddSystem<Camera>()
                .AddSystem<RenderingSystem>()
                .AddSystem<PhysicsSystem>()
                .AddSystem<WorldLoadingSystem>()
                .AddSystem<SpectatorMovement>()
                .AddSystem<HudSystem>();
            _game.Components.Add(_world);

            _worlds = Directory.GetFiles(Path.Combine(Game.Content.RootDirectory, "worlds"));

            if (_startingWorld == null)
                _startingWorld = _worlds[0];
            else
            {
                foreach (var x in _worlds)
                {
                    if (Path.GetFileNameWithoutExtension(x) == _startingWorld)
                    {
                        _startingWorld = x;
                        break;
                    }
                    _currentWorld++;
                }
            }
        }

        public override void Initialize()
        {
            _world.GetSystem<WorldLoadingSystem>().LoadWorld(_startingWorld);

            var hud = _world.GetSystem<HudSystem>();
        }

        public override void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {

        }

        protected override void Dispose(bool disposing)
        {
            _world.Dispose();
            base.Dispose(disposing);
        }

        public int DrawOrder => 1;
        public bool Visible => true;
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

    }
}
