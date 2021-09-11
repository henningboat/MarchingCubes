using System;
using System.Diagnostics.SymbolStore;
using Code.CubeMarching;
using Code.CubeMarching.Authoring;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public class ShapeNode : NodeModel
    {
        public IPortModel GeometryOut { get; set; }
        public IPortModel PositionIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(nameof(GeometryOut));
            PositionIn = this.AddDataInputPort<Vector3>(nameof(PositionIn));
        }
    }


    public class SphereShapeNode : ShapeNode
    {
        public override string Title
        {
            get => "Sphere";
            set { }
        }

        public IPortModel RadiusIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue:  8);
        }
    }

    public abstract class GeometryCombinerNode : NodeModel
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(null, nameof(GeometryOut));
        }
    }

    public abstract class SymmetricalGeometryCombinerNode : GeometryCombinerNode
    {
        protected abstract CombinerOperation CombinerOperation { get; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }

        
        public override string Title
        {
            get => CombinerOperation.ToString();
            set { }
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            GeometryInputA = this.AddDataInputPort<DistanceFieldValue>(nameof(GeometryInputA), "null", PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);
            GeometryInputB = this.AddDataInputPort<DistanceFieldValue>(nameof(GeometryInputB), "null", PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);
        }
    }

    public class AdditionGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.Add;
    }
}