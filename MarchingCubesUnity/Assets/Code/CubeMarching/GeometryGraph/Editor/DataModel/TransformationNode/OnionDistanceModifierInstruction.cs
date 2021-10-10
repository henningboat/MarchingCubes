using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class OnionDistanceModifierInstruction : DistanceModifierInstruction
    {
        public OnionDistanceModifierInstruction(GeometryGraphProperty thickness, GeometryGraphResolverContext context, GeometryGraphProperty transformation) :
            base(context.CurrentCombinerDepth, context.CurrentCombiner, transformation, thickness)
        {
        }

        protected override DistanceModificationType Type => DistanceModificationType.Onion;
    }
}