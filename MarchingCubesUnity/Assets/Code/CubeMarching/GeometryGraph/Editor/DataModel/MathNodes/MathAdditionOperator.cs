using System;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public class MathAdditionOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Addition;
    }

    [Serializable]
    public class MathSubtractionOperator : MathOperator
    {
        public override MathOperatorType OperatorType => MathOperatorType.Subtraction;
    }
}