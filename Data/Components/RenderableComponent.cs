using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Data.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Components
{
    internal abstract class RenderableComponent : EntityComponent
    {
        public bool Visible = true;
        public bool Rendering = true;
        public abstract bool IsVisible(ref Camera frustum);
        public virtual void Draw3D(ref Matrix viewMatrix, ref Matrix projectionMatrix) { }
        public virtual void DebugDraw(ref SpriteBatch batch, ref Matrix viewMatrix, ref Matrix projectionMatrix) { }
    }
}
