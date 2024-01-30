using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Engine.Systems.GUI
{
    [Flags]
    internal enum ParentAlignments
    {
        None,
        Inner,
        Outer,

        Top,
        Bottom,
        Left,
        Right,

        Padding,
    }

    internal enum SizeAlignments
    {
        None = 0,
        Width = 1 << 0,
        Height = 1 << 1,
        Both = 3,
    }

}
