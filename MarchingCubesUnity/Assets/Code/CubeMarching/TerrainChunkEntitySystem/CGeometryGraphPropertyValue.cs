using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct CGeometryGraphPropertyValue : IBufferElementData
    {
        public float Value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}