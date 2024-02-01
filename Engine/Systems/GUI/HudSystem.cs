﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project1.Engine.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.GUI
{
    internal class HudSystem : SystemComponent, IDrawUpdate
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private Game _game;
        private RenderingSystem _render;
        
        public HudRoot Root { get; private set; }
        public Vector2I ScreenCenter { get; private set; }
        public Vector2I ScreenBounds { get; private set; }

        public HudSystem(Game game, RenderingSystem render)
        {
            _game = game;
            _render = render;
            _render.OnGraphicsReady += GraphicInit;
        }

        private void GraphicInit()
        {
            ScreenCenter = new Vector2I((int)_render.ScreenBounds.X / 2, (int)_render.ScreenBounds.Y / 2);
            Root = new HudRoot(this)
            {
                Visible = true,
                InputEnabled = true,
                Position = ScreenCenter,
            };
            _render.EnqueueMessage(new RenderMessageLoadTexture("Textures/GUI/ColorableSprite"));
        }

        public void Draw(GameTime delta)
        {
            Root.PreDraw((float)delta.ElapsedGameTime.TotalSeconds);
        }

        public void DrawSprite(string sprite, Vector2I pos, Vector2I bounds, float depth)
        {
            Rectangle rec = new Rectangle(pos.X - bounds.X / 2, pos.Y - bounds.Y / 2, bounds.X, bounds.Y);
            _render.EnqueueMessage(new RenderMessageDrawSprite(sprite, rec, depth));
        }

        public void DrawText(string font, string text, float scale, float depth, Vector2I pos, Color color)
        {
            _render.EnqueueMessage(new RenderMessageDrawText(font, text, scale, depth, pos, color));
        }

        public override void Update(GameTime delta)
        {
            Root.PreLayout(false);
        }

    }
}
