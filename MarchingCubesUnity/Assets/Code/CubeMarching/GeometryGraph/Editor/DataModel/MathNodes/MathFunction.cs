using System;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathFunction : MathNode
    {
        [SerializeField] protected string[] m_ParameterNames = new string[0];

        public override void ResetConnections()
        {
        }

        public IPortModel DataOut0 { get; private set; }

        protected override void OnDefineNode()
        {
            foreach (var name in m_ParameterNames)
            {
                this.AddDataInputPort<float>(name);
            }

            DataOut0 = this.AddDataOutputPort<float>("out");
        }
    }
}