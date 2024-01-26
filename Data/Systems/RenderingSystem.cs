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
        public GraphicsDeviceManager Graphics { get; private set; }
        public float AspectRatio => _graphicsDevice.Viewport.AspectRatio;
        public Rectangle ScreenSize => _graphicsDevice.Viewport.Bounds;

        private GraphicsDevice _graphicsDevice;
        private BasicEffect _basicEffect;
        private Camera _camera;
        private SpriteFont _font;
        private SpriteBatch _debugSpriteBatch;
        private SpriteBatch _renderingSpriteBatch;

        private GameTime tickTime;
        private bool _debugMode;
        private World _world;

        public RenderingSystem(World world, Game game, Camera camera)
        {
            _camera = camera;
            _world = world;
            Graphics = new GraphicsDeviceManager(game);
            Graphics.DeviceCreated += GraphicInit;
        }

        private void GraphicInit(object sender, EventArgs e)
        {
            Console.WriteLine("Graphics Init");

            _graphicsDevice = Graphics.GraphicsDevice;
            _basicEffect = new BasicEffect(_graphicsDevice);
            _debugSpriteBatch = new SpriteBatch(_graphicsDevice);
            _renderingSpriteBatch = new SpriteBatch(_graphicsDevice);
            _font = _world.Game.Content.Load<SpriteFont>("Fonts/Debug");
            _camera.SetupProjection(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 90);
        }

        public void Draw(GameTime delta)
        {
            long timeNow = DateTime.Now.Ticks;

            _graphicsDevice.Clear(Color.CornflowerBlue);
            _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            var drawables = _world.GetEntityComponents<RenderableComponent>();

            _basicEffect.View = _camera.ViewMatrix;
            _basicEffect.Projection = _camera.ProjectionMatrix;
            _basicEffect.LightingEnabled = true;
            _basicEffect.TextureEnabled = true;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            int drawing = 0;
            _renderingSpriteBatch.Begin();
            // TODO : some sort of spatial partitioning
            // oct-tree or dynamic sectors
            foreach (var x in drawables)
            {
                x.Rendering = false;
                if (x.Visible && x.IsVisible(ref _camera))
                {
                    x.Rendering = true;
                    drawing++;
                    _basicEffect.CurrentTechnique.Passes[0].Apply();
                    x.Draw(ref _graphicsDevice, ref _camera);
                }
            }
            _renderingSpriteBatch.End();

            if (_debugMode)
            {
                _debugSpriteBatch.Begin();
                _basicEffect.CurrentTechnique.Passes[0].Apply();
                foreach (var x in drawables)
                    if (x.Rendering)
                        x.DebugDraw(ref _debugSpriteBatch, ref _graphicsDevice, ref _camera);

                long ticksTaken = (DateTime.Now.Ticks - timeNow) / 10000;

                _debugSpriteBatch.DrawString(_font, $"Rendering Debug:\n" +
                    $"Time: {Math.Round(delta.TotalGameTime.TotalMilliseconds / 1000, 2)}s\n" +
                    $"FPS: {Math.Round(delta.ElapsedGameTime.TotalSeconds * 1000, 2)}ms {Math.Round((ticksTaken / delta.ElapsedGameTime.TotalMilliseconds) * 100)}%\n" +
                    $"TPS: {Math.Round(tickTime.ElapsedGameTime.TotalSeconds * 1000, 2)}ms\n" +
                    $"Entities: {_world.EntityCount}\n" +
                    $"Drawn: {drawing}/{drawables.Count()}\n" +
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
