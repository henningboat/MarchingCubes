using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder
{
    public abstract class CombinerNode : GeometryNode
    {
        public override Port.Capacity? InputPortCapacity => Port.Capacity.Multi;
        public override Port.Capacity? OutputPortCapacity => Port.Capacity.Multi;
    }
}