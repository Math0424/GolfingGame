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
    internal class DebugDrawComponent : RenderableComponent
    {
        private MeshComponent _meshComponent;
        private static SpriteFont _font;

        public override void Initalize()
        {
            _meshComponent = _entity.GetComponent<MeshComponent>();
            if (_font == null)
                _font = _entity.World.Game.Content.Load<SpriteFont>("Fonts/Debug");
        }

        public override void Draw3D(ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            BoundingBox bb = _meshComponent.Model.BoundingBox;
            BoundingBox WAABB = new BoundingBox(Vector3.Transform(bb.Min, _entity.Position.LocalMatrix), Vector3.Transform(bb.Max, _entity.Position.LocalMatrix));
            Vector3[] corners = WAABB.GetCorners();

            //draw bounding box
            var vertices = new[] { 
                new VertexPosition(corners[0]), new VertexPosition(corners[1]),
                new VertexPosition(corners[1]), new VertexPosition(corners[2]),
                new VertexPosition(corners[2]), new VertexPosition(corners[3]),
                new VertexPosition(corners[0]), new VertexPosition(corners[3]),
                new VertexPosition(corners[4]), new VertexPosition(corners[5]),
                new VertexPosition(corners[5]), new VertexPosition(corners[6]),
                new VertexPosition(corners[6]), new VertexPosition(corners[7]),
                new VertexPosition(corners[4]), new VertexPosition(corners[7]),
                new VertexPosition(corners[0]), new VertexPosition(corners[4]),
                new VertexPosition(corners[1]), new VertexPosition(corners[5]),
                new VertexPosition(corners[2]), new VertexPosition(corners[6]),
                new VertexPosition(corners[3]), new VertexPosition(corners[7])
            };
            _entity.World.Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);
        }

        public override void DebugDraw(ref SpriteBatch batch, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            var cam = _entity.World.GetSystem<CameraSystem>();
            if (cam != null) {
                Vector3 pos = _entity.Position.Position;
                Vector2 screen = cam.WorldToScreen(ref pos);
                //batch.DrawString(_font, $"ID: {_entity.Id}", screen, Color.Black);
            }
        }


        public override bool IsVisible(ref BoundingFrustum frustum)
        {
            return _meshComponent.IsVisible(ref frustum);
        }
    }
}
