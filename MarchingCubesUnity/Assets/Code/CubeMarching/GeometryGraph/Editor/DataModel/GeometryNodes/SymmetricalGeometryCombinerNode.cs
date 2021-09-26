using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public abstract class SymmetricalGeometryCombinerNode : GeometryCombinerNode
    {
        protected abstract CombinerOperation CombinerOperation { get; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }
        public IPortModel BlendFactor { get; set; }

        public override void Resolve(GeometryGraphResolverContext context)
        {
            var blendFactorProperty = BlendFactor.ResolvePropertyInput(context, GeometryPropertyType.Float);
            context.BeginWriteCombiner(new CombinerInstruction(CombinerOperation, blendFactorProperty, context.CurrentCombinerDepth));
            GeometryInputA.ResolveGeometryInput(context);
            GeometryInputB.ResolveGeometryInput(context);

            context.FinishWritingCombiner();
        }

        public override string Title
        {
            get => CombinerOperation.ToString();
            set { }
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            GeometryInputA = this.AddDataInputPort<DistanceFieldValue>("", nameof(GeometryInputA), PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);
            GeometryInputB = this.AddDataInputPort<DistanceFieldValue>("", nameof(GeometryInputB), PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);

            BlendFactor = this.AddDataInputPort<float>("BlendFactor", nameof(BlendFactor), PortOrientation.Horizontal,
                CombinerOperation.HasBlendFactor() ? PortModelOptions.Default : PortModelOptions.Hidden);
        }
    }
}