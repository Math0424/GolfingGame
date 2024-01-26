using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Components
{
    internal class SpriteComponent : RenderableComponent
    {
        private static SpriteFont _font;
        private string _assetName;
        private Texture2D _texture;

        public SpriteComponent(string assetName)
        {
            _assetName = assetName;
        }

        public override void Initalize()
        {
            _texture = _entity.World.Game.Content.Load<Texture2D>(_assetName);
            if (_font == null)
                _font = _entity.World.Game.Content.Load<SpriteFont>("Fonts/Debug");
        }

        public override void Draw(ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;
            Vector3 posx = pos.Position;
            float depth = Vector3.DistanceSquared(cam.Translation, posx);
            //batch.Draw(_texture, cam.WorldToScreen(ref posx), Rectangle.Empty, Color.White, 0, Vector2.One, Vector2.One, SpriteEffects.None, depth);
        }

        public override void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position; 
            Vector3 posx = pos.Position;
            Vector2 screen = cam.WorldToScreen(ref posx);
            batch.DrawString(_font, $"ID: {_entity.Id}", screen, Color.Black);
        }

        public override bool IsVisible(ref Camera cam)
        {
            return cam.IsInFrustum(_entity.Position.Position);
        }
    }
}
