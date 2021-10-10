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
            _geometryInput = this.AddExecutionInputPort("", nameof(_geometryInput));
            _geometryOutput = this.AddExecutionOutputPort("", nameof(_geometryInput));
        }

        protected abstract GeometryGraphProperty GetTransformationInstruction(GeometryGraphResolverContext context, GeometryGraphProperty parent);

        public void Resolve(GeometryGraphResolverContext context,GeometryGraphProperty transformation)
        {
            _geometryInput.ResolveGeometryInput(context, GetTransformationInstruction(context, transformation));
        }
    }
}