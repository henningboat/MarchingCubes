using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal class CombinerInstruction : GeometryGraphResolverContext.GeometryGraphInstruction
    {
        public readonly CombinerOperation Operation;
        public readonly GeometryGraphProperty Property;

        public CombinerInstruction(CombinerOperation operation, GeometryGraphProperty property, int currentCombinerDepth) : base(currentCombinerDepth)
        {
            Operation = operation;
            Property = property;
        }

        public override GeometryInstruction GetInstruction()
        {
            return new()
            {
                CombinerDepth = Depth,
                CoverageMask = BitArray512.AllBitsTrue,
                DependencyIndex = Depth + 1,
                TerrainShape = default,
                TerrainInstructionType = TerrainInstructionType.Combiner,
                Combiner = new CGeometryCombiner()
                {
                    Operation = Operation,
                    BlendFactor = 1
                }
            };
        }
    }
}