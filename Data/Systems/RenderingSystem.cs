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

        public RenderingSystem(Game game)
        {
            Graphics = new GraphicsDeviceManager(game);
        }

        public override void Initalize()
        {
            _basicEffect = new BasicEffect(Graphics.GraphicsDevice);

            _camera = _world.GetSystem<CameraSystem>();
            if (_camera != null) 
                _camera.SetupProjection(AspectRatio, 90);
        }

        public void Draw(GameTime delta)
        {
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            var drawables = _world.GetEntityComponents<RenderableComponent>();
            
            _basicEffect.View = _camera.ViewMatrix;
            _basicEffect.Projection = _camera.ProjectionMatrix;
            _basicEffect.LightingEnabled = true;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            foreach (var x in drawables)
                if (x.Visible && x.IsVisible(ref _camera.Frustum))
                    x.Draw(ref _camera.ViewMatrix, ref _camera.ProjectionMatrix);
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            var vertices = new[] { new VertexPosition(start), new VertexPosition(end) };
            Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public override void Update(GameTime delta)
        {

        }
    }
}
