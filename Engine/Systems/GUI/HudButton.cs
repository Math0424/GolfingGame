using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.GUI
{
    internal class HudButton : HudElement
    {
        public Color HoverColor;
        public Color PressedColor;

        public Action<object> OnHovered;
        public Action<object> OnClicked;

        protected bool _isHovered;
        protected bool _isPressed;
        private bool _wasWithinBounds;

        public HudButton(HudElement parent) : base(parent) 
        {
            HoverColor = Color.Gray;
            PressedColor = Color.DarkSlateGray;
        }

        public override void HandleInput(ref HudInput input)
        {
            if (input.IsWithinBounds(Position, Bounds))
            {
                if (!_wasWithinBounds)
                {
                    _wasWithinBounds = true;
                    OnHovered?.Invoke(this);
                }

                _isHovered = true;
                if (Input.IsNewMouseDown(Input.MouseButtons.LeftButton))
                    _isPressed = true;

                if (_isPressed && Input.IsNewMouseUp(Input.MouseButtons.LeftButton))
                {
                    _isPressed = false;
                    OnClicked?.Invoke(this);
                }
            }
            else
            {
                _isHovered = false;
                _isPressed = false;
                _wasWithinBounds = false;
            }
        }

        public override void Draw(float deltaTime)
        {
            if (_isPressed)
                _core.Root.DrawColoredSprite("Textures/GUI/ColorableSprite", Position, Bounds, zOffset, PressedColor);
            else if(_isHovered)
                _core.Root.DrawColoredSprite("Textures/GUI/ColorableSprite", Position, Bounds, zOffset, HoverColor);
            else
                base.Draw(deltaTime);
        }

    }
}
