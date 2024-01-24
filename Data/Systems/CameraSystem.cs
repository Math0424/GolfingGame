using Microsoft.Xna.Framework;
using Project1.Data.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    internal class CameraSystem : SystemComponent
    {
        public ref Matrix ViewMatrix { get => ref _viewMatrix; }
        public ref Matrix WorldMatrix { get => ref _worldMatrix; }
        public ref Matrix ProjectionMatrix { get => ref _projectionMatrix; }

        private Matrix _viewMatrix;
        private Matrix _worldMatrix;
        private Matrix _projectionMatrix;
        private float _aspectRatio;

        public CameraSystem(bool smoothing)
        {
            _worldMatrix = Matrix.CreateTranslation(0, 0, 0);
            _viewMatrix = Matrix.CreateLookAt(_worldMatrix.Translation, new Vector3(0, 0, 1), -Vector3.UnitY);
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1, 0.1f, 1000f);
        }

        public CameraSystem SetupProjection(float aspectRatio, float FOV)
        {
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspectRatio, 0.1f, 1000f);
            return this;
        }

        public CameraSystem SetupOrthographic(float width, float height)
        {
            _projectionMatrix = Matrix.CreateOrthographic(width, height, 0.1f, 1000f);
            return this;
        }

        public bool IsVisible(Entity ent)
        {
            PositionComponent comp = ent.GetComponent<PositionComponent>();
            if (comp != null)
            {
                
            }
            return false;
        }

        public override void Draw(GameTime delta)
        {

        }

        public override void Update(GameTime delta)
        {

        }
    }


}
