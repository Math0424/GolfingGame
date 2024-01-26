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
        public abstract bool IsVisible(ref Camera cam);
        public virtual void Draw(ref GraphicsDevice batch, ref Camera cam) { }
        public virtual void DebugDraw(ref SpriteBatch batch, ref GraphicsDevice graphics, ref Camera cam) { }
    }
}
