using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class OnionDistanceModifierInstruction : DistanceModifierInstruction
    {
        private GeometryGraphProperty _thickness;

        public OnionDistanceModifierInstruction(GeometryGraphProperty thickness, GeometryGraphResolverContext context) : base(context.CurrentCombinerDepth)
        {
            _thickness = thickness;
        }
        public override GeometryInstruction GetInstruction()
        {
            throw new NotImplementedException();
        }
    }
}