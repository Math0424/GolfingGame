using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.GUI
{
    internal class HudElement : HudNode
    {

        public override HudNode Parent
        {
            set
            {
                _parent = value;
                _classParent = Parent as HudElement;
            }
        }

        public bool UseCursor { get; set; }
        public bool ShareCursor { get; set; }

        public int Padding;
        public Vector2I Bounds
        {
            get { return _bounds; }
            set {
                _bounds = value;
                UpdateParentAlignment();
            }
        }
        public ParentAlignments ParentAlignment
        {
            get { return _parentAlignments; }
            set
            {
                _parentAlignments = value;
                UpdateParentAlignment();
            }
        }
        public SizeAlignments SizeAlignment
        {
            get => _sizeAlignments;
            set
            {
                _sizeAlignments = value;
                UpdateSizeAlignment();
            }
        }

        private HudElement _classParent;
        private HudCore _core;
        private ParentAlignments _parentAlignments;
        private SizeAlignments _sizeAlignments;
        private Vector2I _bounds;

        public HudElement(HudRoot core) : base(core) {}

        public HudElement(HudCore core) : base(core)
        {
            _core = core;
            _classParent = core;
            Setup();
        }

        public HudElement(HudElement parent) : base(parent)
        {
            _core = parent?._core;
            _classParent = parent;
            Setup();
        }

        public void Setup()
        {
            ShareCursor = false;
            Visible = true;
            Bounds = new Vector2I(10, 10);
            Position = Parent.Position;
        }

        public override void Layout() {}

        protected void UpdateSizeAlignment()
        {
            Vector2I size = Bounds;
            if ((_sizeAlignments & SizeAlignments.Width) == SizeAlignments.Width)
                size.X = _classParent.Bounds.X;
            if((_sizeAlignments & SizeAlignments.Height) == SizeAlignments.Height)
                size.Y = _classParent.Bounds.Y;
            Bounds = size;
        }

        protected void UpdateParentAlignment()
        {
            Vector2I newPos = Position;
            if (_parentAlignments.HasFlag(ParentAlignments.Center))
            {
                newPos.X = Parent.PositionRef.X + Bounds.X / 2;
                newPos.Y = Parent.PositionRef.Y + Bounds.Y / 2;
                return;
            }

            int inner = _parentAlignments.HasFlag(ParentAlignments.Inner) ? -1 : 1;
            int padding = _parentAlignments.HasFlag(ParentAlignments.Padding) ? Padding * inner : 0;

            if (_parentAlignments.HasFlag(ParentAlignments.Top))
            {
                newPos.Y = Parent.PositionRef.Y - (_classParent.Bounds.Y / 2) - padding;
                newPos.Y -= (Bounds.Y / 2) * inner;
            }
            if (_parentAlignments.HasFlag(ParentAlignments.Bottom))
            {
                newPos.Y = Parent.PositionRef.Y + (_classParent.Bounds.Y / 2) + padding;
                newPos.Y += (Bounds.Y / 2) * inner;
            }
            if (_parentAlignments.HasFlag(ParentAlignments.Left))
            {
                newPos.X = Parent.PositionRef.X - (_classParent.Bounds.X / 2) - padding;
                newPos.X -= (Bounds.X / 2) * inner;
            }
            if (_parentAlignments.HasFlag(ParentAlignments.Right))
            {
                newPos.X = Parent.PositionRef.X + (_classParent.Bounds.X / 2) + padding;
                newPos.X += (Bounds.X / 2) * inner;
            }
            Position = newPos;
        }

        public override void Draw(float deltaTime)
        {
            _core.Root.DrawSprite("Textures/GUI/ColorableSprite", Position, Bounds, zOffset);
        }

    }
}
