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
        private int _prevScrollWheel = 0;

        public override void Draw(GameTime delta) { }

        public override void Update(GameTime deltaTime)
        {
            var cam = _world.GetSystem<CameraSystem>();
            Matrix m = cam.WorldMatrix;

            float delta = (float)deltaTime.ElapsedGameTime.TotalSeconds;
            var bounds = GolfGame.Instance.GraphicsDevice.Viewport.Bounds;
            var mousePos = Mouse.GetState().Position;

            float xdelta = (mousePos.X - (bounds.Width / 2)) * delta * .03f;
            float ydelta = (mousePos.Y - (bounds.Height / 2)) * delta * .03f;

            Mouse.SetPosition(bounds.Width / 2, bounds.Height / 2);

            m.Translation = Vector3.Zero;
            m *= Matrix.CreateFromYawPitchRoll(xdelta, ydelta, 0);
            m.Translation = cam.Translation;

            delta *= 2.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                delta *= 2.0f;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                m.Translation += cam.Forward * delta;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                m.Translation += cam.Backward * delta;

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                m.Translation += cam.Left * delta;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                m.Translation += cam.Right * delta;

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                m.Translation += cam.Up * delta;
            if (Keyboard.GetState().IsKeyDown(Keys.C))
                m.Translation += cam.Down * delta;

            if (Mouse.GetState().ScrollWheelValue != _prevScrollWheel)
            {
                cam.SetFOV(cam.FOV + ((_prevScrollWheel - Mouse.GetState().ScrollWheelValue) / 50f));
                _prevScrollWheel = Mouse.GetState().ScrollWheelValue;
            }
            cam.SetWorldMatrix(m);
        }
    }
}
