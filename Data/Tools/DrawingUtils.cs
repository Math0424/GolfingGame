using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data
{
    internal static class DrawingUtils
    {

        public static void DrawLine(GraphicsDevice graphics, Vector3 start, Vector3 dir, Color color)
        {
            var vertices = new[] {
                new VertexPositionColor(start, color), new VertexPositionColor(start + dir, color),
            };
            graphics.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public static void DrawMatrix(GraphicsDevice graphics, Matrix matrix)
        {
            DrawLine(graphics, matrix.Translation, Vector3.Normalize(matrix.Up), Color.Green);
            DrawLine(graphics, matrix.Translation, Vector3.Normalize(matrix.Right), Color.Red);
            DrawLine(graphics, matrix.Translation, Vector3.Normalize(matrix.Forward), Color.Blue);
        }


    }
}
