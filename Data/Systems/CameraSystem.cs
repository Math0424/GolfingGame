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
        public ref BoundingFrustum Frustum => ref _frustum;
        public ref Matrix ViewMatrix => ref _viewMatrix;
        public ref Matrix WorldMatrix => ref _worldMatrix;
        public ref Matrix ProjectionMatrix => ref _projectionMatrix;
        public float FOV { get; private set; }

        private BoundingFrustum _frustum;
        private Matrix _viewMatrix;
        private Matrix _worldMatrix;
        private Matrix _projectionMatrix;
        private float _aspectRatio;

        public Vector3 Right => _worldMatrix.Right;
        public Vector3 Left => _worldMatrix.Left;
        public Vector3 Forward => _worldMatrix.Forward;
        public Vector3 Backward => _worldMatrix.Backward;
        public Vector3 Up => _worldMatrix.Up;
        public Vector3 Down => _worldMatrix.Down;
        public Vector3 Translation => _worldMatrix.Translation;

        public CameraSystem(bool smoothing)
        {
            SetViewMatrix(Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY));
            SetupProjection(90, 1);
        }

        public CameraSystem SetupProjection(float aspectRatio, float FOV)
        {
            _aspectRatio = aspectRatio;
            this.FOV = FOV;
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspectRatio, 0.1f, 1000f);
            return this;
        }

        public CameraSystem SetupProjection(float FOV)
        {
            SetupProjection(_aspectRatio, FOV);
            return this;
        }

        public CameraSystem SetupOrthographic(float width, float height)
        {
            _projectionMatrix = Matrix.CreateOrthographic(width, height, 0.1f, 1000f);
            return this;
        }

        public void SetViewMatrix(Matrix matrix)
        {
            _viewMatrix = matrix;
            _worldMatrix = Matrix.Invert(_viewMatrix);
            _frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
        }

        public void SetWorldMatrix(Matrix matrix)
        {
            _worldMatrix = matrix;
            _viewMatrix = Matrix.Invert(_worldMatrix);
            _frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
        }

        public bool IsInFrustum(ref BoundingBox box)
        {
            return _frustum.Intersects(box);
        }

        public void SetFOV(float fov)
        {
            SetupProjection(_aspectRatio, Math.Clamp(fov, 1, 179));
        }

        public Vector3 WorldToScreen(ref Vector3 worldPos)
        {
            return Vector3.Transform(worldPos, _viewMatrix);
        }

        public Vector3 ScreenToWorld(ref Vector3 screenPos)
        {
            Matrix matrix = Matrix.Invert(_worldMatrix * _viewMatrix * _projectionMatrix);
            return Vector3.Transform(screenPos, matrix);
        }

        public override void Draw(GameTime delta)
        {

        }

        public override void Update(GameTime delta)
        {

        }
    }


}
