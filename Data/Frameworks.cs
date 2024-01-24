using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data
{
    internal abstract class EntityComponent
    {
        public virtual void Close() {}
    }

    internal abstract class SystemComponent
    {
        protected World _world;
        public virtual void Initalize() {}
        public abstract void Draw(GameTime delta);
        public abstract void Update(GameTime delta);
        public virtual void Close() {}
    }
}
