using System;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    [Serializable]
    public class GraphResult : NodeModel
    {
        public override string Title
        {
            get => "Result";
            set { }
        }

        public IPortModel DataIn { get; private set; }

        protected override void OnDefineNode()
        {
            DataIn = this.AddExecutionInputPort("", nameof(DataIn));
        }
    }
}