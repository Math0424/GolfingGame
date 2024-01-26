using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    internal class PlayerMovement : SystemComponent
    {
        private float yaw = 0, pitch = 0;
        public bool Controlling = true;
        private int JustFocused = 0;

        public override void Initalize()
        {
            _world.GameFocused += CenterCursor;
        }

        public override void Close()
        {
            _world.GameFocused -= CenterCursor;
        }

        private void CenterCursor()
        {
            JustFocused = 5;
        }

        public override void Update(GameTime deltaTime)
        {
            if (!Controlling || !_world.Game.IsActive)
                return;

            var cam = _world.GetSystem<Camera>();
            float delta = (float)deltaTime.ElapsedGameTime.TotalSeconds;
            
            var bounds = _world.Game.GraphicsDevice.Viewport.Bounds;
            var mousePos = Mouse.GetState().Position;
            float xdelta = (mousePos.X - (bounds.Width / 2)) * delta * 3f;
            float ydelta = (mousePos.Y - (bounds.Height / 2)) * delta * 3f;
            Mouse.SetPosition(bounds.Width / 2, bounds.Height / 2);

            if (JustFocused > 0)
            {
                JustFocused--;
                return;
            }

            yaw -= xdelta;
            pitch -= ydelta;

            if (pitch > 89.0f)
                pitch = 89.0f;
            if (pitch < -89.0f)
                pitch = -89.0f;

            Matrix m = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), 0);
            m.Translation = cam.Translation;

            delta *= 2.0f;
            if (Input.IsKeyDown(Keys.LeftShift))
                delta *= 10.0f;

            if (Input.IsKeyDown(Keys.W))
                m.Translation += cam.Forward * delta;
            if (Input.IsKeyDown(Keys.S))
                m.Translation += cam.Backward * delta;

            if (Input.IsKeyDown(Keys.A))
                m.Translation += cam.Left * delta;
            if (Input.IsKeyDown(Keys.D))
                m.Translation += cam.Right * delta;

            if (Input.IsKeyDown(Keys.Space))
                m.Translation += cam.Up * delta;
            if (Input.IsKeyDown(Keys.C))
                m.Translation += cam.Down * delta;

            if (Input.MouseWheelDelta() != 0)
            {
                cam.SetFOV(cam.FOV - (Input.MouseWheelDelta() / 50f));
            }
            cam.SetWorldMatrix(m);
        }
    }
}
