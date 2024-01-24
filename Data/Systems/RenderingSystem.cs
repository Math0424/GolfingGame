using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems
{
    internal class RenderingSystem : SystemComponent
    {
        private SpriteBatch _spriteBatch;
        private GraphicsDeviceManager _graphics;
        private Game _game;
        private CameraSystem _camera;

        private float _FOV;
        private float _aspectRatio;

        public RenderingSystem(Game game)
        {
            _game = game;
            _graphics = new GraphicsDeviceManager(game);
        }

        public override void Initalize()
        {
            _aspectRatio = _game.GraphicsDevice.Viewport.AspectRatio;
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            _camera = _world.GetSystem<CameraSystem>();
            if (_camera != null) 
            {
                _camera.SetupProjection(_aspectRatio, 90);
            }
        }

        public override void Draw(GameTime delta)
        {
            _game.GraphicsDevice.Clear(Color.CornflowerBlue);

        }

        public override void Update(GameTime delta)
        {

        }
    }
}
