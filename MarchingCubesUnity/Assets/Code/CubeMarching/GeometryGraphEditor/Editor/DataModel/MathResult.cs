using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public class MathResult : NodeModel
    {
        public override string Title
        {
            get => "Result";
            set { }
        }

        public float Evaluate()
        {
            var inputPorts = this.GetInputPorts();
            var port = inputPorts.FirstOrDefault(model => model.UniqueName == "in");

            return port.GetValue();
        }

        public IPortModel DataIn0 { get; private set; }
        public IPortModel DataIn1 { get; private set; }
        public IPortModel DataIn2 { get; private set; }
        public IPortModel DataIn3 { get; private set; }

        protected override void OnDefineNode()
        {
            DataIn0 = this.AddDataInputPort<DistanceFieldValue>("",nameof(DataIn0));
            DataIn1 = this.AddDataInputPort<DistanceFieldValue>("",nameof(DataIn1));
            DataIn2 = this.AddDataInputPort<DistanceFieldValue>("",nameof(DataIn2));
            DataIn3 = this.AddDataInputPort<DistanceFieldValue>("",nameof(DataIn3));
        }

        public void AddInstructions(List<GeometryInstruction> results)
        {
            foreach (var portModel in GetConnectedEdges())
            {
                var connection = portModel.FromPort.NodeModel;
                if (connection is IShapeNode shapeNode)
                {
                    results.Add(shapeNode.GetTerrainInstruction());
                }   
            }
        }
    }

    public enum DistanceFieldValue
    {
    }
}