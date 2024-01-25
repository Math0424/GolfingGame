using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    internal class RenderingSystem : SystemComponent
    {
        public GraphicsDeviceManager Graphics { get; private set; }
        public float AspectRatio => Graphics.GraphicsDevice.Viewport.AspectRatio;
        public Rectangle ScreenSize => Graphics.GraphicsDevice.Viewport.Bounds;

        private BasicEffect _basicEffect;
        private CameraSystem _camera;
        private SpriteFont _font;
        private SpriteBatch _spriteBatch;

        private GameTime tickTime;

        public RenderingSystem(Game game)
        {
            Graphics = new GraphicsDeviceManager(game);
        }

        public override void Initalize()
        {
            _basicEffect = new BasicEffect(Graphics.GraphicsDevice);
            _font = _world.Game.Content.Load<SpriteFont>("Fonts/Debug");
            _spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);

            _camera = _world.GetSystem<CameraSystem>();
            if (_camera != null)
                _camera.SetupProjection(Graphics.GraphicsDevice.Viewport.Width, Graphics.GraphicsDevice.Viewport.Height, 90);
        }

        public void Draw(GameTime delta)
        {
            long timeNow = DateTime.Now.Ticks;

            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            var drawables = _world.GetEntityComponents<RenderableComponent>();
            
            _basicEffect.View = _camera.ViewMatrix;
            _basicEffect.Projection = _camera.ProjectionMatrix;
            _basicEffect.LightingEnabled = true;
            _basicEffect.TextureEnabled = true;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            _spriteBatch.Begin();
            int drawing = 0;
            foreach (var x in drawables)
            {
                if (x.Visible && x.IsVisible(ref _camera.Frustum))
                {
                    drawing++;
                    x.Draw3D(ref _camera.ViewMatrix, ref _camera.ProjectionMatrix);
                    x.DebugDraw(ref _spriteBatch, ref _camera.ViewMatrix, ref _camera.ProjectionMatrix);
                }
            }

            long ticksTaken = (DateTime.Now.Ticks - timeNow) / 10000;

            _spriteBatch.DrawString(_font, $"Rendering Debug:\n" +
                $"Time: {Math.Round(delta.TotalGameTime.TotalMilliseconds / 1000, 2)}s\n" +
                $"FPS: {Math.Round(delta.ElapsedGameTime.TotalSeconds * 1000, 2)}ms {Math.Round((ticksTaken / delta.ElapsedGameTime.TotalMilliseconds) * 100)}%\n" +
                $"TPS: {Math.Round(tickTime.ElapsedGameTime.TotalSeconds * 1000, 2)}ms\n" +
                $"Entities: {_world.EntityCount}\n" +
                $"Drawn: {drawing}/{drawables.Count()}\n" +
                $"Pos: [{Math.Round(_camera.Translation.X, 2)}, {Math.Round(_camera.Translation.Y, 2)}, {Math.Round(_camera.Translation.Z, 2)}]", 
                new Vector2(0, 0), Color.Yellow);
            _spriteBatch.End();
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            var vertices = new[] { new VertexPosition(start), new VertexPosition(end) };
            _world.Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public override void Update(GameTime delta)
        {
            tickTime = delta;
        }
    }
}
