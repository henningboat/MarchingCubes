using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [GenerateAuthoringComponent]
    public struct CGeometryGraphPropertyOverwriteProvider : IComponentData
    {
        public Float16 Value;
    }
}