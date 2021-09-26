using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathNode : NodeModel
    {
        public float GetValue(IPortModel port)
        {
            return port.GetValue();
        }

        public abstract float Evaluate();

        public abstract void ResetConnections();
    }
}