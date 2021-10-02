using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

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

        protected abstract GeometryTransformationInstruction GetTransformationInstruction(GeometryGraphResolverContext context, GeometryTransformationInstruction parent);

        public void Resolve(GeometryGraphResolverContext context,GeometryTransformationInstruction parent)
        {
            _geometryInput.ResolveGeometryInput(context, GetTransformationInstruction(context, parent));
        }
    }
}