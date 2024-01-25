﻿using Microsoft.Xna.Framework;
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
        public abstract bool IsVisible(ref BoundingFrustum frustum);
        public abstract void Draw(ref Matrix viewMatrix, ref Matrix projectionMatrix);
    }
}