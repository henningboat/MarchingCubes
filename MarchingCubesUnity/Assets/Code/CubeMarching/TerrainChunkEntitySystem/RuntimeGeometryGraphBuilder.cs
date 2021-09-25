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
        private readonly ComponentDataFromEntity<CGeometryGraphInstance> _getGeometryGraphInstanceFromEntity;

        public RuntimeGeometryGraphBuilder(SystemBase system, Entity graphEntity)
        {
            _graphEntity = graphEntity;
            _getGeometryGraphInstanceFromEntity = system.GetComponentDataFromEntity<CGeometryGraphInstance>();
        }

        public void Execute(DynamicBuffer<GeometryInstruction> geometryInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition, bool isPlaying,
            DynamicBuffer<CValueBufferEntry> valueBuffer)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
            geometryInstructions.Clear();
            valueBuffer.Clear();

            if (_getGeometryGraphInstanceFromEntity[_graphEntity].graph.IsCreated == false)
            {
                return;
            }

            ref var geometryGraphBlob = ref _getGeometryGraphInstanceFromEntity[_graphEntity].graph.Value;
            for (var i = 0; i < geometryGraphBlob.instructions.Length; i++)
            {
                geometryInstructions.Add(geometryGraphBlob.instructions[i]);
            }

            for (var i = 0; i < geometryGraphBlob.valueBuffer.Length; i++)
            {
                valueBuffer.Add(new CValueBufferEntry() {Value = geometryGraphBlob.valueBuffer[i]});
            }
        }
    }

    public struct CValueBufferEntry : IBufferElementData
    {
        public float Value;
    }
}