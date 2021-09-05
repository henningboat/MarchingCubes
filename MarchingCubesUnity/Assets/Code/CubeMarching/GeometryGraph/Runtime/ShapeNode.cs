using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public abstract class ShapeNode : GeometryNode
    {
        [HideInInspector] [SerializeField] private GeometryNodePort _geometryOutput;

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            var serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new GeometryNodePortDescription(serializedObject, nameof(_geometryOutput), "", Direction.Output, Port.Capacity.Multi)
            };
        }
    }
}