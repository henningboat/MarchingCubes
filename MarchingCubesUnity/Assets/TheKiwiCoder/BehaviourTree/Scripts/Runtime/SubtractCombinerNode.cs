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
            var serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new(serializedObject, nameof(_firstGeometryPort), "first", Direction.Input, Port.Capacity.Single),
                new(serializedObject, nameof(_otherGeometryPort), "other", Direction.Input, Port.Capacity.Multi),
                new(serializedObject, nameof(_outputPort), "", Direction.Output, Port.Capacity.Multi)
            };
        }
    }
}