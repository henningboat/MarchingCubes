using System.Linq;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public abstract class DistanceModifierInstruction : GeometryGraphInstruction
    {
        private GeometryGraphProperty[] _properties;
        private GeometryGraphProperty _transformation;
        private CombinerInstruction _combiner;
        protected abstract DistanceModificationType Type { get; }

        protected DistanceModifierInstruction(int depth, CombinerInstruction combiner, GeometryGraphProperty transformation,  params GeometryGraphProperty[] properties) : base(depth)
        {
            _combiner = combiner;
            _transformation = transformation;
            _properties = properties;
        }


        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.DistanceModification, (int) Type, Depth, _combiner.GetCombinerSetting(), _transformation, _properties.ToList());
        }
    }
}