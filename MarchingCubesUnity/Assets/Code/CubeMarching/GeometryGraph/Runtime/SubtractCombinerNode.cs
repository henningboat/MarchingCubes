using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public class SubtractCombinerNode : CombinerNode
    {
        [HideInInspector] [SerializeField] private GeometryNodePort _firstGeometryInputPort;

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            var serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new(serializedObject, nameof(_firstGeometryInputPort), "first", Direction.Input, Port.Capacity.Single),
                new(serializedObject, nameof(_geometryInputPort), "other", Direction.Input, Port.Capacity.Multi),
                new(serializedObject, nameof(_outputPort), "", Direction.Output, Port.Capacity.Multi)
            };
        }
    }
}