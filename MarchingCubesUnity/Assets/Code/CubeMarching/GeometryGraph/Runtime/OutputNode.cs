using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TheKiwiCoder
{
    public class OutputNode : GeometryNode
    {
        [SerializeField] private GeometryNodePort _outputPort;

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            return new()
            {
                new GeometryNodePortDescription(new SerializedObject(this), nameof(_outputPort), "", Direction.Input, Port.Capacity.Single)
            };
        }
    }
}