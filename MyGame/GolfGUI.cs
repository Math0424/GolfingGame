using Project1.Engine;
using Project1.Engine.Systems.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class GolfGUI : HudCore
    {
        private HudButton _startGameBtn;
        private HudButton _sourceCodeBtn;

        private HudButton _levelSelectBtn;

        public GolfGUI(HudRoot root) : base(root)
        {
            Visible = true;

            _sourceCodeBtn = new HudTextButton(this)
            {
                Bounds = new Vector2I(200, 50),
                Padding = 20,
                Text = "Source Code",
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Padding
            };
            _sourceCodeBtn.OnClicked += () => { Process.Start("explorer", "https://github.com/Math0424/GolfingGame"); };

            _startGameBtn = new HudTextButton(_sourceCodeBtn)
            {
                Padding = 10,
                Text = "Start Game",
                Bounds = new Vector2I(200, 50),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Padding
            }; 
            
            _levelSelectBtn = new HudTextButton(this)
            {
                Padding = 20,
                Text = "Level Select",
                Bounds = new Vector2I(200, 50),
                ParentAlignment = ParentAlignments.Right | ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Padding
            };
        }
    }
}
