using Microsoft.Xna.Framework;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class EntityGolfingComponent : EntityUpdateComponent
    {

        public int Strokes { get; private set; }
        public bool TurnComplete { get; private set; }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                _hasStroked = false;
                TurnComplete = false;
            }
        }
        
        private Vector3 _resetPos;
        private float _killLevel;
        private Vector2 _mouseDragStart;
        private bool _hasStroked;
        private bool _isActive;
        private string _name;
        private Color _color;

        public EntityGolfingComponent(Vector3 resetPos, float killLevel, string name, Color color)
        {
            _color = color;
            _name = name;
            _resetPos = resetPos;
            _killLevel = killLevel;
            IsActive = false;
        }

        public override void Update(GameTime deltaTime)
        {
            var camera = _entity.World.Render.Camera;
            DrawingUtils.DrawWorldText(_entity.World.Render, _name, _entity.Position.Position + camera.Up, _color, TextDrawOptions.Centered);

            if (!IsActive)
                return;

            var physics = _entity.GetComponent<PrimitivePhysicsComponent>();

            var matrix = camera.WorldMatrix;
            matrix.Translation = _entity.Position.Position;
            camera.SetWorldMatrix(matrix);

            if (_entity.Position.Position.Y < _killLevel)
            {
                Strokes++;
                physics.RigidBody.WorldMatrix = Matrix.CreateTranslation(_resetPos);
                physics.RigidBody.Stop();
            }

            if (physics.IsSleeping)
            {
                if (_hasStroked)
                {
                    IsActive = false;
                    TurnComplete = true;
                    _hasStroked = false;
                    return;
                }

                if (Input.IsNewMouseDown(Input.MouseButtons.LeftButton))
                    _mouseDragStart = Input.MousePosition();

                if (Input.IsNewMouseUp(Input.MouseButtons.LeftButton))
                {
                    Vector2 deltaMouse = Input.MousePosition() - _mouseDragStart;
                    Vector3 val = -new Vector3(deltaMouse.X, 0, deltaMouse.Y) / 100;
                    physics.AddForce(val);
                    _hasStroked = true;
                    Strokes++;
                }

                if (Input.IsMouseDown(Input.MouseButtons.LeftButton))
                {
                    _entity.World.Game.IsMouseVisible = false;
                    Vector2 deltaMouse = Input.MousePosition() - _mouseDragStart;
                    Vector3 val = new Vector3(deltaMouse.X, 0, deltaMouse.Y);
                    _entity.World.Render.EnqueueMessage(new RenderMessageDrawLine(_entity.Position.Position, _entity.Position.Position + val / 10, Color.Red));
                }
                else
                    _entity.World.Game.IsMouseVisible = true;
            }

        }
    }
}
