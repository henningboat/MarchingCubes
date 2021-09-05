using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TheKiwiCoder
{
    public class SubtractCombinerNode : CombinerNode
    {
        [SerializeField] private GeometryNodePort _firstGeometryPort;
        [SerializeField] private GeometryNodePort _otherGeometryPort;
        [SerializeField] private GeometryNodePort _outputPort;

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new GeometryNodePortDescription(serializedObject, nameof(_firstGeometryPort), Direction.Input, Port.Capacity.Single),
                new GeometryNodePortDescription(serializedObject, nameof(_otherGeometryPort), Direction.Input, Port.Capacity.Multi),
                new GeometryNodePortDescription(serializedObject, nameof(_outputPort), Direction.Output, Port.Capacity.Multi),
            };
        }
    }
}