using Project1.Engine;
using Project1.Engine.Systems.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class MainMenuGUI : HudCore
    {
        public Action<string, int> StartGame;

        private HudTextButton _startGameBtn;
        private HudTextButton _sourceCodeBtn;

        private HudTextButton _levelSelectBtn;
        private HudTextButton _playerModeBtn;

        private HudTextButton[] _levelSelectBtns;
        private int _playerCount;

        public MainMenuGUI(HudRoot root, string[] levels) : base(root)
        {
            Visible = true;
            _playerCount = 0;

            _sourceCodeBtn = new HudTextButton(this)
            {
                Bounds = new Vector2I(200, 50),
                Padding = 20,
                Text = "Source Code",
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Padding
            };
            _sourceCodeBtn.OnClicked += (e) => { Process.Start("explorer", "https://github.com/Math0424/GolfingGame"); };

            _startGameBtn = new HudTextButton(_sourceCodeBtn)
            {
                Padding = 10,
                Text = "Start Game",
                Bounds = new Vector2I(200, 50),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Padding
            };
            _startGameBtn.OnClicked += (e) =>
            {
                StartGame?.Invoke(null, _playerCount + 1);
            };
            
            _levelSelectBtn = new HudTextButton(this)
            {
                Padding = 20,
                Text = "Level Select",
                Bounds = new Vector2I(200, 50),
                ParentAlignment = ParentAlignments.Right | ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Padding
            };
            _levelSelectBtn.OnClicked += (e) =>
            {
                foreach(var x in _levelSelectBtns)
                    x.Visible = !x.Visible;
            };

            _levelSelectBtns = new HudTextButton[levels.Length];
            for(int i = 0; i < levels.Length; i++)
            {
                _levelSelectBtns[i] = new HudTextButton(_levelSelectBtn)
                {
                    Text = Path.GetFileNameWithoutExtension(levels[i]),
                    Bounds = new Vector2I(100, 40),
                    Visible = false,
                    Padding = 10,
                    ParentAlignment = ParentAlignments.Top | ParentAlignments.Right | ParentAlignments.InnerH | ParentAlignments.Padding,
                };
                _levelSelectBtns[i].Position -= new Vector2I(0, i * 50);
                _levelSelectBtns[i].OnClicked += (e) =>
                {
                    StartGame?.Invoke(((HudTextButton)e).Text, _playerCount + 1);
                };
            }

            _playerModeBtn = new HudTextButton(this)
            {
                Padding = 20,
                Text = $"{_playerCount + 1} Player Mode",
                Bounds = new Vector2I(200, 50),
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Padding
            };
            _playerModeBtn.OnClicked += (e) => {
                _playerCount = (_playerCount + 1) % 2;
                _playerModeBtn.Text = $"{_playerCount + 1} Player Mode";
            };
            
        }
    }
}
