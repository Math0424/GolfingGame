using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.GUI
{
    internal class HudSystem : SystemComponent
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private HudNode _root;
        private World _world;

        public HudSystem(World world)
        {
            _world = world;
            _root = new HudElement(null)
            {
                Bounds = _world.Game.GraphicsDevice.Viewport.Bounds,
                Visible = true,
                InputEnabled = true,
                Position = new Vector2(_world.Game.GraphicsDevice.Viewport.Width / 2, _world.Game.GraphicsDevice.Viewport.Height / 2)
            };
            _spriteBatch = new SpriteBatch(_world.Game.GraphicsDevice);
            _font = _world.Game.Content.Load<SpriteFont>("Fonts/Debug");
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
