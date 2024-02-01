using Project1.Engine;
using Project1.Engine.Systems.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class GolfGUI : HudCore
    {
        public GolfGUI(HudRoot root) : base(root)
        {
            Visible = true;

            new HudElement(this)
            {
                Padding = 10,
                Bounds = new Vector2I(50, 50),
            };

            new HudElement(this)
            {
                Padding = 10,
                Bounds = new Vector2I(50, 50),
                Position = new Vector2I(150, 150),
            };
        }
    }
}
