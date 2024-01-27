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
    internal class SpriteComponent : EntityComponent
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

        public float ZDepth(ref Camera cam)
        {
            return Vector3.DistanceSquared(_entity.Position.Position, cam.Translation);
        }

        public void Draw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position.Position;
            var newPos = cam.WorldToScreen(ref pos);

            Rectangle r = new Rectangle((int)newPos.X, (int)newPos.Y, (int)(30), (int)(30));

            batch.Draw(_texture, r, Color.White);
        }

        public void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;
            Vector3 posx = pos.Position;
            Vector3 screen = cam.WorldToScreen(ref posx);
            batch.DrawString(_font, $"Sprite\nID: {_entity.Id}", new Vector2(screen.X, screen.Y), Color.Black, 0, Vector2.Zero, 1 - screen.Z, default, 0);
        }

        public bool IsVisible(ref Camera cam)
        {
            return cam.IsInFrustum(_entity.Position.Position);
        }
    }
}
