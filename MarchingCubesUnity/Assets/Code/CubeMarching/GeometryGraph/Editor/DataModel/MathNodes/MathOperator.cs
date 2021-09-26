using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes
{
    [Serializable]
    public abstract class MathOperator : MathNode
    {
        [SerializeField] [HideInInspector] private int m_InputPortCount = 2;

        public abstract MathOperatorType OperatorType { get; }

        public List<float> Values => this.GetInputPorts().Select(portModel => portModel == null ? 0 : GetValue(portModel)).ToList();

        public override void ResetConnections()
        {
        }

        public int InputPortCount
        {
            get => m_InputPortCount;
            set => m_InputPortCount = Math.Max(2, value);
        }

        public IPortModel DataOut { get; private set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            DataOut = this.AddDataOutputPort<float>("Output");

            AddInputPorts();
        }

        protected abstract void AddInputPorts();

        public GeometryGraphProperty[] GetInputProperties(GeometryGraphResolverContext context, GeometryPropertyType propertyType)
        {
            var inputProperties = new List<GeometryGraphProperty>();

            var inputs = this.GetInputPorts().ToList();

            for (var i = 0; i < inputs.Count; i++)
            {
                inputProperties.Add(inputs[i].ResolvePropertyInput(context, propertyType));
            }

            return inputProperties.ToArray();
        }
    }
}