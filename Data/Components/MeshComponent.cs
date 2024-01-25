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
    internal class MeshComponent : RenderableComponent
    {
        // TODO : possible memory leak if lots unique models are created and then never used again
        private static Dictionary<string, (Model model, BoundingBox boundingBox)> cache = new Dictionary<string, (Model, BoundingBox)>();

        public BoundingBox ModelAABB
        {
            get; private set;
        }
        public string Model
        {
            get => _modelName;
            set => SetModel(_cm, value);
        }
        public Vector3 MeshCenter
        {
            get => _center;
        }

        private Vector3 _center;
        private ContentManager _cm;
        private string _modelName;
        private Model _model;

        public MeshComponent(ContentManager manager, string modelname)
        {
            _cm = manager;
            SetModel(_cm, modelname);
        }
        
        public override bool IsVisible(ref BoundingFrustum frustum)
        {
            var pos = _entity.Position;
            BoundingBox WAABB = new BoundingBox(Vector3.Transform(ModelAABB.Min, pos.TransformMatrix), Vector3.Transform(ModelAABB.Max, pos.TransformMatrix));
            return WAABB.Intersects(frustum);
        }

        public override void Draw(ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            var pos = _entity.Position;
            _model.Draw(pos.TransformMatrix, viewMatrix, projectionMatrix);
        }

        private void SetModel(ContentManager manager, string name)
        {
            _modelName = name;
            if (cache.ContainsKey(name))
            {
                _model = cache[name].model;
                ModelAABB = cache[name].boundingBox;
                return;
            }
            _model = manager.Load<Model>(name);
            CalculateBoundingBox();
            cache[name] = (_model, ModelAABB);
        }

        private void CalculateBoundingBox()
        {
            Vector3 min = Vector3.Zero, max = Vector3.Zero;
            _center = Vector3.Zero;
            int verticies = 0;
            foreach(var mesh in _model.Meshes)
            {
                foreach(var part in mesh.MeshParts)
                {
                    int vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = part.NumVertices * vertexStride;

                    float[] vertexData = new float[part.NumVertices * vertexStride / sizeof(float)];
                    part.VertexBuffer.GetData(vertexData);

                    verticies += vertexBufferSize / sizeof(float);
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 pos = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                        _center += pos;
                        min = Vector3.Min(min, pos);
                        max = Vector3.Max(max, pos);
                    }
                }
            }
            _center /= verticies;
            ModelAABB = new BoundingBox(min, max);
        }

    }
}
