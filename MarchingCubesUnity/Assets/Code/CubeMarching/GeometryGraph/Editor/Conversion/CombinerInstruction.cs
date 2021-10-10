using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class CombinerInstruction : GeometryGraphInstruction
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty Property;

        public CombinerInstruction(CombinerOperation operation, GeometryGraphProperty property, int currentCombinerDepth) : base(currentCombinerDepth)
        {
            Operation = operation;
            Property = property;
        }

        public CGeometryCombiner GetCombinerSetting()
        {
            return new()
            {
                Operation = Operation,
                BlendFactor = new FloatValue() {Index = Property.Index}
            };
        }

        public override GeometryInstruction GetInstruction()
        {
            var propertyIndexes = new int16();
            propertyIndexes[0] = Depth + 1;

            return new GeometryInstruction()
            {
                CombinerDepth = Depth,
                GeometryInstructionType = GeometryInstructionType.Combiner,
                Combiner = new CGeometryCombiner()
                {
                    Operation = Operation,
                    BlendFactor = new FloatValue() {Index = Property.Index}
                },
                PropertyIndexes = propertyIndexes
            };
        }
    }
}