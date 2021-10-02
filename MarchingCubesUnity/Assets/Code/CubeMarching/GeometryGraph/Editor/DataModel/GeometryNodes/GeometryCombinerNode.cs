using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public abstract class GeometryCombinerNode : NodeModel, IGeometryNode
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(null, nameof(GeometryOut));
        }

        public abstract void Resolve(GeometryGraphResolverContext context, GeometryTransformationInstruction parent);
    }
}