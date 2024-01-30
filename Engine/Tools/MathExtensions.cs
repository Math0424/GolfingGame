using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine
{
    internal static class MathExtensions
    {

        public static Matrix CrossMatrix(this Vector3 v)
        {
            return new Matrix(0, v.Z, -v.Y, 0, 
                              -v.Z, 0, v.X, 0, 
                              v.Y, -v.X, 0, 0, 
                              0,   0,   0,  0);
        }

        public static float GetIndex(this Vector3 v, int i)
        {
            switch(i)
            {
                case 0:
                    return v.X;
                case 1:
                    return v.Y;
                case 2:
                    return v.Z;
            }
            throw new Exception($"{i} is outside vector bounds");
        }

        public static void SetIndex(this ref Vector3 v, int i, float value)
        {
            switch (i)
            {
                case 0:
                    v.X = value;
                    return;
                case 1:
                    v.Y = value;
                    return;
                case 2:
                    v.Z = value;
                    return;
            }
            throw new Exception($"{i} is outside vector bounds");
        }

    }
}
