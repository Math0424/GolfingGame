using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Systems;
using Project1.Data.Systems.RenderMessages;
using Project1.Data.Tools;
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
        public float BoundingSphereRadius;
    }

    internal class MeshComponent : RenderableComponent
    {
        // TODO : possible memory leak if lots unique models are created and then never used again
        private static Dictionary<string, ModelInfo> cache = new Dictionary<string, ModelInfo>();

        public ref ModelInfo Model => ref _info;
        public BoundingBox AABB => _AABB;
        public string ModelName
        {
            get => _info.Name;
            set => SetModel(value);
        }

        private ModelInfo _info;
        private string _modelName;
        private BoundingBox _AABB;

        public MeshComponent(string modelname)
        {
            _modelName = modelname;
        }

        private void UpdateAABB()
        {
            var m = _entity.Position.TransformMatrix;
            BoundingBox bb = Model.BoundingBox;
            for (int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    float e = m[i, j] * bb.Min.GetIndex(j);
                    float f = m[i, j] * bb.Max.GetIndex(j);
                    if (e < f)
                    {
                        bb.Min.SetIndex(i, bb.Min.GetIndex(i) + e);
                        bb.Max.SetIndex(i, bb.Max.GetIndex(i) + f);
                    } 
                    else
                    {
                        bb.Min.SetIndex(i, bb.Min.GetIndex(i) + f);
                        bb.Max.SetIndex(i, bb.Max.GetIndex(i) + e);
                    }
                }
            }
            bb.Min += m.Translation;
            bb.Max += m.Translation;
            _AABB = bb;
        }

        public override void Initalize()
        {
            _entity.World.Render.EnqueueMessage(new RenderMessageLoadMesh(_modelName));
            _entity.Position.UpdatedTransforms += UpdateAABB;
            SetModel(_modelName);
        }

        public override void Close()
        {
            _entity.Position.UpdatedTransforms -= UpdateAABB;
        }

        public override bool IsVisible(ref Camera cam)
        {
            // I want to cull everything behind the ViewMatrix, but something is wrong~
            // float dist = Vector3.Dot(cam.WorldMatrix.Forward, _entity.Position.Position - ModelCenter) + Vector3.Dot(cam.Forward, cam.Translation);
            // if (dist > -_info.Radius)
            // {
            //     Console.WriteLine($"Behind plane {dist} ({_info.Radius})");
            //     return false;
            // }

            return cam.Frustum.Intersects(AABB);
        }

        public override void Draw(RenderingSystem rendering, ref Camera cam)
        {
            rendering.EnqueueMessage(new RenderMessageDrawMesh(_modelName, _entity.Position.TransformMatrix));
        }

        // public void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam)
        // {
        //     var pos = _entity.Position;
        //     Vector3[] corners = AABB.GetCorners();
        // 
        //     //draw bounding box
        //     var vertices = new[] {
        //         new VertexPosition(corners[0]), new VertexPosition(corners[1]),
        //         new VertexPosition(corners[1]), new VertexPosition(corners[2]),
        //         new VertexPosition(corners[2]), new VertexPosition(corners[3]),
        //         new VertexPosition(corners[0]), new VertexPosition(corners[3]),
        //         new VertexPosition(corners[4]), new VertexPosition(corners[5]),
        //         new VertexPosition(corners[5]), new VertexPosition(corners[6]),
        //         new VertexPosition(corners[6]), new VertexPosition(corners[7]),
        //         new VertexPosition(corners[4]), new VertexPosition(corners[7]),
        //         new VertexPosition(corners[0]), new VertexPosition(corners[4]),
        //         new VertexPosition(corners[1]), new VertexPosition(corners[5]),
        //         new VertexPosition(corners[2]), new VertexPosition(corners[6]),
        //         new VertexPosition(corners[3]), new VertexPosition(corners[7])
        //     };
        //     graphics.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);
        // 
        //     Vector3 posx = pos.Position;
        //     Vector3 screen = cam.WorldToScreen(ref posx);
        //     batch.DrawString(_font, $"Mesh\nID: {_entity.Id}\nV:{Model.Verticies}", new Vector2(screen.X, screen.Y), Color.Black, 0, Vector2.Zero, 1 - screen.Z, default, 0);
        // }

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
            int verticies = 0;
            foreach(var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                    verticies += part.NumVertices;

                int vertexStride = mesh.MeshParts[0].VertexBuffer.VertexDeclaration.VertexStride;
                float[] vertexData = new float[verticies * vertexStride / sizeof(float)];
                mesh.MeshParts[0].VertexBuffer.GetData(vertexData);

                for (int i = 0; i < vertexData.Length; i += vertexStride / sizeof(float))
                {
                    Vector3 pos = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
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
                BoundingSphereRadius = Vector3.Distance(min, max),
            };
        }

    }
}
