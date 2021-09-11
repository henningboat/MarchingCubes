using System;
using System.Linq;
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

        protected override void OnDefineNode()
        {
            DataIn0 = this.AddDataInputPort<DistanceFieldValue>("in");
        }
    }

    public enum DistanceFieldValue
    {
    }
}