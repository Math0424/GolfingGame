using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.RenderMessages
{
    enum RenderMessageType
    {
        None,

        LoadMesh,
        LoadFont,
        LoadTexture,

        DrawSprite,
        DrawText,

        DrawMesh,
        DrawLine,
        DrawQuad,
    }

    internal abstract class RenderMessage
    {
        public RenderMessageType Type { get; private set; }
        public RenderMessage(RenderMessageType type)
        {
            this.Type = type;
        }
    }
}
