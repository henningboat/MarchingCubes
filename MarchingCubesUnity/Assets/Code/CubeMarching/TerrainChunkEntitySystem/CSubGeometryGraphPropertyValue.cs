using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct CMainGeometryGraphPropertyValue : IBufferElementData
    {
        public float Value;
    } 
    public struct CSubGeometryGraphPropertyValue : IBufferElementData
    {
        public float Value;
    }
}