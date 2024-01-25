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
        private BasicEffect basicEffect;
        private GraphicsDeviceManager _graphics;
        private Game _game;
        private CameraSystem _camera;

        private float _aspectRatio;

        public RenderingSystem(Game game)
        {
            _game = game;
            _graphics = new GraphicsDeviceManager(game);
        }

        public override void Initalize()
        {
            _aspectRatio = _game.GraphicsDevice.Viewport.AspectRatio;
            basicEffect = new BasicEffect(_game.GraphicsDevice);

            _camera = _world.GetSystem<CameraSystem>();
            if (_camera != null) 
                _camera.SetupProjection(_aspectRatio, 90);
        }

        public override void Draw(GameTime delta)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            var drawables = _world.GetEntityComponents<RenderableComponent>();
            
            basicEffect.View = _camera.ViewMatrix;
            basicEffect.Projection = _camera.ProjectionMatrix;
            basicEffect.LightingEnabled = true;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            foreach (var x in drawables)
                if (x.Visible && x.IsVisible(ref _camera.Frustum))
                    x.Draw(ref _camera.ViewMatrix, ref _camera.ProjectionMatrix);
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            var vertices = new[] { new VertexPosition(start), new VertexPosition(end) };
            _game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public override void Update(GameTime delta)
        {

        }
    }
}
