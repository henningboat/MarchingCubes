using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public class OutputNode : GeometryNode
    {
        [FormerlySerializedAs("_outputPort")] [HideInInspector] [SerializeField]
        private GeometryNodePort _inputPort;

        public GeometryNode GetInputNode(GeometryTree tree)
        {
            return _inputPort.GetFirstConnection(tree);
        }

        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            return new()
            {
                new GeometryNodePortDescription(new SerializedObject(this), nameof(_inputPort), "", Direction.Input, Port.Capacity.Single)
            };
        }

        public override NodePort GetOutputPort()
        {
            throw new NotImplementedException();
        }
    }
}