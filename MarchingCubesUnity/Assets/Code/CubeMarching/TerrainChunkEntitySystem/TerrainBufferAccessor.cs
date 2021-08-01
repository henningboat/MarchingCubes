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
        public readonly TotalClusterCounts ClusterCounts;

        private const int clusterLength = 64;
        private const int chunkLength = 8;
        private const int chunksInCluster = 512;

        public TerrainBufferAccessor(SystemBase systemBase)
        {
            unsafe
            {
                DataBuffer = systemBase.GetSingletonBuffer<TerrainChunkDataBuffer>().AsNativeArray().Reinterpret<PackedTerrainData>(sizeof(TerrainChunkDataBuffer));
                IndexBuffer = systemBase.GetSingletonBuffer<TerrainChunkIndexMap>();
                ClusterCounts = systemBase.GetSingleton<TotalClusterCounts>();
            }
        }

        public float GetSurfaceDistance(int3 positionWS)
        {
            positionWS = clamp(positionWS, 0, ClusterCounts.Value * clusterLength);

            int chunkIndex = IndexBuffer[Utils.PositionToIndex(positionWS / 8, ClusterCounts.Value * (clusterLength / chunkLength))].Index;

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