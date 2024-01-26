using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Components
{
    struct ModelInfo
    {
        public Model Model;
        public string Name;
        public int Verticies;
        public BoundingBox BoundingBox;
        public Vector3 ModelCenter;
        public float Radius;
    }

    internal class MeshComponent : RenderableComponent
    {
        // TODO : possible memory leak if lots unique models are created and then never used again
        private static Dictionary<string, ModelInfo> cache = new Dictionary<string, ModelInfo>();
        private static SpriteFont _font;

        public ref ModelInfo Model => ref _info;
        public Vector3 ModelCenter => _info.ModelCenter;
        public string ModelName
        {
            get => _info.Name;
            set => SetModel(value);
        }

        private ModelInfo _info;
        private string _modelName;

        public MeshComponent(string modelname)
        {
            _modelName = modelname;
        }

        public override void Initalize()
        {
            SetModel(_modelName);
            if (_font == null)
                _font = _entity.World.Game.Content.Load<SpriteFont>("Fonts/Debug");
        }

        public override bool IsVisible(ref Camera cam)
        {
            var pos = _entity.Position;

            // I want to cull everything behind the ViewMatrix, but something is wrong~
            // float dist = Vector3.Dot(cam.WorldMatrix.Forward, _entity.Position.Position - ModelCenter) + Vector3.Dot(cam.Forward, cam.Translation);
            // if (dist > -_info.Radius)
            // {
            //     Console.WriteLine($"Behind plane {dist} ({_info.Radius})");
            //     return false;
            // }

            BoundingBox WAABB = new BoundingBox(Vector3.Transform(_info.BoundingBox.Min, pos.TransformMatrix), Vector3.Transform(_info.BoundingBox.Max, pos.TransformMatrix));
            return cam.Frustum.Intersects(WAABB);
        }

        public override void Draw(ref GraphicsDevice graphics, ref Camera cam)
        {
            _info.Model.Draw(_entity.Position.TransformMatrix, cam.ViewMatrix, cam.ProjectionMatrix);
        }

        public override void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        {
            var pos = _entity.Position;
            BoundingBox bb = Model.BoundingBox;
            BoundingBox obb = new BoundingBox(Vector3.Transform(bb.Min, pos.TransformMatrix), Vector3.Transform(bb.Max, pos.TransformMatrix));
            Vector3[] corners = obb.GetCorners();

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
            graphics.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);

            Vector3 posx = pos.Position;
            Vector2 screen = cam.WorldToScreen(ref posx);
            batch.DrawString(_font, $"ID: {_entity.Id}\nV:{Model.Verticies}", screen, Color.Black);
        }

        private void SetModel(string name)
        {
            _info.Name = name;
            if (cache.ContainsKey(name))
            {
                _info = cache[name];
                return;
            }
            Model model = _entity.World.Game.Content.Load<Model>(name);
            CalculateModelInfo(model, out _info);

            cache[name] = _info;
        }

        private void CalculateModelInfo(Model model, out ModelInfo info)
        {
            Vector3 min = Vector3.Zero, max = Vector3.Zero;
            Vector3 center = Vector3.Zero;
            int verticies = 0;
            foreach(var mesh in model.Meshes)
            {
                foreach(var part in mesh.MeshParts)
                    verticies += part.NumVertices;

                int vertexStride = mesh.MeshParts[0].VertexBuffer.VertexDeclaration.VertexStride;
                float[] vertexData = new float[verticies * vertexStride / sizeof(float)];
                mesh.MeshParts[0].VertexBuffer.GetData(vertexData);

                for (int i = 0; i < vertexData.Length; i += vertexStride / sizeof(float))
                {
                    Vector3 pos = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                    center += pos;
                    min = Vector3.Min(min, pos);
                    max = Vector3.Max(max, pos);
                }
            }

            info = new ModelInfo()
            {
                Model = model,
                Verticies = verticies,
                ModelCenter = (min + max) / 2,
                BoundingBox = new BoundingBox(min, max),
                Radius = Vector3.Distance(min, max) / 2,
            };
        }

    }
}
