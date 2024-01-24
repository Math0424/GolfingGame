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
    internal class MeshComponent : EntityComponent
    {
        public BoundingBox BoundingBox
        {
            get; private set;
        }
        public string Model
        {
            get => _modelName;
            set => SetModel(_cm, value);
        }

        private string _modelName;
        private ContentManager _cm;
        private Model _model;

        public MeshComponent(ContentManager manager, string modelname)
        {
            _cm = manager;
            SetModel(_cm, modelname);
        }

        private void SetModel(ContentManager manager, string name)
        {
            _modelName = name;
            _model = manager.Load<Model>(name);
            CalculateBoundingBox();
        }

        private void CalculateBoundingBox()
        {
            Vector3 min = Vector3.Zero, max = Vector3.Zero;
            foreach(var mesh in _model.Meshes)
            {
                foreach(var part in mesh.MeshParts)
                {
                    int vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = part.NumVertices * vertexStride;

                    float[] vertexData = new float[part.NumVertices * vertexStride / sizeof(float)];
                    part.VertexBuffer.GetData(vertexData);

                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 pos = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                        min = Vector3.Min(min, pos);
                        max = Vector3.Max(max, pos);
                    }
                }
            }
            BoundingBox = new BoundingBox(min, max);
        }
    }
}
