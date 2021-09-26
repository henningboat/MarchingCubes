using Unity.Entities;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public struct CGeometryGraphInstance : IComponentData
    {
        public BlobAssetReference<GeometryGraphBlob> graph;
        public Entity OverwriteEntity;
    }
}