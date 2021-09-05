using System.Collections.Generic;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public abstract class CombinerNode : GeometryNode
    {
        [HideInInspector] [SerializeField] protected GeometryNodePort _geometryInputPort;
        [HideInInspector] [SerializeField] protected GeometryNodePort _outputPort;

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            var serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new(serializedObject, nameof(_geometryInputPort), "", Direction.Input, Port.Capacity.Single),
                new(serializedObject, nameof(_outputPort), "", Direction.Output, Port.Capacity.Multi)
            };
        }
    }
}