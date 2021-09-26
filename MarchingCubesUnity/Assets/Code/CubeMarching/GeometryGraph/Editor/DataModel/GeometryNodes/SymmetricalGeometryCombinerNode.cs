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

        public override void Resolve(GeometryGraphResolverContext context)
        {
            context.BeginWriteCombiner(new CGeometryCombiner() {Operation = CombinerOperation});
            GeometryInputA.ResolveGeometryInput(context);
            GeometryInputB.ResolveGeometryInput(context);
            context.FinishWritingCombiner(CombinerOperation, new GeometryGraphConstantProperty((object) 0f, context, GeometryPropertyType.Float, "Zero"));
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
        }
    }
}