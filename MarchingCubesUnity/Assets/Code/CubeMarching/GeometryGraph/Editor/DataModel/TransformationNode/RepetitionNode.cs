using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    [UsedImplicitly]
    public class RepetitionNode : TransformationNode
    {
        private IPortModel _periodInput;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _periodInput = this.AddDataInputPort("", nameof(_periodInput), defaultValue: Vector3.one * 8);
        }

        protected override TransformationInstruction ResolveInstruction(GeometryGraphResolverContext context)
        {
            return new(TerrainTransformationType.Repetition, context.CurrentCombinerDepth, _periodInput.ResolvePropertyInput(context, GeometryPropertyType.Float3));
        }
    }
}