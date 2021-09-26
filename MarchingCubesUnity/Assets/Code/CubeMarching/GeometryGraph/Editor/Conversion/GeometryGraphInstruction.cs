using Code.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public abstract class GeometryGraphInstruction
    {
        public readonly int Depth;

        protected GeometryGraphInstruction(int depth)
        {
            Depth = depth;
        }

        public abstract GeometryInstruction GetInstruction();
    }
}