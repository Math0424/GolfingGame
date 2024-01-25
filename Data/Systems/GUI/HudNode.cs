using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data.Systems.GUI
{
    internal abstract class HudNode
    {
        public HudElement Parent { get; private set; }
        public bool Visible { get; set; }
        public bool InputEnabled { get; set; }
        public IReadOnlyList<HudNode> Children => _children;

        private List<HudNode> _children;
        private bool _registered;

        public HudNode(HudNode parent)
        {
            _children = new List<HudNode>();
            parent.AddChild(this);
        }

        public void AddChild(HudNode element)
        {
            if (element._registered)
                throw new InvalidOperationException("Element already added to another HudNode");
            element._registered = true;
            _children.Add(element);
        }

        public void RemoveChild(HudNode element)
        {
            if (element._registered && !_children.Contains(element))
                throw new InvalidOperationException("Element cannot be removed, different HudNode is owner");
            element._registered = false;
            _children.Remove(element);
        }

        public virtual void PreLayout(bool force)
        {
            if (Visible || force)
                Layout();
        }

        public virtual void PreDraw()
        {
            if (Visible)
                Draw();
        }

        public abstract void Layout();
        public abstract void Draw();
    }
}
