using Code.CubeMarching.GeometryGraph;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.Utils;
using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct RuntimeGeometryGraphBuilder
    {
        private Entity _graphEntity;
        public ComponentDataFromEntity<CGeometryGraphInstance> GetGeometryGraphInstanceFromEntity;
        public BufferFromEntity<CGeometryGraphPropertyValue> GetPropertyValueBufferFromEntity;

        public RuntimeGeometryGraphBuilder(SystemBase system, Entity graphEntity)
        {
            _graphEntity = graphEntity;
            GetGeometryGraphInstanceFromEntity = system.GetComponentDataFromEntity<CGeometryGraphInstance>(false);
            GetPropertyValueBufferFromEntity = system.GetBufferFromEntity<CGeometryGraphPropertyValue>(false);
        }

        public void Execute(DynamicBuffer<GeometryInstruction> geometryInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition, bool isPlaying,
            DynamicBuffer<CGeometryGraphPropertyValue> valueBuffer)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
            geometryInstructions.Clear();
            valueBuffer.Clear();

            if (GetGeometryGraphInstanceFromEntity[_graphEntity].graph.IsCreated == false)
            {
                return;
            }

            var graphValueProperties = GetPropertyValueBufferFromEntity[_graphEntity];

            ref var geometryGraphBlob = ref GetGeometryGraphInstanceFromEntity[_graphEntity].graph.Value;
            for (var i = 0; i < geometryGraphBlob.geometryInstructions.Length; i++)
            {
                geometryInstructions.Add(geometryGraphBlob.geometryInstructions[i]);
            }

            for (var i = 0; i < graphValueProperties.Length; i++)
            {
                valueBuffer.Add(graphValueProperties[i]);
            }
        }
    }
}