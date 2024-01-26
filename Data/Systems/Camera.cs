using Microsoft.Xna.Framework;
using Project1.Data.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    internal class Camera : SystemComponent
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
        private int _height;
        private int _width;

        public Vector3 Right => _worldMatrix.Right;
        public Vector3 Left => _worldMatrix.Left;
        public Vector3 Forward => _worldMatrix.Forward;
        public Vector3 Backward => _worldMatrix.Backward;
        public Vector3 Up => _worldMatrix.Up;
        public Vector3 Down => _worldMatrix.Down;
        public Vector3 Translation => _worldMatrix.Translation;

        public Camera()
        {
            _height = 480;
            _width = 800;
            SetViewMatrix(Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY));
            SetupProjection(_width, _height, 1);
        }

        public Camera SetupProjection(int width, int height, float FOV)
        {
            _aspectRatio = (float)width / height;
            this._height = height;
            this._width = width;
            this.FOV = FOV;

            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), _aspectRatio, 0.1f, 100f);
            return this;
        }

        public Camera SetupProjection(float FOV)
        {
            SetupProjection(_width, _height, FOV);
            return this;
        }

        public Camera SetupOrthographic(float width, float height)
        {
            _projectionMatrix = Matrix.CreateOrthographic(width, height, 0.1f, 100f);
            return this;
        }

        public void SetViewMatrix(Matrix matrix)
        {
            _viewMatrix = matrix;
            Matrix.Invert(ref _viewMatrix, out _worldMatrix);
            _frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
        }

        public void SetWorldMatrix(Matrix matrix)
        {
            _worldMatrix = matrix;
            Matrix.Invert(ref _worldMatrix, out _viewMatrix);
            _frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
        }

        public bool IsInFrustum(ref BoundingBox box)
        {
            return _frustum.Intersects(box);
        }

        public void SetFOV(float fov)
        {
            SetupProjection(_width, _height, Math.Clamp(fov, 1, 179));
        }

        public Vector2 WorldToScreen(ref Vector3 worldPos)
        {
            Vector3 pos = Vector3.Normalize(Vector3.Transform(worldPos, _viewMatrix * _projectionMatrix));
            return new Vector2((pos.X + 1) * 0.5f * _width, (1 - pos.Y) * 0.5f * _height);
        }

        // TODO : fix this
        public Vector3 ScreenToWorld(ref Vector3 screenPos)
        {
            Matrix matrix = Matrix.Invert(_worldMatrix * _viewMatrix * _projectionMatrix);
            return Vector3.Transform(screenPos, matrix);
        }

        public override void Update(GameTime delta)
        {

        }
    }


}
