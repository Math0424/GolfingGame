﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Systems;
using Project1.Data.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data
{
    internal static class DrawingUtils
    {

        public static void DrawLine(RenderingSystem graphics, Vector3 start, Vector3 dir, Color color)
        {
            graphics.EnqueueMessage(new RenderMessageDrawLine(start, start + dir, color));
        }

        public static void DrawMatrix(RenderingSystem render, Matrix matrix)
        {
            DrawLine(render, matrix.Translation, Vector3.Normalize(matrix.Up), Color.Green);
            DrawLine(render, matrix.Translation, Vector3.Normalize(matrix.Right), Color.Red);
            DrawLine(render, matrix.Translation, Vector3.Normalize(matrix.Forward), Color.Blue);
        }

        public static void DrawWorldText(RenderingSystem graphics, string text, Vector3 worldPos, Color color)
        {
            Vector3 screen = graphics.Camera.WorldToScreen(ref worldPos);
            graphics.EnqueueMessage(new RenderMessageDrawText("Fonts/Debug", text, 12, 1 / screen.Z, new Vector2(screen.X, screen.Y), color));
        }

    }
}
