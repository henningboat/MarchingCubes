using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public class MathMultiplicationOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Multiplication;

        protected override void AddInputPorts()
        {
            for (var i = 0; i < InputPortCount; ++i)
            {
                this.AddDataInputPort<float>("Term " + (i + 1));
            }
        }
    }
}