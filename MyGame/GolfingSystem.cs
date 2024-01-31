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

        public GolfingSystem(Game game, World world, RenderingSystem render, Camera camera)
        {
            _render = render;
            _game = game;
            _cam = camera;
            _world = world;
            _cam.SetWorldMatrix(Matrix.CreateRotationX(-MathHelper.PiOver2));

            float aspectRatio = (float)render.ScreenBounds.X / render.ScreenBounds.Y;
            _cam.SetupOrthographic(aspectRatio * 15, 15, -1.5f, 50f);
        }

        public void Reset()
        {
            _strokes = 0;
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
                        Vector3 val = -new Vector3(deltaMouse.X, 0, deltaMouse.Y) / 10;
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
