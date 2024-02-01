using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems;
using Project1.Engine.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class GolfingSystem : SystemComponent
    {
        private Entity _player;
        private Camera _cam;
        private World _world;
        private RenderingSystem _render;
        private float _strokes;
        private Game _game;
        private WorldLoadingSystem _worldLoader;

        private int _worldId;
        public string[] _levels = new[]
        {
            "Worlds/world1.txt",
            "Worlds/world2.txt",
        };

        public GolfingSystem(Game game, World world, WorldLoadingSystem worldLoader, RenderingSystem render, Camera camera)
        {
            _worldId = 0;
            _worldLoader = worldLoader;
            _render = render;
            _game = game;
            _cam = camera;
            _world = world;
            _cam.SetWorldMatrix(Matrix.CreateRotationX(-MathHelper.PiOver2));

            float aspectRatio = render.ScreenBounds.X / render.ScreenBounds.Y;
            _cam.SetupOrthographic(aspectRatio * 15, 15, -50f, 50f);

            LoadWorld(_worldId);
        }

        private void LoadWorld(int id)
        {
            _worldLoader.LoadWorld(_levels[id]);
            if (_player != null)
                _player.GetComponent<PrimitivePhysicsComponent>().RigidBody.WorldMatrix = Matrix.CreateTranslation(_worldLoader.PlayerLocation);
            var hole = _world.GetEntity(_worldLoader.HoleId).GetComponent<PrimitivePhysicsComponent>();
            hole.Collision += (e, v) => Console.WriteLine($"{_worldLoader.HoleId}->{e} at {v} while ball is at {_player.Position.Position}");
            hole.Collision += (e, v) => { if (e == _player.Id) EnterHole(); };
        }

        public void EnterHole()
        {
            var enumerator = _world.GetEntities();
            while(enumerator.Current != null)
            {
                var entity = enumerator.Current;
                if (entity.Id != _player.Id)
                    entity.Close();
                enumerator.MoveNext();
            }
            LoadWorld(++_worldId);
            _strokes = 0;
            _player.GetComponent<PrimitivePhysicsComponent>().Stop();
        }

        public void SetPlayer(Entity ent)
        {
            _player = ent;
        }

        private Vector2 _mouseDragStart;
        public override void Update(GameTime delta)
        {
            if (_player != null && _player.Id != -1)
            {
                var matrix = _cam.WorldMatrix;
                matrix.Translation = _player.Position.Position;
                _cam.SetWorldMatrix(matrix);

                var physics = _player.GetComponent<PrimitivePhysicsComponent>();
                if (physics.IsSleeping)
                {
                    if (Input.IsNewMouseDown(Input.MouseButtons.LeftButton))
                        _mouseDragStart = Input.MousePosition();

                    if (Input.IsNewMouseUp(Input.MouseButtons.LeftButton))
                    {
                        Vector2 deltaMouse = Input.MousePosition() - _mouseDragStart;
                        Vector3 val = -new Vector3(deltaMouse.X, 0, deltaMouse.Y) / 100;
                        Vector3 torque = -Vector3.Normalize(Vector3.Cross(val, Vector3.Up)) / 100;
                        physics.AddForce(val);
                        physics.AddTorque(torque);
                        _strokes++;
                    }

                    if (Input.IsMouseDown(Input.MouseButtons.LeftButton))
                    {
                        _game.IsMouseVisible = false;
                        Vector2 deltaMouse = Input.MousePosition() - _mouseDragStart;
                        Vector3 val = new Vector3(deltaMouse.X, 0, deltaMouse.Y);
                        _render.EnqueueMessage(new RenderMessageDrawLine(_player.Position.Position, _player.Position.Position + val / 10, Color.Red));
                    }
                    else
                        _game.IsMouseVisible = true;
                }
            }
        }
    }
}
