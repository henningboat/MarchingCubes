using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [GenerateAuthoringComponent]
    public struct CGeometryGraphPropertyOverwriteProvider : IComponentData
    {
        public float4x4 Value;
    }
}