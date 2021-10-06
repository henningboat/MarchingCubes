using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public struct GeometryGraphBlob
    {
        public BlobArray<CSubGeometryGraphPropertyValue> valueBuffer;
        public BlobArray<MathInstruction> mathInstructions;
        public BlobArray<GeometryInstruction> geometryInstructions;
    }
}