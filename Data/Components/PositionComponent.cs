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
        public ref Matrix LocalMatrix
        {
            get => ref _localMatrix;
        }

        public ref Matrix WorldMatrix
        {
            get => ref _worldMatrix;
        }

        public ref Vector3 Position
        {
            get => ref _position;
        }

        private Matrix _localMatrix;
        private Matrix _worldMatrix;
        private Vector3 _position;

        public PositionComponent()
        {
            _localMatrix = Matrix.Identity;
            _worldMatrix = Matrix.Identity;
            _position = Vector3.Zero;
        }

        public void SetWorldMatrix(Matrix matrix)
        {
            _worldMatrix = matrix;
        }

        public void SetLocalMatrix(Matrix matrix)
        {
            _localMatrix = matrix;
        }

        public void SetPosition(Vector3 vector)
        {
            _position = vector;
        }

    }
}
