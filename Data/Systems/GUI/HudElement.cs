using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.GUI
{
    internal abstract class HudElement : HudNode
    {
        public bool UseCursor { get; set; }
        public bool ShareCursor { get; set; }
        public Rectangle Bounds { get; set; }
        public ParentAlignments ParentAlignment { get; set; }
        public SizeAlignments SizeAlignment { get; set; }

        public HudElement(HudElement parent) : base(parent)
        {
            ShareCursor = false;
            Bounds = new Rectangle(0, 0, 10, 10);
        }

        public override void Layout()
        {

        }

        private void GetForcedLayout()
        {

        }

    }
}
