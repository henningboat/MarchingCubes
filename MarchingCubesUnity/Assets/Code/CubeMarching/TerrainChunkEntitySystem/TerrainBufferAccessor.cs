using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct TerrainBufferAccessor
    {
        public readonly NativeArray<PackedTerrainData> DataBuffer;
        public readonly DynamicBuffer<TerrainChunkIndexMap> IndexBuffer;
        public readonly TotalClustersCount ClustersCount;

        private const int clusterLength = 64;
        private const int chunkLength = 8;
        private const int chunksInCluster = 512;

        public TerrainBufferAccessor(SystemBase systemBase)
        {
            unsafe
            {
                DataBuffer = systemBase.GetSingletonBuffer<TerrainChunkDataBuffer>().AsNativeArray().Reinterpret<PackedTerrainData>(sizeof(TerrainChunkDataBuffer));
                IndexBuffer = systemBase.GetSingletonBuffer<TerrainChunkIndexMap>();
                ClustersCount = systemBase.GetSingleton<TotalClustersCount>();
            }
        }

        public float GetSurfaceDistance(int3 positionWS)
        {
            positionWS = clamp(positionWS, 0, ClustersCount.Value * clusterLength);

            int chunkIndex = IndexBuffer[Utils.PositionToIndex(positionWS / 8, ClustersCount.Value * (clusterLength / chunkLength))].Index;

            var surfaceDistance = GetPointPosition(positionWS % chunkLength, chunkIndex);
            return surfaceDistance;
        }

        private  float GetPointPosition(int3 positionWithinTerrainChunk, int chunkIndex)
        {
            int subChunkIndex = Utils.PositionToIndex(positionWithinTerrainChunk / 4, 2);
            int indexWithinSubChunk = Utils.PositionToIndex(positionWithinTerrainChunk % 4, 4);
            var indexWithinTerrainChunk = subChunkIndex * 64 + indexWithinSubChunk;

            var indexInTerrainBuffer = indexWithinTerrainChunk+chunkIndex*512;
            float surfaceDistance = DataBuffer[indexInTerrainBuffer / 4].SurfaceDistance.PackedValues[indexInTerrainBuffer % 4];
            return surfaceDistance;
        }
    }
}