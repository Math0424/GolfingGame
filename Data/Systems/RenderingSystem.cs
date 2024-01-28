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
    internal class RenderingSystem : SystemComponent, IDrawUpdate
    {
        public Action<SpriteBatch, GraphicsDevice, Camera> DebugDraw;

        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _basicEffect;
        private Camera _camera;
        private SpriteFont _font;
        private SpriteBatch _debugSpriteBatch;
        private SpriteBatch _spriteBatch;
        private SpriteEffect _spriteEffect;

        private GameTime tickTime;
        private bool _debugMode;
        private World _world;

        public RenderingSystem(World world, Game game, Camera camera)
        {
            _camera = camera;
            _world = world;
            _graphics = new GraphicsDeviceManager(game);
            world.AddInjectedType(_graphics);
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

            //_basicEffect.EnableDefaultLighting();
            _basicEffect.LightingEnabled = true;
            _basicEffect.AmbientLightColor = Vector3.One;

            _spriteEffect = new SpriteEffect(_graphicsDevice);

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
            _basicEffect.VertexColorEnabled = false;

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

            var sprites = _world.GetEntityComponents<SpriteComponent>();
            if (sprites != null)
            {
                _spriteEffect.CurrentTechnique.Passes[0].Apply();
                _spriteBatch.Begin();
                
                sprites = sprites.OrderBy(e => -e.ZDepth(ref _camera)).ToArray();
                foreach (var x in sprites)
                    if (x.IsVisible(ref _camera))
                        x.Draw(ref _spriteBatch, ref _graphicsDevice, ref _camera);
                
                _spriteBatch.End();
            }
            
            if (_debugMode)
            {
                _debugSpriteBatch.Begin();

                Vector3 prevAmbientColor = _basicEffect.AmbientLightColor;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.TextureEnabled = false;
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.AmbientLightColor = Vector3.One;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                DebugDraw?.Invoke(_debugSpriteBatch, _graphicsDevice, _camera);
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

                _basicEffect.AmbientLightColor = prevAmbientColor;
                _debugSpriteBatch.End();
            }

            // I fixed it, hah, im going crazy----
            // this took too fucking long to figure out
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
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
