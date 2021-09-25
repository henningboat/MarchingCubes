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

        public IPortModel DataIn { get; private set; }

        protected override void OnDefineNode()
        {
            DataIn = this.AddDataInputPort<DistanceFieldValue>("", nameof(DataIn));
        }
    }

    public enum DistanceFieldValue
    {
    }
}