using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project1.Engine.Components;
using Project1.Engine.Systems.RenderMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Engine.Systems
{
    internal class RenderingSystem : SystemComponent, IDrawUpdate
    {
        public Camera Camera { get; private set; }
        public Vector2 ScreenBounds { get; private set; }
        public Action DoDraw;

        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _basicEffect;
        private SpriteFont _font;
        private SpriteBatch _debugSpriteBatch;
        private SpriteBatch _spriteBatch;
        private SpriteEffect _spriteEffect;


        private GameTime tickTime;
        private bool _debugMode;
        private World _world;
        private Game _game;

        private List<RenderMessage> _renderMessages;

        public RenderingSystem(World world, Game game, Camera camera)
        {
            _game = game;
            Camera = camera;
            _world = world;
            _graphics = new GraphicsDeviceManager(game);
            world.AddInjectedType(_graphics);
            _graphics.DeviceCreated += GraphicInit;
            _graphics.DeviceDisposing += StopGraphics;

            _renderMessages = new List<RenderMessage>();
            _meshes = new Dictionary<string, Model>();
            _fonts = new Dictionary<string, SpriteFont>();
            _textures = new Dictionary<string, Texture2D>();
        }

        private void StopGraphics(object sender, EventArgs e)
        {
            Console.WriteLine($"Graphics Disposed");
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
            EnqueueMessage(new RenderMessageLoadFont("Fonts/Debug"));

            ScreenBounds = new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            Camera.SetupProjection(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 90);
            //_camera.SetupOrthographic(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        }

        // TODO move draw to a different thread away from the main gameloop
        public void Draw(GameTime delta)
        {
            long timeNow = DateTime.Now.Ticks;
            _graphicsDevice.Clear(Color.CornflowerBlue);

            DoDraw?.Invoke();

            var drawables = _world.GetEntityComponents<RenderableComponent>();

            int rendering = 0;
            // TODO : some sort of spatial partitioning
            // oct-tree or dynamic sectors

            var camera = Camera;
            foreach (var x in drawables)
            {
                if (x.Visible && x.IsVisible(ref camera))
                {
                    rendering++;
                    x.Draw(this, ref camera);
                }
            }

            int renderMessageCount = _renderMessages.Count;
            ProcessRenderMessages();
            _renderMessages.Clear();

            if (_debugMode)
            {
                _debugSpriteBatch.Begin();

                Vector3 prevAmbientColor = _basicEffect.AmbientLightColor;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.TextureEnabled = false;
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.AmbientLightColor = Vector3.One;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                long ticksTaken = (DateTime.Now.Ticks - timeNow) / 10000;
                
                _debugSpriteBatch.DrawString(_font, $"Rendering Debug:\n" +
                    $"World: {_world.WorldName}\n" +
                    $"Time: {Math.Round(delta.TotalGameTime.TotalMilliseconds / 1000, 2)}s\n" +
                    $"FPS: {Math.Round(delta.ElapsedGameTime.TotalSeconds * 1000, 2)}ms {Math.Round((ticksTaken / delta.ElapsedGameTime.TotalMilliseconds) * 100)}%\n" +
                    $"TPS: {Math.Round(tickTime.ElapsedGameTime.TotalSeconds * 1000, 2)}ms\n" +
                    $"Entities: {_world.EntityCount}\n" +
                    $"Drawn: {rendering}/{drawables.Count()}\n" +
                    $"DrawCalls: {renderMessageCount} / {_graphicsDevice.Metrics.DrawCount}\n" +
                    $"Triangles: {_graphicsDevice.Metrics.PrimitiveCount}\n" +
                    $"Textures: {_graphicsDevice.Metrics.TextureCount}\n" +
                    $"Pos: [{Math.Round(Camera.Translation.X, 2)}, {Math.Round(Camera.Translation.Y, 2)}, {Math.Round(Camera.Translation.Z, 2)}]",
                    new Vector2(0, 0), Color.Yellow);

                _basicEffect.AmbientLightColor = prevAmbientColor;
                _debugSpriteBatch.End();
            }
        }


        private Dictionary<string, Model> _meshes;
        private Dictionary<string, Texture2D> _textures;
        private Dictionary<string, SpriteFont> _fonts;

        private static VertexPositionTexture[] _quadVertexPositionTexture = new[]
        {
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, -1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, -1)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
        };
        private static short[] _quadVertexIndicesNoBack = new short[] { 0, 1, 2, 2, 3, 0 };
        private static short[] _quadVertexIndices = new short[] { 0, 1, 2, 2, 3, 0, 0, 3, 2, 2, 1, 0 };

        private void ProcessRenderMessages()
        {
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Camera.ViewMatrix;
            _basicEffect.Projection = Camera.ProjectionMatrix;
            _basicEffect.TextureEnabled = true;
            _basicEffect.VertexColorEnabled = false;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            _basicEffect.CurrentTechnique.Passes[0].Apply();

            RenderMessage[] arr = _renderMessages.ToArray();
            foreach (var message in arr)
            {
                switch (message.Type)
                {
                    case RenderMessageType.LoadMesh:
                        var loadMesh = (RenderMessageLoadMesh)message;
                        _meshes[loadMesh.Model] = _game.Content.Load<Model>(loadMesh.Model);
                        break;
                    case RenderMessageType.LoadFont:
                        var loadFont = (RenderMessageLoadFont)message;
                        _fonts[loadFont.Font] = _game.Content.Load<SpriteFont>(loadFont.Font);
                        break;
                    case RenderMessageType.LoadTexture:
                        var loadTexture = (RenderMessageLoadTexture)message;
                        _textures[loadTexture.Texture] = _game.Content.Load<Texture2D>(loadTexture.Texture);
                        break;
                    case RenderMessageType.DrawMesh:
                        var drawMesh = (RenderMessageDrawMesh)message;
                        _basicEffect.VertexColorEnabled = false;
                        _basicEffect.TextureEnabled = true;
                        _meshes[drawMesh.Model].Draw(drawMesh.Matrix, Camera.ViewMatrix, Camera.ProjectionMatrix);
                        break;
                    case RenderMessageType.DrawLine:
                        var drawLine = (RenderMessageDrawLine)message;
                        _basicEffect.VertexColorEnabled = true;
                        _basicEffect.TextureEnabled = false;
                        _basicEffect.CurrentTechnique.Passes[0].Apply();
                        var coloredLineVertices = new[] {
                            new VertexPositionColor(drawLine.Pos1, drawLine.Color), new VertexPositionColor(drawLine.Pos2, drawLine.Color),
                        };
                        _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, coloredLineVertices, 0, 1);
                        break;
                    case RenderMessageType.DrawQuad:
                        var drawQuad = (RenderMessageDrawQuad)message;
                        _basicEffect.VertexColorEnabled = false;
                        _basicEffect.TextureEnabled = true;
                        _basicEffect.Texture = _textures[drawQuad.Texture];
                        _basicEffect.World = drawQuad.Matrix;
                        _basicEffect.CurrentTechnique.Passes[0].Apply();
                        if (drawQuad.DrawBack)
                            _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _quadVertexPositionTexture, 0, 4, _quadVertexIndices, 0, 4);
                        else
                            _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _quadVertexPositionTexture, 0, 4, _quadVertexIndicesNoBack, 0, 2);
                        _basicEffect.View = Camera.ViewMatrix;
                        break;
                }
            }

            RenderMessage[] depthSpriteBatch = _renderMessages.Where(e => e.Type == RenderMessageType.DrawSprite || e.Type == RenderMessageType.DrawText).OrderBy(e => -((RenderMessageDepth)e).Depth).ToArray();
            _spriteEffect.CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Begin();
            foreach (var sprite in depthSpriteBatch)
            {
                switch(sprite.Type)
                {
                    case RenderMessageType.DrawSprite:
                        var drawSprite = (RenderMessageDrawSprite)sprite;
                        _spriteBatch.Draw(_textures[drawSprite.Texture], drawSprite.Rectangle, Color.White);
                        break;
                    case RenderMessageType.DrawText:
                        var drawText = (RenderMessageDrawText)sprite;
                        _spriteBatch.DrawString(_fonts[drawText.Font], drawText.Text, drawText.Pos, drawText.Color);
                        break;
                }
            }
            _spriteBatch.End();

        }

        public void EnqueueMessage(RenderMessage message)
        {
            _renderMessages.Add(message);
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
