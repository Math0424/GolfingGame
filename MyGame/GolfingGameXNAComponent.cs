using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems;
using Project1.Engine.Systems.GUI;
using Project1.Engine.Systems.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class GolfingGameXNAComponent : GameComponent
    {
        public Action InvokeMainMenu;

        private Game _game;
        private int _playerCount;
        private World _world;
        private string[] _worlds;
        private int _currentWorld;

        private List<Entity> _players;
        private int _currentPlayerToGolf;

        private Entity _currentPlayer;

        private Color[] _playerColors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Orange,
            Color.Pink,
        };

        public GolfingGameXNAComponent(Game game, string startingWorld, int playerCount) : base(game)
        {
            _game = game;
            _playerCount = playerCount;

            _world = new World(_game)
                .AddSystem<Camera>()
                .AddSystem<RenderingSystem>()
                .AddSystem<PhysicsSystem>()
                .AddSystem<WorldLoadingSystem>()
                .AddSystem<HudSystem>();
            _game.Components.Add(_world);

            _worlds = Directory.GetFiles(Path.Combine(Game.Content.RootDirectory, "worlds"));

            if (startingWorld == null)
                _currentWorld = 0;
            else
            {
                for (int i = 0; i < _worlds.Length; i++)
                {
                    if (Path.GetFileNameWithoutExtension(_worlds[i]) == startingWorld)
                    {
                        _currentWorld = i;
                        break;
                    }
                }
            }
        }

        public override void Initialize()
        {
            var cam = _world.Render.Camera;
            cam.SetWorldMatrix(Matrix.CreateRotationX(-MathHelper.PiOver2));

            int aspectRatio = _world.Render.ScreenBounds.X / _world.Render.ScreenBounds.Y;
            cam.SetupOrthographic(aspectRatio * 15, 15, -50f, 50f);

            var hud = _world.GetSystem<HudSystem>();
            LoadWorld();
        }

        private void LoadWorld()
        {
            foreach (var e in _world.GetEntities())
                e.Close();

            var worldLoader = _world.GetSystem<WorldLoadingSystem>();
            worldLoader.LoadWorld(_worlds[_currentWorld]);

            _world.GetEntity(worldLoader.HoleId).GetComponent<PrimitivePhysicsComponent>().Collision += HoleCollision;

            _players = new List<Entity>(_playerCount);
            for (int i = 0; i < _playerCount; i++)
            {
                _players.Add(_world.CreateEntity()
                    .AddComponent(new PositionComponent(Matrix.CreateTranslation(worldLoader.PlayerLocation)))
                    .AddComponent(new MeshComponent("models/sphere"))
                    .AddComponent(new EntityGolfingComponent(worldLoader.PlayerLocation, worldLoader.KillLevel.Y, $"Player {i+1}", _playerColors[i % _playerColors.Length]))
                    .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Dynamic, .08f, .5f)));
                _players[i].Position.SetLocalMatrix(Matrix.CreateScale(.40f));
                _players[i].GetComponent<PrimitivePhysicsComponent>().IsEnabled = false;
            }
            ActivateCurrentPlayer();
        }

        private void ActivateCurrentPlayer()
        {
            if (_players.Count == 0)
                return;

            _currentPlayer = _players[_currentPlayerToGolf % _players.Count];
            _currentPlayer.GetComponent<EntityGolfingComponent>().IsActive = true;
            _currentPlayer.GetComponent<PrimitivePhysicsComponent>().IsEnabled = true;
        }

        private void HoleCollision(int id, Vector3 pos)
        {
            for(int i = 0; i < _players.Count; i++)
            {
                Entity player = _players[i];
                if (player.Id == id)
                {
                    _players.Remove(player);
                    player.Close();
                    if (_currentPlayer == player)
                        ActivateCurrentPlayer();
                    if (_players.Count != 0 && (i % _players.Count) > _currentPlayerToGolf)
                        _currentPlayerToGolf--;
                    break;
                }
            }
            if (_players.Count == 0)
            {
                _currentWorld++;
                if (_currentWorld == _worlds.Length)
                {
                    InvokeMainMenu?.Invoke();
                    return;
                }
                LoadWorld();
            }
        }

        public override void Update(GameTime gameTime)
        {
            EntityGolfingComponent golfBall = _currentPlayer.GetComponent<EntityGolfingComponent>();
            if (golfBall.TurnComplete)
            {
                golfBall.IsActive = false;
                _currentPlayerToGolf++;
                ActivateCurrentPlayer();
            }

            if (Input.IsNewKeyDown(Keys.Escape))
                InvokeMainMenu?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            _game.Components.Remove(_world);
            _world.Dispose();
            base.Dispose(disposing);
        }

    }
}
