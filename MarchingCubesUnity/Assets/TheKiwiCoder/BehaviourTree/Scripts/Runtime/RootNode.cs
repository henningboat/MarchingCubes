using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TheKiwiCoder
{
    public class RootNode : GeometryNode
    {
        public override Port.Capacity? InputPortCapacity => Port.Capacity.Single;
    }
}