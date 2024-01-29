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
    public enum BillboardOption
    {
        CameraFacing,
        EntityFacing,
    }

    internal class BillboardComponent : RenderableComponent
    {
        private static SpriteFont _font;
        private string _assetName;
        private Texture2D _texture;
        private BillboardOption _option;

        private static VertexPositionTexture[] _vertexPositionTexture = new[]
        {
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, -1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, -1)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
        };
        private static short[] _vertexIndicesNoBack = new short[] { 0, 1, 2, 2, 3, 0 };
        private static short[] _vertexIndices = new short[] { 0, 1, 2, 2, 3, 0, 0, 3, 2, 2, 1, 0 };

        public BillboardComponent(string assetName, BillboardOption option = BillboardOption.CameraFacing)
        {
            _option = option;
            _assetName = assetName;
        }

        public override void Initalize()
        {
            _texture = _entity.World.Game.Content.Load<Texture2D>(_assetName);
            if (_font == null)
                _font = _entity.World.Game.Content.Load<SpriteFont>("Fonts/Debug");
        }

        // TOOD : group billboards with the same texture together
        public override void Draw(ref BasicEffect effect, ref GraphicsDevice graphics, ref Camera cam)
        {
            effect.TextureEnabled = true;
            effect.Texture = _texture;

            Matrix mat;
            switch (_option)
            {
                case BillboardOption.CameraFacing:
                    mat = cam.WorldMatrix;
                    mat.Translation = _entity.Position.Position;
                    effect.World = mat;
                    effect.CurrentTechnique.Passes[0].Apply();
                    // no need to draw backsides of trianges we dont see
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexPositionTexture, 0, 4, _vertexIndicesNoBack, 0, 2);
                    break;
                case BillboardOption.EntityFacing:
                    mat = _entity.Position.TransformMatrix;
                    effect.World = mat;
                    effect.CurrentTechnique.Passes[0].Apply();
                    // draw both front and back
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexPositionTexture, 0, 4, _vertexIndices, 0, 4);
                    break;
            }
        }

        public override void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position; 
            Vector3 posx = pos.Position;
            Vector3 screen = cam.WorldToScreen(ref posx);
            batch.DrawString(_font, $"Billboard\nID: {_entity.Id}\nTxt: {_assetName}", new Vector2(screen.X, screen.Y), Color.Black, 0, Vector2.Zero, 1 - screen.Z, default, 0);
        }

        public override bool IsVisible(ref Camera cam)
        {
            return cam.IsInFrustum(_entity.Position.Position);
        }
    }
}
