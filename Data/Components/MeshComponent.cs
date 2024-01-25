using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
    }

    internal class MeshComponent : RenderableComponent
    {
        // TODO : possible memory leak if lots unique models are created and then never used again
        private static Dictionary<string, ModelInfo> cache = new Dictionary<string, ModelInfo>();

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
        }

        public override bool IsVisible(ref BoundingFrustum frustum)
        {
            var pos = _entity.Position;
            BoundingBox WAABB = new BoundingBox(Vector3.Transform(_info.BoundingBox.Min, pos.TransformMatrix), Vector3.Transform(_info.BoundingBox.Max, pos.TransformMatrix));
            return WAABB.Intersects(frustum);
        }

        public override void Draw3D(ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _info.Model.Draw(_entity.Position.TransformMatrix, viewMatrix, projectionMatrix);
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
                ModelCenter = center / verticies,
                BoundingBox = new BoundingBox(min, max),
            };
            Console.WriteLine($"{min}, {max}");
        }

    }
}
