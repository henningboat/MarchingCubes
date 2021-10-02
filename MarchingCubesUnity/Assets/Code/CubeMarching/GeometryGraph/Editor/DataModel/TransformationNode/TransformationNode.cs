using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public abstract class TransformationNode : NodeModel, IGeometryNode
    {
        private IPortModel _geometryInput;
        private IPortModel _geometryOutput;

        protected override void OnDefineNode()
        {
            _geometryInput = this.AddDataInputPort<DistanceFieldValue>("", nameof(_geometryInput));
            _geometryOutput = this.AddDataOutputPort<DistanceFieldValue>("", nameof(_geometryInput));
        }

        protected abstract TransformationInstruction ResolveInstruction(GeometryGraphResolverContext geometryGraphResolverContext);

        public void Resolve(GeometryGraphResolverContext context)
        {
            context.WriteTransformation(ResolveInstruction(context));
            _geometryInput.ResolveGeometryInput(context);
        }
    }
}