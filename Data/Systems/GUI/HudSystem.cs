using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.GUI
{
    internal class HudSystem : SystemComponent, IDrawUpdate
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private HudNode _root;
        private Game _game;
        private GraphicsDeviceManager _graphics;

        public HudSystem(Game game, GraphicsDeviceManager graphics)
        {
            _game = game;
            _graphics = graphics;
            _graphics.DeviceCreated += GraphicInit;
        }

        private void GraphicInit(object sender, EventArgs e)
        {
            _graphics = (GraphicsDeviceManager)sender;
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _root = new HudElement(null)
            {
                Bounds = _graphics.GraphicsDevice.Viewport.Bounds,
                Visible = true,
                InputEnabled = true,
                Position = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2, _graphics.GraphicsDevice.Viewport.Height / 2)
            };
            _font = _game.Content.Load<SpriteFont>("Fonts/Debug");
        }

        public void RegisterHudElement(HudElement element)
        {
            _root.AddChild(element);
        }

        public void Draw(GameTime delta)
        {

        }

        public override void Update(GameTime delta)
        {

        }

    }
}
