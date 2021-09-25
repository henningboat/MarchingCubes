using System;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
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
