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
        public Rectangle Bounds
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
        private ParentAlignments _parentAlignments;
        private SizeAlignments _sizeAlignments;
        private Rectangle _bounds;

        public HudElement(HudElement parent) : base(parent)
        {
            ShareCursor = false;
            Bounds = new Rectangle(0, 0, 10, 10);
            _parentAlignments = ParentAlignments.None | ParentAlignments.Padding;
        }

        public override void Layout()
        {

        }

        protected void UpdateSizeAlignment()
        {
            Rectangle size = Bounds;
            if ((_sizeAlignments & SizeAlignments.Width) == SizeAlignments.Width)
                size.Width = _classParent.Bounds.Width;
            if((_sizeAlignments & SizeAlignments.Height) == SizeAlignments.Height)
                size.Height = _classParent.Bounds.Height;
            Bounds = size;
        }

        protected void UpdateParentAlignment()
        {
            if ((_parentAlignments & ParentAlignments.Top) == ParentAlignments.Top)
            {

            }
            if (_parentAlignments.HasFlag(ParentAlignments.Bottom))
            {

            }
            if (_parentAlignments.HasFlag(ParentAlignments.Left))
            {

            }
            if (_parentAlignments.HasFlag(ParentAlignments.Right))
            {

            }
        }

        public override void Draw()
        {

        }

    }
}
