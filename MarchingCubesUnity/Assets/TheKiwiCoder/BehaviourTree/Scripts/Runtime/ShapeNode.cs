using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder
{
    public abstract class ShapeNode : GeometryNode
    {
        public override Port.Capacity? OutputPortCapacity => Port.Capacity.Multi;
    }
}