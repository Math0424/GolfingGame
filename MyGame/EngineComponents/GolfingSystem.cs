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
        public string[] _levelRotation = new[]
        {
            "Worlds/world1.txt",
            "Worlds/world2.txt",
            "Worlds/world3.txt",
            "Worlds/world4.txt",
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

            int aspectRatio = render.ScreenBounds.X / render.ScreenBounds.Y;
            _cam.SetupOrthographic(aspectRatio * 15, 15, -50f, 50f);

            LoadWorld(_worldId);
        }

        private void LoadWorld(int id)
        {
            _worldLoader.LoadWorld(_levelRotation[id % _levelRotation.Length]);
            if (_player != null)
                _player.GetComponent<PrimitivePhysicsComponent>().RigidBody.WorldMatrix = Matrix.CreateTranslation(_worldLoader.PlayerLocation);
            var hole = _world.GetEntity(_worldLoader.HoleId).GetComponent<PrimitivePhysicsComponent>();
            hole.Collision += (e, v) => { if (e == _player.Id) EnterHole(); };
        }

        public void EnterHole()
        {
            foreach(var e in _world.GetEntities())
                if (e.Id != _player.Id)
                    e.Close();
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
                var physics = _player.GetComponent<PrimitivePhysicsComponent>();
                _render.EnqueueMessage(new RenderMessageDrawText("Fonts/Debug", $"World: {_levelRotation[_worldId % _levelRotation.Length]}\nStrokes: {_strokes}", 20, 0, Vector2.One * 20, physics.IsSleeping ? Color.Azure : Color.Red));

                var matrix = _cam.WorldMatrix;
                matrix.Translation = _player.Position.Position;
                _cam.SetWorldMatrix(matrix);

                if (_player.Position.Position.Y < _worldLoader.KillLevel.Y)
                {
                    _strokes++;
                    physics.RigidBody.WorldMatrix = Matrix.CreateTranslation(_worldLoader.PlayerLocation);
                    physics.RigidBody.Stop();
                }

                if (physics.IsSleeping)
                {
                    if (Input.IsNewMouseDown(Input.MouseButtons.LeftButton))
                        _mouseDragStart = Input.MousePosition();

                    if (Input.IsNewMouseUp(Input.MouseButtons.LeftButton))
                    {
                        Vector2 deltaMouse = Input.MousePosition() - _mouseDragStart;
                        Vector3 val = -new Vector3(deltaMouse.X, 0, deltaMouse.Y) / 100;
                        physics.AddForce(val);
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
