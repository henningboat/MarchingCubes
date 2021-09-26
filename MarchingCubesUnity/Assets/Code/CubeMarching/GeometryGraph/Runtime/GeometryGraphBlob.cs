using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public struct GeometryGraphBlob
    {
        public BlobArray<MathInstruction> mathInstructions;
        public BlobArray<GeometryInstruction> geometryInstructions;
        public BlobArray<CGeometryGraphPropertyValue> valueBuffer;
    }
}