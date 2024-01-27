using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _basicEffect;
        private Camera _camera;
        private SpriteFont _font;
        private SpriteBatch _debugSpriteBatch;
        private SpriteBatch _spriteBatch;

        private GameTime tickTime;
        private bool _debugMode;
        private World _world;

        public RenderingSystem(World world, Game game, Camera camera)
        {
            _camera = camera;
            _world = world;
            _graphics = new GraphicsDeviceManager(game);
            _graphics.DeviceCreated += GraphicInit;
        }

        private void GraphicInit(object sender, EventArgs e)
        {
            Console.WriteLine($"Graphics Init");

            _graphics = (GraphicsDeviceManager)sender;
            _graphicsDevice = _graphics.GraphicsDevice;
            _basicEffect = new BasicEffect(_graphicsDevice);

            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _debugSpriteBatch = new SpriteBatch(_graphicsDevice);

            _basicEffect.EnableDefaultLighting();
            _basicEffect.AmbientLightColor *= 2;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _font = _world.Game.Content.Load<SpriteFont>("Fonts/Debug");
            _camera.SetupProjection(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 90);
            //_camera.SetupOrthographic(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        }

        public void Draw(GameTime delta)
        {
            long timeNow = DateTime.Now.Ticks;

            _graphicsDevice.Clear(Color.CornflowerBlue);

            var drawables = _world.GetEntityComponents<RenderableComponent>();
            List<RenderableComponent> rendering = new List<RenderableComponent>(drawables.Length);

            _basicEffect.View = _camera.ViewMatrix;
            _basicEffect.Projection = _camera.ProjectionMatrix;
            _basicEffect.TextureEnabled = true;

            // TODO : some sort of spatial partitioning
            // oct-tree or dynamic sectors
            foreach (var x in drawables)
            {
                if (x.Visible && x.IsVisible(ref _camera))
                {
                    rendering.Add(x);
                    x.Draw(ref _basicEffect, ref _graphicsDevice, ref _camera);
                }
            }

            // sprite effect + sprite batch
            //_spriteEffect.CurrentTechnique.Passes[0].Apply();

            //_spriteBatch.Begin();
            //sprites = sprites.OrderBy(e => -e.ZDepth(ref _camera)).ToList();
            //foreach (var x in sprites)
            //    x.Draw(ref _basicEffect, ref _graphicsDevice, ref _camera);
            //_spriteBatch.End();

            //foreach (var p in _basicEffect.CurrentTechnique.Passes)
            //{
            //    //p.Apply();
            //    foreach (var x in drawables)
            //        if (x.Rendering)
            //            x.Draw(ref _graphicsDevice, ref _camera);
            //}

            if (_debugMode)
            {
                _debugSpriteBatch.Begin();

                _basicEffect.World = Matrix.Identity;
                _basicEffect.TextureEnabled = false;
                _basicEffect.CurrentTechnique.Passes[0].Apply();
                
                foreach (var x in rendering)
                    x.DebugDraw(ref _debugSpriteBatch, ref _graphicsDevice, ref _camera);
                
                long ticksTaken = (DateTime.Now.Ticks - timeNow) / 10000;
                
                _debugSpriteBatch.DrawString(_font, $"Rendering Debug:\n" +
                    $"World: {_world.WorldName}\n" +
                    $"Time: {Math.Round(delta.TotalGameTime.TotalMilliseconds / 1000, 2)}s\n" +
                    $"FPS: {Math.Round(delta.ElapsedGameTime.TotalSeconds * 1000, 2)}ms {Math.Round((ticksTaken / delta.ElapsedGameTime.TotalMilliseconds) * 100)}%\n" +
                    $"TPS: {Math.Round(tickTime.ElapsedGameTime.TotalSeconds * 1000, 2)}ms\n" +
                    $"Entities: {_world.EntityCount}\n" +
                    $"Drawn: {rendering.Count()}/{drawables.Count()}\n" +
                    $"DrawCount: {_graphicsDevice.Metrics.DrawCount}\n" +
                    $"Triangles: {_graphicsDevice.Metrics.PrimitiveCount}\n" +
                    $"Textures: {_graphicsDevice.Metrics.TextureCount}\n" +
                    $"Pos: [{Math.Round(_camera.Translation.X, 2)}, {Math.Round(_camera.Translation.Y, 2)}, {Math.Round(_camera.Translation.Z, 2)}]",
                    new Vector2(0, 0), Color.Yellow);
                
                _debugSpriteBatch.End();
            }
        }

        public override void Update(GameTime delta)
        {
            tickTime = delta;
            if (Input.IsNewKeyDown(Keys.F11))
            {
                _debugMode = !_debugMode;
            }
        }
    }
}
