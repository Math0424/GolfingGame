﻿using Microsoft.Xna.Framework;
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
        public ref Matrix TransformMatrix => ref _transformMatrix;
        public Vector3 Position => _worldMatrix.Translation;

        private Matrix _transformMatrix;
        private Matrix _localMatrix;
        private Matrix _worldMatrix;

        public PositionComponent(Vector3 Pos) : this()
        {
            _worldMatrix.Translation = Pos;
            UpdateTransformMatrix();
        }

        public PositionComponent()
        {
            _localMatrix = Matrix.Identity;
            _worldMatrix = Matrix.Identity;
            UpdateTransformMatrix();
        }

        public void SetWorldMatrix(Matrix matrix)
        {
            _worldMatrix = matrix;
            UpdateTransformMatrix();
        }

        public void SetLocalMatrix(Matrix matrix)
        {
            _localMatrix = matrix;
            UpdateTransformMatrix();
        }

        public void SetPosition(Vector3 vector)
        {
            _worldMatrix.Translation = vector;
            UpdateTransformMatrix();
        }

        public void Scale(float scale)
        {
            Matrix s = Matrix.CreateScale(scale);
            _localMatrix *= s;
            UpdateTransformMatrix();
        }

        private void UpdateTransformMatrix()
        {
            _transformMatrix = _localMatrix * _worldMatrix;
        }

    }
}
