using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TheKiwiCoder
{
    public abstract class ShapeNode : GeometryNode
    {
        [SerializeField] private GeometryNodePort _geometryOutput;
        
        public override Port.Capacity? OutputPortCapacity => Port.Capacity.Multi;
        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            return new()
            {
                new GeometryNodePortDescription(serializedObject,nameof(_geometryOutput), "", Direction.Output, Port.Capacity.Multi),
            };
        }
    }
}