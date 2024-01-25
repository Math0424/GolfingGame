using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Components
{
    internal class PositionComponent : EntityComponent
    {
        // avoid passing struct clones, use refs
        public ref Matrix LocalMatrix => ref _localMatrix;
        public ref Matrix WorldMatrix => ref _worldMatrix;
        public ref Vector3 Position => ref _position;
        public ref Matrix TransformMatrix => ref _transformMatrix;

        private Matrix _transformMatrix;
        private Matrix _localMatrix;
        private Matrix _worldMatrix;
        private Vector3 _position;

        public PositionComponent()
        {
            _localMatrix = Matrix.Identity;
            _worldMatrix = Matrix.Identity;
            _position = Vector3.Zero;
            _transformMatrix = _localMatrix * _worldMatrix;
        }

        public void SetWorldMatrix(Matrix matrix)
        {
            _worldMatrix = matrix;
            _transformMatrix = _localMatrix * _worldMatrix;
        }

        public void SetLocalMatrix(Matrix matrix)
        {
            _localMatrix = matrix;
            _transformMatrix = _localMatrix * _worldMatrix;
        }

        public void SetPosition(Vector3 vector)
        {
            _position = vector;
        }

        public void Scale(float scale)
        {
            Matrix s = Matrix.CreateScale(scale);
            _localMatrix *= s;
            _transformMatrix = _localMatrix * _worldMatrix;
        }

    }
}
